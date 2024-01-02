using System.Collections.Generic;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using TileModel = Model.Tile;
using WorldModel = Model.World;
using CreatureModel = Model.Creature;

namespace Model.Manager
{
    [MoonSharpUserData]
    public class Creature : global::Model.Interface.IJsonSerializable
    {
        public List<CreatureModel> Creatures { get; }

        //////////////////////////////
        /// Constructor
        //////////////////////////////
       
        public Creature()
        {
            Creatures = new List<CreatureModel>();
        }
       
        //////////////////////////////
        /// Update
        //////////////////////////////

        // TODO: make factory!!!
        public CreatureModel Create(TileModel tile, float speed, string type)
        {
            var c = new CreatureModel(tile, speed, type);
            Creatures.Add(c);
            _callbackCreatureCreated?.Invoke(c);
            return c;
        }

        //////////////////////////////
        /// Callback
        //////////////////////////////
        
        private System.Action<CreatureModel> _callbackCreatureCreated;
        
        public void RegisterCreatureCreated(System.Action<CreatureModel> callback)
        {
            _callbackCreatureCreated += callback;
        }

        public void UnregisterCreatureCreated(System.Action<CreatureModel> callback)
        {
            _callbackCreatureCreated -= callback;
        }
        
        //////////////////////////////
        /// IJsonSerializable
        //////////////////////////////

        public void FromJson(JToken token)
        {
            if (token == null) {
                return;
            } 
            
            foreach (var t in (JArray) token) {
                var x = (int) t["X"];
                var y = (int) t["Y"];
                var z = (int) t["Z"];
                var type = (string) t["Type"];
                var speed = (int) t["Speed"];

                Create(WorldModel.Current.GetTileModelAt(x, y, z), speed, type);
            }
        }

        public JToken ToJson()
        {
            var array = new JArray();
            foreach (var c in Creatures) {
                array.Add(c.ToJson());    
            }
            return array;
        }
    }
}
