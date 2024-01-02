using System;
using Model.Interface;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using TileModel = Model.Tile;
using ItemModel = Model.Item;
using CreatureModel = Model.Creature;

namespace Model
{
    [MoonSharpUserData]
    public class Item : IJsonSerializable, global::Model.Interface.ISelectable
    {
        public string Type { get; private set; }
        public int MaxStackSize { get; private set; }

        private int _stackSize;
        public int StackSize
        {
            get => _stackSize;
            set {
                if (_stackSize == value) {
                    return;
                }

                _stackSize = value;
                _callbackItemChanged?.Invoke(this);
            }
        }
        
        public TileModel Tile { get; set; }
        public CreatureModel Creature { get; set; }
        
        /////////////////////////////////////////////////////////////////
        /// Constructor
        /////////////////////////////////////////////////////////////////

        public Item()
        {
        }

        public Item(string type, int maxStackSize, int stackSize = 1)
        {
            Type = type;
            MaxStackSize = maxStackSize;
            StackSize = stackSize;
        }

        private Item(ItemModel other)
        {
            Type = other.Type;
            MaxStackSize = other.MaxStackSize;
            StackSize = other.StackSize;
        }

        public virtual ItemModel Clone()
        {
            return new ItemModel(this);
        }

        public void Update(float deltaTime)
        {
        }
        
        /////////////////////////////////////////////////////////////////
        /// Callback
        /////////////////////////////////////////////////////////////////
       
        private Action<ItemModel> _callbackItemChanged;
        
        public void RegisterChangedCallback(Action<ItemModel> callback)
        {
            _callbackItemChanged += callback;
        }
        
        public void UnregisterChangedCallback(Action<ItemModel> callback)
        {
            _callbackItemChanged -= callback;
        }
        
        /////////////////////////////////////////////////////////////////
        /// ISelectable
        /////////////////////////////////////////////////////////////////

        public string GetName()
        {
            return Type;
        }

        public string GetDescription()
        {
            return "stack " + StackSize + "/" + MaxStackSize;
        }

        public string GetHitPointString()
        {
            // does item have hitpoints? how does it get destripyed?
            // maybe it's just a percentage chance based on damage.
            return "";
        }
        
        /////////////////////////////////////////////////////////////////
        /// IJsonSerializable
        /////////////////////////////////////////////////////////////////

        public void FromJson(JToken token)
        {
        }

        public JToken ToJson()
        {
            var json = new JObject {
                { "MaxStackSize", MaxStackSize },
                { "StackSize", StackSize },
                { "Type", Type }
            };

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
            
            return json;
        }
    }
}
