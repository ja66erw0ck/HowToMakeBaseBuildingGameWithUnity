using System;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using JobModel = Model.Job;
using TileModel = Model.Tile;
using ItemModel = Model.Item;
using WorldModel = Model.World;
using StructureModel = Model.Structure;
using StructureAction = Model.Action.Structure;

namespace Model
{
    [MoonSharpUserData]
    public sealed class Job : global::Model.Interface.IJsonSerializable
    {
        // This class holds info for a queued up job, which can include
        // things like placing structure, moving stored inventory,
        // working at a desk, and maybe even fighting enemies.
       public TileModel Tile { get; set; }
       public float JobTime { get; private set; }

       private readonly float _jobTimeRequired;
       private readonly bool _isRepeat = false;
       
       // FIXME: Hard-coding a parameter for structure. do not like.
       public string Type { get; protected set; }

       public StructureModel StructurePrototype;

       public StructureModel Structure; // the piece of structure that owns this job. frequently will be null.

       private const bool AcceptsAnyInventoryItem = false;

       private Action<JobModel> _callbackJobCompleted; // we have finished the work cycle and so things should probably get built or whatever.
       private Action<JobModel> _callbackJobStopped; // job has been stopped, either because it's non-repeating or was cancelled.
       private Action<JobModel> _callbackJobWorked; // gets called each time some work is performed -- maybe update the UI?
       
       private readonly List<string> _callbackJobCompletedLua;
       private readonly List<string> _callbackJobStoppedLua;
       private readonly List<string> _callbackJobWorkedLua;

       public const bool CanTakeFromStockpile = true;

       public readonly Dictionary<string, ItemModel> ItemRequirements;

       public Job(TileModel tile, string type, Action<JobModel> callbackJobCompleted, float jobTime, ItemModel[] itemRequirements, bool isRepeat = false)
       {
           Tile = tile;
           Type = type;
           _jobTimeRequired = JobTime = jobTime;
           _isRepeat = isRepeat;

           if (callbackJobCompleted != null) {
               _callbackJobCompleted += callbackJobCompleted;
           }

           _callbackJobCompletedLua = new List<string>();
           _callbackJobStoppedLua = new List<string>();
           _callbackJobWorkedLua = new List<string>();

           ItemRequirements = new Dictionary<string, ItemModel>();
           if (itemRequirements == null) {
               return;
           }

           foreach (var item in itemRequirements) {
               ItemRequirements[item.Type] = item.Clone();
           }
       }

       private Job(JobModel other)
       {
           Tile = other.Tile;
           Type = other.Type;
           JobTime = other.JobTime;
           _jobTimeRequired = other._jobTimeRequired;
           _isRepeat = other._isRepeat;
           
           _callbackJobCompleted += other._callbackJobCompleted;
           
           _callbackJobCompletedLua = new List<string>(other._callbackJobCompletedLua);
           _callbackJobStoppedLua = new List<string>(other._callbackJobStoppedLua);
           _callbackJobWorkedLua = new List<string>(other._callbackJobWorkedLua);
           
           StructurePrototype = other.StructurePrototype;

           ItemRequirements = new Dictionary<string, ItemModel>();
           if (other.ItemRequirements == null) {
               return;
           }

           foreach (var item in other.ItemRequirements.Values) {
               ItemRequirements[item.Type] = item.Clone();
           }
       }

       public JobModel Clone()
       {
           return new JobModel(this);
       }

       public void RegisterJobCompleted(Action<JobModel> callback)
       {
           _callbackJobCompleted += callback;
       }
       
       public void UnregisterJobCompleted(Action<JobModel> callback)
       {
           _callbackJobCompleted -= callback;
       }

       public void RegisterJobStopped(Action<JobModel> callback)
       {
           _callbackJobStopped += callback;
       }

       public void UnregisterJobStopped(Action<JobModel> callback)
       {
           _callbackJobStopped -= callback;
       }

       public void RegisterJobWorked(Action<JobModel> callback)
       {
           _callbackJobWorked += callback;
       }

       public void UnregisterJobWorked(Action<JobModel> callback)
       {
           _callbackJobWorked -= callback;
       }

       public void RegisterJobCompleted(string callback)
       {
           _callbackJobCompletedLua.Add(callback);
       }
       
       public void UnregisterJobCompleted(string callback)
       {
           _callbackJobCompletedLua.Remove(callback);
       }

       public void RegisterJobStopped(string callback)
       {
           _callbackJobStoppedLua.Add(callback);
       }

       public void UnregisterJobStopped(string callback)
       {
           _callbackJobStoppedLua.Remove(callback);
       }

       public void RegisterJobWorked(string callback)
       {
           _callbackJobWorkedLua.Add(callback);
       }

       public void UnregisterJobWorked(string callback)
       {
           _callbackJobWorkedLua.Remove(callback);
       }

       private void JobWorked()
       { 
           _callbackJobWorked?.Invoke(this);
           
           if (_callbackJobWorkedLua == null) {
               return;
           }

           foreach (var luaFunction in _callbackJobWorkedLua) {
               StructureAction.CallFunction(luaFunction, this);
           }
       }

       private void JobCompleted()
       {
           _callbackJobCompleted?.Invoke(this);
           
           if (_callbackJobCompletedLua == null) {
               return;
           }

           foreach (var luaFunction in _callbackJobCompletedLua) {
               StructureAction.CallFunction(luaFunction, this);
           }
       }

       private void JobStopped()
       {
           _callbackJobStopped?.Invoke(this);
           
           if (_callbackJobStoppedLua == null) {
               return;
           }

           foreach (var luaFunction in _callbackJobStoppedLua) {
               StructureAction.CallFunction(luaFunction, this);
           }
       }

       public void DoWork(float workTime)
       {
           // check to make sure we actually have everything we need.
           // if not, don't register the work time.
           if (!HasAllMaterial()) {
               //Debug.LogError("Tried to do work on a job that doesn't have all the material.");
       
               JobWorked();
               return;
           }
           
           JobTime -= workTime;

           JobWorked();

           if (!(JobTime <= 0f)) {
               return;
           }

           // do whatever is supposed to happen with a job cycle completes.
           JobCompleted();

           if (!_isRepeat) {
               JobStopped();
           } else {
               // this is a repeating job and must be rest.
               JobTime += _jobTimeRequired;
           }
       }

       public void CancelJob()
       {
           JobStopped();
           WorldModel.Current.JobManager.Remove(this);
       }

       public bool HasAllMaterial()
       {
           return ItemRequirements.Values.All(item => item.MaxStackSize <= item.StackSize);
       }

       public int DesiresItemType(ItemModel item)
       {
           if (AcceptsAnyInventoryItem) {
               return item.MaxStackSize;
           } 
           
           if (!ItemRequirements.ContainsKey(item.Type)) {
               return 0;
           }

           if (ItemRequirements[item.Type].StackSize >= ItemRequirements[item.Type].MaxStackSize) {
               // we already have all that we need!
               return 0;
           }

           // the inventory is of a type we want, and we still need more.
           return ItemRequirements[item.Type].MaxStackSize - ItemRequirements[item.Type].StackSize;
       }

       public ItemModel GetFirstDesiredItem()
       {
           return ItemRequirements.Values.FirstOrDefault(item => item.MaxStackSize > item.StackSize);
       }
       
       /////////////////////////////////////////////////////////////////
       /// IJsonSerializable
       /////////////////////////////////////////////////////////////////

       public void FromJson(JToken token)
       {
       }

       public JToken ToJson()
       {
           return new JObject {
               { "X", Tile.X },
               { "Y", Tile.Y },
               { "Z", Tile.Z },
               { "Type", Type },
               { "JobTimeRequired", _jobTimeRequired },
               { "IsRepeat", _isRepeat },
           };
       }
    }
}
