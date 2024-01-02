using System.Collections.Generic;
using Model.Interface;
using Newtonsoft.Json.Linq;
using WorldModel = Model.World;
using StructureModel = Model.Structure;

namespace Model.Manager
{
    public class Structure : IJsonSerializable
    {
        public readonly List<StructureModel> Structures;

        public Structure()
        {
            Structures = new List<StructureModel>();
        }

        public void Add(StructureModel structure)
        {
            Structures.Add(structure);
        }

        public void Remove(StructureModel structure)
        {
            Structures.Remove(structure);
        }
        
        //////////////////////////////
        /// Callback
        //////////////////////////////
        private System.Action<StructureModel> _callbackStructureCreated;
        
        public void RegisterStructureCreated(System.Action<StructureModel> callback)
        {
            _callbackStructureCreated += callback;
        }
        
        public void UnRegisterStructureCreated(System.Action<StructureModel> callback)
        {
            _callbackStructureCreated -= callback;
        }
        
        public bool OnStructureCreated(StructureModel structure)
        {
            if (_callbackStructureCreated == null) {
                return false;
            }

            _callbackStructureCreated.Invoke(structure);
            return true;
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
                
                var structure = WorldModel.Current.PlaceStructure(type, WorldModel.Current.GetTileModelAt(x, y, z), false);
                structure.MovementCost = (float) t["MovementCost"];
            }
        }

        public JToken ToJson()
        {
            var token = new JArray();
            foreach (var s in Structures) {
                token.Add(s.ToJson());
            }
            return token;
        }
    }
}
