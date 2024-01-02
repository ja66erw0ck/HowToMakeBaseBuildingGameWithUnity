using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Model.Interface;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using UnityEngine;
using EnterableType = Type.Enterable;
using TileType = Type.Tile;
using JobModel = Model.Job;
using ItemModel = Model.Item;
using TileModel = Model.Tile;
using WorldModel = Model.World;
using StructureModel = Model.Structure;
using StructureAction = Model.Action.Structure;
using TimeManager = Manager.Time;

namespace Model
{
    // structure are things like walls, doors, and structure (e.g. a sofa)
    [MoonSharpUserData]
    public class Structure : IJsonSerializable, global::Model.Interface.ISelectable
    {
        // custom parameter for this particular piece of structure. we are
        // using a dictionary because later, custom LUA function will be
        // able to use whatever parameters the user/modder would like.
        // basically, the LUA code will bind to this dictionary.
        private readonly Dictionary<string, float> _parameters;
        private readonly Dictionary<string, string> _sprites;

        // these actions are called every update. they get passed the structure
        // they belong to, plus a deltatime
        private readonly List<string> _updateAction;
        private string _isEnterable;

        private readonly List<string> _replaceableStructure = new List<string>();

        public readonly List<JobModel> _jobs;

        // if this structure gets worked by a person.
        // where is the correct spot for them to stand,
        // relative to the botton-left tile of the sprite.
        // NOTE: this could even be something outside of the actual
        // structure tile itself! (in fact, this will probably be common).
        private Vector2 _jobSpotOffset = Vector2.zero;

        // if the job causes some kind of object to be spawned, where will it appear?
        private Vector2 _jobSpawnSpotOffset = Vector2.zero;

        // sprites
        private IEnumerable<string> ReplaceableStructure => _replaceableStructure;

        // this represents the base tile of the object -- but in practice, large objects may actually occupy multi tiles
        public TileModel Tile { get; private set; }

        // will be queried by the visual system to know what sprite to render for this object
        public string Type { get; private set; }

        private string _name = null;
        public string Name {
            get => string.IsNullOrEmpty(_name) ? Type : _name;
            private set => _name = value;
        }

        // this is a multipler, so a value of "2" here, means you move twice as slowly
        // tile types an other enviromental effects may be combined.
        // for example, a "rough" tile (cost of 2) with a table (cost of 3) that is on fire (cost of 3)
        // would have a total movement cost of  (2+3+3=8), so you'd move through this tile at 1/8th normal speed.
        // special !!! if movementconst = 0, the this tile is impassible. (e.g. a wall).
        public float MovementCost { get; set; }

        public bool IsRoomEnclosure { get; protected set; }

        // for example, a soft might be 3x2 (actual graphics only appear to cover the 3x1 aprea,
        // but the extra row is for leg room
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Color Tint { get; private set; }

        private bool IsLinkToNeighbour { get; set; }

        private readonly System.Func<TileModel, bool> _funcIsValidatePosition;

        // empty constructor is used for serialization
        public Structure()
        {
            _parameters = new Dictionary<string, float>();
            _sprites = new Dictionary<string, string>();
            _jobs = new List<JobModel>();
            
            Tint = Color.white;
            MovementCost = 1f;
            Width = 1;
            Height = 1;
            IsLinkToNeighbour = false;
            IsRoomEnclosure = false;

            _updateAction = new List<string>();
            
            _funcIsValidatePosition = DEFAULT___IsValidPosition;
            
            TimeManager.Instance.EveryFrameUnpaused += Update;
        }

        // copy constructor
        private Structure(Structure other)
        {
            Type = other.Type;
            Name = other.Name;
            MovementCost = other.MovementCost;
            IsRoomEnclosure = other.IsRoomEnclosure;
            Width = other.Width;
            Height = other.Height;
            Tint = other.Tint;
            IsLinkToNeighbour = other.IsLinkToNeighbour;
            IsRoomEnclosure = other.IsRoomEnclosure;

            _jobSpotOffset = other._jobSpotOffset;
            _jobSpawnSpotOffset = other._jobSpawnSpotOffset;

            _parameters = new Dictionary<string, float>(other._parameters);
            _sprites = new Dictionary<string, string>(other._sprites);
            _jobs = new List<JobModel>(); 
            
            _updateAction = new List<string>(other._updateAction);

            if (other._isEnterable != null) {
                _isEnterable = other._isEnterable;
            }
            
            if (other._funcIsValidatePosition != null) {
                _funcIsValidatePosition = (System.Func<TileModel, bool>) other._funcIsValidatePosition.Clone();
            }
            
            TimeManager.Instance.EveryFrameUnpaused += Update;
        }

        ~Structure()
        {
            TimeManager.Instance.EveryFrameUnpaused -= Update;
        }

        // make a copy of the current structure. sub-classed should
        // override this Clone() if a different (sub-classed) copy
        // constructor should be run.
        private Structure Clone()
        {
            return new Structure(this);
        }        
        
        //////////////////////////////
        /// Update
        //////////////////////////////

        private void Update(float deltaTime)
        {
            if (Tile == null) {
                return;
            }
           
            StructureAction.CallFunctionsWithStructure(_updateAction.ToArray(), this, deltaTime);
        }

        public float GetParameter(string key, float defaultValue = 0)
        {
            return _parameters.ContainsKey(key) == false ? defaultValue : _parameters[key];
        }
        
        public void SetParameter(string key, float value)
        {
            _parameters[key] = value;
        }
        
        public void ChangeParameter(string key, float value)
        {
            _parameters[key] += value;
        }

        // TODO: implement larger object
        // TODO : implment object rotation
        
        public static Structure Place(Structure prototype, TileModel tile)
        {
            if (prototype._funcIsValidatePosition(tile) == false) {
                Debug.Log("! Place -- position validity function returned false");
                return null;
            }

            // we know our placement destination is valid
            var structure = prototype.Clone();
            structure.Tile = tile;

            // FIXME: this assumes we are 1x1!!!
            if (tile.PlaceStructure(structure) == false) {
                return null;
            }

            if (structure.IsLinkToNeighbour != true) {
                return structure;
            }
            // this type of structure links itself to its neighbours,
            // so we should inform our neighbours that they have a new
            // buddy. just trigger their onChangedCallback
                
            var neighbourTile = tile.West;
            if (neighbourTile is { Structure: { } } && neighbourTile.Structure.Type == structure.Type) {
                // we have a norther neighbour with the same object type as us, so
                // tell it that it has changed by firing is callback.
                neighbourTile.Structure.CallbackOnChanged?.Invoke(neighbourTile.Structure);
            }

            neighbourTile = tile.East;
            if (neighbourTile is { Structure: { } } && neighbourTile.Structure.Type == structure.Type) {
                neighbourTile.Structure.CallbackOnChanged?.Invoke(neighbourTile.Structure);
            }

            neighbourTile = tile.North;
            if (neighbourTile is { Structure: { } } && neighbourTile.Structure.Type == structure.Type) {
                neighbourTile.Structure.CallbackOnChanged?.Invoke(neighbourTile.Structure);
            }

            neighbourTile = tile.South;
            if (neighbourTile is { Structure: { } } && neighbourTile.Structure.Type == structure.Type) {
                neighbourTile.Structure.CallbackOnChanged?.Invoke(neighbourTile.Structure);
            }

            return structure;
        }
       
        /// Callback
        
        public System.Action<Structure> CallbackOnChanged; // using in lua script
        public System.Action<Structure> CallbackOnRemoved; // using in lua script

        public void RegisterOnChangedCallback(System.Action<StructureModel> callback)
        {
            CallbackOnChanged += callback;
        }
        
        public void UnregisterOnChangedCallback(System.Action<StructureModel> callback)
        {
            CallbackOnChanged -= callback;
        }
        
        public void RegisterOnRemovedCallback(System.Action<StructureModel> callback)
        {
            CallbackOnRemoved += callback;
        }
        
        public void UnregisterOnRemovedCallback(System.Action<StructureModel> callback)
        {
            CallbackOnRemoved -= callback;
        }
       
        public bool IsValidPosition(TileModel tile)
        {
            return _funcIsValidatePosition(tile);
        }
        
        //
        public EnterableType IsEnterable()
        {
            if(string.IsNullOrEmpty(_isEnterable)) {
                return EnterableType.Yes;
            }

            var result = StructureAction.CallFunction(_isEnterable, this);

            return (EnterableType)result.Number;
        }
       
        // FIXME : these functions should never be called directly.
        // so they probably shouldn't be public functions of Structure
        // this will be replaced by validation checks fed to use from
        // lua files that will be customizable for each piece of structuree.
        // for example, a door might specific that it needs two walls to  conenct to
        private bool DEFAULT___IsValidPosition(TileModel tile)
        {
            for (var xOffset = tile.X; xOffset < (tile.X + Width); xOffset++) {
                for (var yOffset = tile.Y; yOffset < (tile.Y + Height); yOffset++) {
                    var target = WorldModel.Current.GetTileModelAt(xOffset, yOffset, tile.Z);
                    
                    // check to see if there is strcuture which is replacable
                    var isReplaceabe = false;

                    if (target.Structure != null) {
                        if (ReplaceableStructure.Any(t => target.Structure.Name == t)) {
                            isReplaceabe = true;
                        }
                    }
                    
                    // make sure tile is floor
                    if (target.Type != TileType.Floor) {
                        return false;
                    }

                    // make sure tile doesn't already have structure
                    if (target.Structure != null && !isReplaceabe) {
                        return false;
                    }
                }
            }
            
            return true;
        }

        // registers a function that will be called every updates
        // later this implementation might change a bit as we support LUA.
        private void RegisterUpdateAction(string luaFunctionName)
        {
            _updateAction.Add(luaFunctionName);
        }

        public void UnregisterUpdateAction(string luaFunctionName)
        {
            _updateAction.Remove(luaFunctionName);
        }

        public int JobCount()
        {
            return _jobs.Count;
        }

        public void AddJob(JobModel job)
        {
            job.Structure = this;
            _jobs.Add(job);
            job.RegisterJobStopped(OnJobStopped);
            WorldModel.Current.JobManager.Enqueue(job);
        }

        private void OnJobStopped(JobModel job)
        {
            RemoveJob(job);    
        }

        private void RemoveJob(JobModel job)
        {
            job.UnregisterJobStopped(OnJobStopped);
            _jobs.Remove(job);
            job.Structure = null;
            //WorldModel.Current.JobQueue.Remove(job);
        }

        private void ClearJobs()
        {
            var jobs = _jobs.ToArray(); 
            foreach (var job in jobs) {
                RemoveJob(job);
            }
        }

        public void CancelJobs()
        {
            var jobs = _jobs.ToArray(); 
            foreach (var job in jobs) {
                job.CancelJob();
            }
        }

        public bool IsStockpile()
        {
            return Type == "Stockpile";
        }

        public void Deconstruct()
        {
            WorldModel.Current.StructureManager.Remove(this);
            Tile.UnPlaceStructure();
            CallbackOnRemoved?.Invoke(this);
            
            // do we need to recalculate our rooms?
            if (IsRoomEnclosure) {
                Room.DoRoomFloodFill(Tile);    
            }
            
            WorldModel.Current.InvalidateTileGraph();    
            
            // at his point, no data structures should be pointing to us,
            // so we should get garbage-collected.
        }

        public TileModel GetJobSpotTile()
        {
            return WorldModel.Current.GetTileModelAt(Tile.X + (int)_jobSpotOffset.x, Tile.Y + (int)_jobSpotOffset.y, Tile.Z);
        }
        
        public TileModel GetJobSpawnSpotTile()
        {
            return WorldModel.Current.GetTileModelAt(Tile.X + (int)_jobSpawnSpotOffset.x, Tile.Y + (int)_jobSpawnSpotOffset.y, Tile.Z);
        }

        public void ReadXmlParameters(XmlReader reader)
        {
            if (reader.ReadToDescendant("Parameter") == false) {
                return;
            }

            do {
                var key = reader.GetAttribute("Name");
                var value = float.Parse(reader.GetAttribute("Value"));
                _parameters[key] = value;
            } while (reader.ReadToNextSibling("Parameters"));
        }

        public void ReadXmlPrototype(XmlReader parent)
        {
            Type = parent.GetAttribute("Type"); 
            var reader = parent.ReadSubtree();
            
            while (reader.Read()) {
                switch (reader.Name) {
                    case "Name":
                        reader.Read();
                        Name = reader.ReadContentAsString();
                        break;
                    case "MovementCost":
                        reader.Read();
                        MovementCost = reader.ReadContentAsFloat();
                        break;
                    case "Width":
                        reader.Read();
                        Width = reader.ReadContentAsInt();
                        break;
                    case "Height":
                        reader.Read();
                        Height = reader.ReadContentAsInt();
                        break;
                    case "IsLinkToNeighbour":
                        reader.Read();
                        IsLinkToNeighbour = reader.ReadContentAsBoolean();
                        break;
                    case "IsRoomEnclosure":
                        reader.Read();
                        IsRoomEnclosure = reader.ReadContentAsBoolean();
                        break;
                    case "Tint":
                        reader.Read();
                        var tint = reader.ReadContentAsString().Split(',');
                        Tint = new Color32(byte.Parse(tint[0]), byte.Parse(tint[1]), byte.Parse(tint[2]), byte.Parse(tint[3]));
                        break;
                    case "Parameters":
                        ReadXmlParameters(reader); // read in the param tag
                        break;
                    case "CanReplaceStructure":
                        _replaceableStructure.Add(reader.GetAttribute("ObjectName").ToString());
                        break;
                    case "BuildingJob":
                        var jobTime = float.Parse(reader.GetAttribute("JobTime"));
                        var items = new List<ItemModel>();
                        
                        var itemsReader = reader.ReadSubtree();
                        while (itemsReader.Read()) {
                            if (itemsReader.Name == "Item") {
                                // found an item requirement, so add it to the list!
                                items.Add(new ItemModel(
                                        itemsReader.GetAttribute("Type"),
                                        int.Parse(itemsReader.GetAttribute("Amount")),
                                        0)
                                );
                            }
                        }
                        
                        var job = new JobModel(null, Type, StructureAction.CompleteJobStructure, jobTime, items.ToArray());
                        
                        WorldModel.Current.RegisterStructureJobPrototype(job, this);
                        break;
                    case "OnUpdate":
                        var updateFunction = reader.GetAttribute("Function");
                        RegisterUpdateAction(updateFunction);
                        break;
                    case "IsEnterable":
                        _isEnterable = reader.GetAttribute("Function");
                        break;
                    case "JobSpotOffset":
                        _jobSpotOffset = new Vector2(
                            int.Parse(reader.GetAttribute("X")),
                            int.Parse(reader.GetAttribute("Y"))
                        );
                        break;
                    case "JobSpawnSpotOffset":
                        _jobSpawnSpotOffset = new Vector2(
                            int.Parse(reader.GetAttribute("X")),
                            int.Parse(reader.GetAttribute("Y"))
                        );
                        break;
                    case "DeconstructJob":
                        /*
                        var jobTime = float.Parse(reader.GetAttribute("JobTime"));
                        var items = new List<ItemModel>();
                        
                        var itemsReader = reader.ReadSubtree();
                        while (itemsReader.Read()) {
                            items.Add(
                                new ItemModel(
                                    itemsReader.GetAttribute("Type"),
                                    int.Parse(itemsReader.GetAttribute("Amount")),
                                    0
                                )
                            );
                        }
                        var job = new JobModel(null, ObjectType, StructureAction.CompleteJobStructure, jobTime, items.ToArray());
                        World.Current.RegisterStructureJobPrototype(new JobModel(), this);
                        */
                        break;
                    case "Sprites":
                        var spritesReader = reader.ReadSubtree();
                        while (spritesReader.Read()) {
                            if (spritesReader.Name == "Sprite") {
                                _sprites[spritesReader.GetAttribute("Key")] =
                                    spritesReader.GetAttribute("Name");
                            }
                        }
                        break;
                }    
            }
        }
        
        public string GetSpriteName(string key)
        {
            if (_sprites.ContainsKey(key)) {
                return _sprites[key];
            }

            return null;
        }

        //////////////////////////////
        /// ISelectable
        //////////////////////////////

        public string GetName()
        {
            return Name;
        }

        public string GetDescription()
        {
            // todo: add description property and matching XML field
            return "this is a structure";
        }

        public string GetHitPointString()
        {
            // todo : add a hitpoint system to ... well ... everything
            return "20/20";
        }
        
        //////////////////////////////
        /// IJsonSerializable
        //////////////////////////////

        public void FromJson(JToken token)
        {
            // read from parameters!!!
        }

        public JToken ToJson()
        {
            var parameters = new JArray();
            foreach (var json in _parameters.Keys.Select(
                k => new JObject {
                    { k, _parameters[k] }
            })) {
                parameters.Add(json);
            }

            return new JObject {
                { "X", Tile.X },
                { "Y", Tile.Y },
                { "Z", Tile.Z },
                { "Type", Type },
                { "MovementCost", MovementCost },
                { "Parameters", parameters },
            };
        }
    }
}
