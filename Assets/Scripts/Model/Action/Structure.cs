using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;
using JobModel = Model.Job;
using ItemModel = Model.Item;
using WorldModel = Model.World;
using StructureModel = Model.Structure;
using StructureAction =  Model.Action.Structure;

namespace Model.Action
{
    public class Structure
    {
        private static StructureAction _instance;
        private readonly Script _luaScript; 
        
        public Structure(string luaCode)
        {
            UserData.RegisterAssembly();
            
            _instance = this;
            _luaScript = new Script();
            
            // If we want to be able to instantiate a new object of a class
            //   i.e. by doing    SomeClass.__new()
            // We need to make the base type visible.
            _luaScript.Globals["Item"] = typeof(ItemModel);
            _luaScript.Globals["Job"] = typeof(JobModel);

            // Also to access statics/globals
            _luaScript.Globals["World"] = typeof(WorldModel);
            
            _luaScript.DoString(luaCode);
        }

        public static void CallFunctionsWithStructure(IEnumerable<string> functionNames, StructureModel structure, float deltaTime)
        {
            foreach (var name in functionNames) {
                var function = _instance._luaScript.Globals[name];
                if (function == null) {
                    Debug.LogError("<" + name +"> is not a Lua function.");    
                }
                var result = _instance._luaScript.Call(function, structure , deltaTime);

                if (result.Type == DataType.String) {
                    Debug.Log(result.String);
                }
            }            
        }

        public static DynValue CallFunction(string functionName, params object[] args)
        {
            var function = _instance._luaScript.Globals[functionName];
            return _instance._luaScript.Call(function, args);
        }
        
        public static void CompleteJobStructure(JobModel job)
        {
            WorldModel.Current.PlaceStructure(job.Type, job.Tile);
            // FIXME: i don't like having to manually and explicitly set
            // flags that prevent conflicts. it's too easy to forget to set/clear them!
            job.Tile.PendingStructureJob = null;
        }
    }
}
