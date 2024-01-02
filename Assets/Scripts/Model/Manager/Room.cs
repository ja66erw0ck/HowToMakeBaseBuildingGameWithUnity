using System.Collections.Generic;
using System.Linq;
using Model.Interface;
using Newtonsoft.Json.Linq;
using UnityEngine;
using RoomModel = Model.Room;

namespace Model.Manager
{
    public class Room : IJsonSerializable
    {
        private readonly List<RoomModel> _rooms;

        public RoomModel this[int index] {
            get {
                if (index < 0 || index >= Count) {
                    return null;
                }

                return _rooms[index];
            }    
        }

        public int Count => _rooms.Count;

        //////////////////////////////
        /// Constructor
        //////////////////////////////
        public Room()
        {
            _rooms = new List<RoomModel> {
                new RoomModel() // OutSideRoom
            };
        }

        public RoomModel GetOutsideRoom()
        {
            return _rooms[0];
        }

        public int GetRoomId(RoomModel room)
        {
            return _rooms.IndexOf(room);
        }

        public RoomModel GetRoomFromId(int id)
        {
            if (id < 0 || id > _rooms.Count - 1) {
                return null;
            }
            
            return _rooms[id];
        }

        public void Add(RoomModel room)
        {
            _rooms.Add(room);
        }

        public void Remove(RoomModel room)
        {
            if (room == GetOutsideRoom()) {
                Debug.LogError("! Tried to delete the outside room");
                return;
            } 
           
            // remove this room from our rooms list
            _rooms.Remove(room);
            
            // all tiles that belonged to this room should be re-assigned to the outside
            room.ReturnTilesToOutsideRoom();
        }
        
        //////////////////////////////
        /// IJsonSerializable
        //////////////////////////////
        
        public void FromJson(JToken token)
        {
            if (token == null) {
                return;
            } 
            
            foreach (var t in token) {
                var room = new RoomModel();
                room.FromJson(t);
                Add(room);
            }
        }

        public JToken ToJson()
        {
            var array = new JArray();
            foreach (var r in _rooms.Where(r => !r.IsOutsideRoom())) {
                array.Add(r.ToJson());
            }
            return array;
        }
    }
}
