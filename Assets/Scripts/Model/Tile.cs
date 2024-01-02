using System;
using System.Collections.Generic;
using Model.Interface;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using UnityEngine;
using TileType = Type.Tile;
using EnterableType = Type.Enterable;
using WorldModel = Model.World;
using JobModel = Model.Job;
using TileModel = Model.Tile;
using ItemModel = Model.Item;
using CreatureModel = Model.Creature;
using StructureModel = Model.Structure;
using RoomModel = Model.Room;

namespace Model
{
    [MoonSharpUserData]
    public class Tile : IJsonSerializable, global::Model.Interface.ISelectable
    {
        private TileType _type = TileType.Empty;
        public TileType Type {
            get => _type;
            set {
                var oldType = _type;
                _type = value;
                // Call the callback and let things know we've changed.
                if (CallbackTileChanged != null && oldType != _type) {
                    CallbackTileChanged(this);
                }
            }
        }
       
        // TODO: should can stack items
        public ItemModel Item { get; set; }
        public readonly List<ItemModel> Items;
        public readonly List<CreatureModel> Creatures;
        public StructureModel Structure { get; set; }
        public readonly List<StructureModel> Structures;
        
        public RoomModel Room { get; set; }
        
        public JobModel PendingStructureJob { get; set; }

        public int X { get; }
        public int Y { get; }
        public int Z { get; }

        public TileModel North => WorldModel.Current.GetTileModelAt(X, Y + 1, Z);
        public TileModel South => WorldModel.Current.GetTileModelAt(X, Y - 1, Z);
        public TileModel East => WorldModel.Current.GetTileModelAt(X + 1, Y, Z);
        public TileModel West => WorldModel.Current.GetTileModelAt(X - 1, Y, Z);
        public TileModel NorthEast => WorldModel.Current.GetTileModelAt(X + 1, Y + 1, Z);
        public TileModel NorthWest => WorldModel.Current.GetTileModelAt(X - 1, Y + 1, Z);
        public TileModel SouthEast => WorldModel.Current.GetTileModelAt(X + 1, Y - 1, Z);
        public TileModel SouthWest => WorldModel.Current.GetTileModelAt(X - 1, Y - 1, Z);
        public TileModel Up => WorldModel.Current.GetTileModelAt(X, Y, Z - 1);
        public TileModel Down => WorldModel.Current.GetTileModelAt(X, Y, Z + 1);
        
        // FIXME : this is just hardcoded for now, basically just a reminder of something we
        // might want to do more ...
        private const float BaseTileMovementCost = 1f;

        public float MovementCost {
            get {
                if (_type == TileType.Empty) {
                    return 0f; // 0 is unwalkable
                }

                if (Structure == null) {
                    return BaseTileMovementCost;
                }

                return BaseTileMovementCost * Structure.MovementCost;
            }
        }
   
        public Action<TileModel> CallbackTileChanged;

        public Tile(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;

            Items = new List<ItemModel>();
            Creatures = new List<CreatureModel>();
            Structures = new List<StructureModel>();
        }

        public void RegisterTileChanged(Action<TileModel> callback)
        {
            CallbackTileChanged += callback;
        }
    
        public void UnregisterTileChanged(Action<TileModel> callback)
        {
            CallbackTileChanged -= callback;
        }

        public bool UnPlaceStructure()
        {
            // just uninstalling. FIXME: what if we have a multi-tile furniture?

            var structure = Structure;
            if (structure == null) {
                return false;
            }

            for (var xoffset = X; xoffset < (X + structure.Width); xoffset++) {
                for (var yoffset = Y; yoffset < (Y + structure.Height); yoffset++) {
                    var tile = WorldModel.Current.GetTileModelAt(xoffset, yoffset, Z);
                    tile.Structure = null;
                }
            }
            
            return true;
        }
        
        public bool PlaceStructure(StructureModel structure)
        {
            if (structure == null) {
                UnPlaceStructure();
            } 
            
            if (structure.IsValidPosition(this) == false) {
                Debug.LogError("Trying to assign an structure to a tile that isn't valid!");
                return false;
            }
            
            for (var xoffset = X; xoffset < (X + structure.Width); xoffset++) {
                for (var yoffset = Y; yoffset < (Y + structure.Height); yoffset++) {
                    var tile = WorldModel.Current.GetTileModelAt(xoffset, yoffset, Z);
                    tile.Structure = structure;
                }
            }
            
            return true;
        }

        public bool PlaceItem(ItemModel item)
        {
            if (item == null) {
                Item = null;
                return true;
            }

            if (Item != null) { 
                // there's already inventory here. maybe we can combine a stack?
                if (Item.Type != item.Type) {
                    Debug.Log("! trying to assign inventory to a tile that already has some of a different type");
                    return false;
                }

                var numToMove = item.StackSize;
                if (Item.StackSize + numToMove > Item.MaxStackSize) {
                    numToMove = Item.MaxStackSize - Item.StackSize;
                }

                Item.StackSize += numToMove;
                item.StackSize -= numToMove;

                return true;
            }

            // at this point, we know that our current item is actually
            // null. now we can't just do a direct assignment, because
            // the inventory needs to know that the old stack is now
            // empty and has to be removed from the previous lists.

            Item = item.Clone();
            Item.Tile = this;
            item.StackSize = 0;  
            
            return true;
        }
    
        // Tell us if two tiles are adjacent.
        public bool IsNeighbour(TileModel tile, bool diagOkay = false)
        {
            // check to see if we have a difference of exactly one between the two
            // tile coordinates. is so, the we are vertical or hoizontal neighbours.
            return
                Mathf.Abs(X - tile.X) + Mathf.Abs(Y - tile.Y) + Mathf.Abs(Z - tile.Z) == 1 ||
                diagOkay && Mathf.Abs(X - tile.X) + Mathf.Abs(Y - tile.Y) + Mathf.Abs(Z - tile.Z) == 2; // z ???
        }

        public TileModel[] GetNeighbours(bool diagOkay = false, bool vertOkay = false)
        {
            TileModel[] neighbours;
            if (!diagOkay) {
                // n e s w u d
                neighbours = !vertOkay ? new TileModel[4] : new TileModel[6];
            } else {
                // n e s w ne se sw nw u d
                neighbours = !vertOkay ? new TileModel[8] : new TileModel[10];
            }

            neighbours[0] = North;
            neighbours[1] = East;
            neighbours[2] = South;
            neighbours[3] = West;

            if (!diagOkay) {
                if (!vertOkay) {
                    return neighbours;
                }

                neighbours[4] = Up;
                neighbours[5] = Down;

                return neighbours;
            }

            neighbours[4] = NorthEast;
            neighbours[5] = SouthEast;
            neighbours[6] = SouthWest;
            neighbours[7] = NorthWest;

            if (!vertOkay) {
                return neighbours;
            }

            neighbours[8] = Up;
            neighbours[9] = Down;

            return neighbours;
        }

        public EnterableType IsEnterable()
        {
            // this returns true if you can enter this tile right this moment
            if (MovementCost == 0) {
                return EnterableType.Never;
            }
            
            // check out structure to see if it has a special block on enterablility
            return Structure?.IsEnterable() ?? EnterableType.Yes;

        }
        
        public bool IsClippingCorner(TileModel neighbour)
        {
            // if the movement from current to neighbor is diagonal (e.g. NE)
            // check to make sure we aren't clipping (e.g. N and E are both walkable)
            var diagonalX = X - neighbour.X;
            var diagonalY = Y - neighbour.Y;

            // we are diagonal
            if (Mathf.Abs(diagonalX) + Mathf.Abs(diagonalY) != 2) {
                return false;
            }

            return WorldModel.Current.GetTileModelAt(X - diagonalX, Y, Z).MovementCost == 0 
                || WorldModel.Current.GetTileModelAt(X, Y - diagonalY, Z).MovementCost == 0;
        }

        /////////////////////////////////////////////////////////////////
        /// ISelectable
        /////////////////////////////////////////////////////////////////

        public string GetName()
        {
            return _type.ToString();
        }

        public string GetDescription()
        {
            return "Tile (" + X + "," + Y + "," + Z + ")";
        }

        public string GetHitPointString()
        {
            // do tiles have hitpoints? can flooring be damaged? obviously "empty" is indestructible
            return "";
        }

        //////////////////////////////
        /// IJsonSerializable
        //////////////////////////////
        public void FromJson(JToken token)
        {
            Room = WorldModel.Current.RoomManager[(int) token["RoomId"]];
            Room?.AssignTile(this);
            Type = (TileType)(int) token["Type"];
        }

        public JToken ToJson()
        {
            return new JObject(
                new JProperty("X", X),
                new JProperty("Y", Y),
                new JProperty("Z", Z),
                new JProperty("RoomId", WorldModel.Current.RoomManager.GetRoomId(Room)),
                new JProperty("Type", Type)
            );
        }
    }
}