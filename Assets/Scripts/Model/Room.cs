using System.Collections.Generic;
using System.Linq;
using Model.Interface;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using UnityEngine;
using TileType = Type.Tile;
using WorldModel = Model.World;
using TileModel = Model.Tile;
using RoomModel = Model.Room;

namespace Model
{
    [MoonSharpUserData]
    public class Room : IJsonSerializable
    {
        public Dictionary<string, float> AtmosphericGasses;

        private List<TileModel> _tiles;

        public Room()
        {
            _tiles = new List<TileModel>();
            AtmosphericGasses = new Dictionary<string, float>();
        }
        
        public void AssignTile(TileModel tile)
        {
            if (_tiles.Contains(tile)) {
                // this tile already in this room
                return;
            }

            // belongs to some other room
            tile.Room?._tiles.Remove(tile);

            tile.Room = this;
            _tiles.Add(tile);
        }

        public void ReturnTilesToOutsideRoom()
        {
            foreach (var tile in _tiles) {
                tile.Room = WorldModel.Current.RoomManager.GetOutsideRoom(); // assign to outside
            }
            _tiles = new List<TileModel>();
        }

        public bool IsOutsideRoom()
        {
            return this == WorldModel.Current.RoomManager.GetOutsideRoom();
        }

        public void ChangeGas(string name, float amount)
        {
            if (IsOutsideRoom()) {
                return;
            } 
            
            if (AtmosphericGasses.ContainsKey(name)) {
                AtmosphericGasses[name] += amount;
            }
            else {
                AtmosphericGasses[name] = amount;
            }

            if (AtmosphericGasses[name] < 0) {
                AtmosphericGasses[name] = 0;
            }
        }

        public float GetGasAmount(string name)
        {
            if (AtmosphericGasses.ContainsKey(name)) {
                return AtmosphericGasses[name];
            }

            return 0;
        }

        public float GetGasPercentage(string name)
        {
            if (!AtmosphericGasses.ContainsKey(name)) {
                return 0f;
            }
            
            var total = AtmosphericGasses.Keys.Sum(gas => AtmosphericGasses[gas]);
            return AtmosphericGasses[name] / total * 100;
        }

        public string[] GetGasNames()
        {
            return AtmosphericGasses.Keys.ToArray();
        }

        public static void DoRoomFloodFill(TileModel tile, bool isOnlyIfOutside = false)
        {
            // source is the piece of structure that may be
            // splitting two existing rooms, or may be the final
            // enclosing piece to form a new room.
            // check the nesw neighbours of the structure's tile
            // and do flood fill from them

            var world = WorldModel.Current;
            var oldRoom = tile.Room;

            if (oldRoom != null) {
                // the tile had a room, so this must be a new piece of structure
                // that is potentially dividing this old room into as many as four new rooms.
                
                // try building a new rooms for each of our nesw directions
                foreach (var t in tile.GetNeighbours()) {
                    if ( t.Room != null && (!isOnlyIfOutside || t.Room.IsOutsideRoom())) {
                        ActualFloodFill(t, oldRoom);
                    }
                }

                tile.Room = null;
                
                oldRoom._tiles.Remove(tile);

                // if this piece of structure was added to an existing room
                // which should always be true assuming with consider  
                // delete that room and assign all tiles within to be "outside" for now  

                if (!oldRoom.IsOutsideRoom()) {
                    // at this point, oldRoom shouln't have nay more tiles left in it,
                    // so in practice this "DeleteRoom" should mostly only need
                    // to remove the room from the world's list.

                    if (oldRoom._tiles.Count > 0) {
                        Debug.LogError("'oldRoom' still has tiles assigned to it. this is clearly wrong.");
                    }

                    world.RoomManager.Remove(oldRoom);
                }
            } else {
                // oldRoom is null, which means the source tile was probably a wall,
                // though this may not be the case any longer (i.e. the wall was
                // probably deconstructed. so the only thing we have to try is
                // to spawn ONE new room starting from the tile in question.

                ActualFloodFill(tile, null);
            }
        }

        protected static void ActualFloodFill(TileModel tile, RoomModel oldRoom)
        {
            if (tile == null) {
                // we are trying to flood fill off the map, so just return
                // without doing anything.
                return;
            }

            if (tile.Room != oldRoom) {
                // this tile was already assigned to another "new" oldRoom, which means
                // that the direction picked isn't isolated. so we can just return
                // without creating a new oldRoom
                return;
            }

            if (tile.Structure != null && tile.Structure.IsRoomEnclosure) {
                // this tile has a wall/door/whatever in it, so cleary
                // we can't do a oldRoom here.
                return;
            }

            if (tile.Type == TileType.Empty) {
                // this tile is empty space and must remain part of outside
                return;
            }
            
            // if we get to this point, then we know that we need to create a new oldRoom
            var newRoom = new RoomModel();
            var tilesToCheck = new Queue<TileModel>();
            tilesToCheck.Enqueue(tile);

            var isConnectedToSpace = false;
            var processedTiles = 0;  

            while (tilesToCheck.Count > 0) {
                var t = tilesToCheck.Dequeue();

                processedTiles++;
                
                if (t.Room == newRoom) {
                    continue;
                }
                
                newRoom.AssignTile(t);

                var neighbors = t.GetNeighbours();
                foreach (var t2 in neighbors) {
                    if (t2 == null || t2.Type == TileType.Empty) {
                        // we have hit open space (either by being the edge of the map or being an empty tile)
                        // so this "room" we're building is actually part of the outside.
                        // therefore, we can immediately end the flood fill (which otherwise would take ages)
                        // and more importantly, we need to delete this "newRoom" and re-assign
                        // all the tiles to Outside
                        isConnectedToSpace = true;
                        
                        /*
                        if (oldRoom != null) {
                            newRoom.ReturnTilesToOutsideRoom();
                            return;
                        }
                        */
                    } else {
                        // we know t2 is not null nor is it an empty tile, so just make sure it
                        // hasn't already been processed an isn't a "wall" type tile.
                        if (t2.Room != newRoom &&
                            (t2.Structure == null || t2.Structure.IsRoomEnclosure == false)
                        ) {
                            tilesToCheck.Enqueue(t2);
                        }
                    }
                }
            }
            
            // Debug.Log("ActualFloodFill - Processed Tiles: " + processedTiles);

            if (isConnectedToSpace) {
                // all tiles that were found by this flood fill should
                // actually be "assigned" to outside
                newRoom.ReturnTilesToOutsideRoom();
                return;
            }
            
            // copy data from the old room into the new room.
            if (oldRoom != null) {
                // in this case we are splitting one room into two ro more,
                // so we can just copy the old gas ratios.
                newRoom.CopyGas(oldRoom);
            } else {
                // in this case, we are merging one or more rooms together,
                // so we need to actually figure out the total volume of gas
                // in the old room vs the new room and correctly adjust
                // atmospheric quantities.
                
                // TODO 
            }
            
            // Tell the world that a new room has been formed.
            WorldModel.Current.RoomManager.Add(newRoom);
        }

        private void CopyGas(RoomModel other)
        {
            foreach (var gas in other.AtmosphericGasses.Keys) {
                AtmosphericGasses[gas] = other.AtmosphericGasses[gas];
            }
        }
        
        //////////////////////////////
        /// IJsonSerializable
        //////////////////////////////

        public void FromJson(JToken token)
        {
            foreach (var t in (JArray) token["Parameters"]) {
                AtmosphericGasses[(string) t["Name"]] = (float) t["Value"];
            }
        }

        public JToken ToJson()
        {
            var array = new JArray();
            foreach (var k in AtmosphericGasses.Keys) {
                array.Add(
                    new JObject(
                        new JProperty("Name", k),
                        new JProperty("Value", AtmosphericGasses[k])
                    ));
            }
            return new JObject {
                { "Parameters", array }
            };
        }
    }
}
