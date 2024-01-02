using System.Collections.Generic;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using Pathfinding;
using UnityEngine;
using JobModel = Model.Job;
using TileModel = Model.Tile;
using ItemModel = Model.Item;
using WorldModel = Model.World;
using CreatureModel = Model.Creature;

namespace Model.Manager
{
    [MoonSharpUserData]
    public class Item : global::Model.Interface.IJsonSerializable
    {
        public Dictionary<string, List<ItemModel>> Items { get; }
        
        //////////////////////////////
        /// Constructor
        //////////////////////////////
        public Item()
        {
            Items = new Dictionary<string, List<ItemModel>>();
        }
       
        
        //////////////////////////////
        /// Update
        //////////////////////////////
        
        public bool Place(TileModel tile, ItemModel item)
        {
            var wasTileEmpty = tile.Item == null; 
            
            if (tile.PlaceItem(item) == false) {
                // the tile did not accept the inventory for whatever reason, therefore stop.
                return false;
            }
            
            Clear(item);
            
            // we may have also created a new stack on the tile, if the tile was previously empty.
            if (!wasTileEmpty) {
                return true;
            }

            if (Items.ContainsKey(tile.Item.Type) == false) {
                Items[tile.Item.Type] = new List<ItemModel>();
            }

            Items[tile.Item.Type].Add(tile.Item);
            OnItemCreated(tile.Item);
            return true;
        }
        
        public bool Place(JobModel job, ItemModel item)
        {
            if (!job.ItemRequirements.ContainsKey(item.Type)) {
                Debug.LogError("! trying to add item to a job that it doesn't want");
                return false;
            }    
            
            job.ItemRequirements[item.Type].StackSize += item.StackSize;
            if (job.ItemRequirements[item.Type].MaxStackSize < job.ItemRequirements[item.Type].StackSize) {
                item.StackSize = job.ItemRequirements[item.Type].StackSize - job.ItemRequirements[item.Type].MaxStackSize;
                job.ItemRequirements[item.Type].StackSize = job.ItemRequirements[item.Type].MaxStackSize;
            } else {
                item.StackSize = 0;
            }
            
            Clear(item);
            return true;
        }
        
        public bool Place(CreatureModel creature, ItemModel item, int amount = -1)
        {
            amount = amount < 0 ? item.StackSize : Mathf.Min(amount, item.StackSize);

            if (creature.Item == null) {
                creature.Item = item.Clone();
                creature.Item.StackSize = 0;
                Items[creature.Item.Type].Add(creature.Item);
            } else if (creature.Item.Type != item.Type) {
                Debug.LogError("! character is trying to pick up a mismatched inventory object type.");
                return false;
            }

            creature.Item.StackSize += amount;

            if (creature.Item.MaxStackSize < creature.Item.StackSize) {
                item.StackSize = creature.Item.StackSize - creature.Item.MaxStackSize;
                creature.Item.StackSize = creature.Item.MaxStackSize;
            } else {
                item.StackSize -= amount;
            }

            Clear(item);
            return true;
        }

        public TileAStar GetPathToClosestItemOfType(string objectType, TileModel tile, int desiredAmount, bool canTakeFromStockpile)
        {
            return !Items.ContainsKey(objectType) 
                ? null 
                : new TileAStar(WorldModel.Current, tile, null, objectType, desiredAmount, canTakeFromStockpile);
        }
       
        
        private void Clear(ItemModel item)
        {
            if (item.StackSize != 0) {
                return;
            }

            if( Items.ContainsKey(item.Type) ) {
                Items[item.Type].Remove(item);
            }
                
            if(item.Tile != null) {
                item.Tile.Item = null;
                item.Tile = null;
            }

            if (item.Creature == null) {
                return;
            }

            item.Creature.Item = null;
            item.Creature = null;
        }
        
        //////////////////////////////
        /// Callback
        //////////////////////////////
        private System.Action<ItemModel> _callbackItemCreated;

        private void OnItemCreated(ItemModel item)
        {
            _callbackItemCreated?.Invoke(item);
        }
        
        public void RegisterItemCreated(System.Action<ItemModel> callback)
        {
            _callbackItemCreated += callback;
        }
        
        public void UnRegisterItemCreated(System.Action<ItemModel> callback)
        {
            _callbackItemCreated -= callback;
        }
        
        /////////////////////////////////////////////////////////////////
        /// IJsonSerializable
        /////////////////////////////////////////////////////////////////

        public void FromJson(JToken token)
        {
            if (token == null) {
                return;
            } 
           
            /*
            foreach (var t in (JArray) token) {
                var maxStackSize = (int) t["MaxStackSize"];
                var stackSize = (int) t["StackSize"];
                var Type = (int) t["Type"];
                
                //public bool Place(TileModel tile, ItemModel item)
                if (Creature != null) {
                    json.Add(
                        "Creature",
                        new JObject {
                            {"X", Creature.Current.X},
                            {"Y", Creature.Current.Y},
                            {"Z", Creature.Current.Z},
                            {"Type", Creature.Type},
                        }
                    );
                }

                if (Tile != null) {
                    json.Add(
                        "Tile",
                        new JObject {
                            {"X", Tile.X},
                            {"Y", Tile.Y},
                            {"Z", Tile.Z},
                        }
                    );
                }
            } */
        }

        public JToken ToJson()
        {
            /*
            var token = new JArray();
            foreach (var i in Items.SelectMany(pair => pair.Value)) {
                token.Add(i.ToJson());
            }
            return token;
            */
            return null;
        }
    }
}
