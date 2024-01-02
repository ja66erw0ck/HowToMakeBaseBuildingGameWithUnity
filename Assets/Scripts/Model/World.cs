using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Model.Interface;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using UnityEngine;
using TileType = Type.Tile;
using TileModel = Model.Tile;
using WorldModel  = Model.World;
using JobModel  = Model.Job;
using CreatureModel  = Model.Creature;
using StructureModel  = Model.Structure;
using RoomModel = Model.Room;
using StructureAction = Model.Action.Structure;
using JobManager = Model.Manager.Job;
using ItemManager = Model.Manager.Item;
using RoomManager = Model.Manager.Room;
using CreatureManager = Model.Manager.Creature;
using StructureManager = Model.Manager.Structure;
using PathTileGraph = Pathfinding.TileGraph;

namespace Model
{
    [MoonSharpUserData]
    public class World : IJsonSerializable
    {
        // 3-dimensional array to hold our tile data
        private TileModel[,,] _tiles;
        
        public ItemManager ItemManager;
        public CreatureManager CreatureManager;
        public RoomManager RoomManager;
        public StructureManager StructureManager;
        public JobManager JobManager;
        
        // the pathfinding graph used to navigate our world map.
        public PathTileGraph TileGraph { get; set; }
       
        //
        public Dictionary<string, StructureModel> StructurePrototypes;
        public Dictionary<string, JobModel> StructureJobPrototypes;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Depth { get; private set; }

        private Action<TileModel> _callbackTileChanged;
        
        public static WorldModel Current { get; private set; }
       
        public World(int width, int height, int depth)
        {
            // creates an empty world
            SetupWorld(width, height, depth);
        }
       
        // default constructor, used when loading a world from a file.
        public World()
        {
        }
        
        private void SetupWorld(int width, int height, int depth)
        {
            // set the current world to be this world.
            Current = this;
            
            Width = width;
            Height = height;
            Depth = depth;
            
            //
            JobManager = new JobManager();
            ItemManager = new ItemManager();
            RoomManager = new RoomManager();
            CreatureManager = new CreatureManager();
            StructureManager = new StructureManager();
            
            _tiles = new TileModel[width, height, depth];

            for (var x = 0; x < width; x++) {
                for (var y = 0; y < height; y++) {
                    for (var z = 0; z < depth; z++) {
                        _tiles[x, y, z] = new TileModel(x, y, z);
                        _tiles[x, y, z].RegisterTileChanged(OnTileChanged);
                        _tiles[x, y, z].Room = RoomManager.GetOutsideRoom();
                        // Rooms 0 is always going to be outside, and that is our default room
                    }
                }
            }
        
            Debug.Log("World created with " + (Width * Height * Depth) + " tiles.");
            CreateStructureProtoTypes();
        }

        public void RegisterStructureJobPrototype(JobModel job, StructureModel structure)
        {
            StructureJobPrototypes[structure.Type] = job;
        }

        private static void LoadStructureLua()
        {
            var filePath = Path.Combine(Application.streamingAssetsPath, "Functions");
            filePath = Path.Combine(filePath, "Structure.lua");
            var luaCode = File.ReadAllText(filePath);
            
            if (luaCode == null) {
                Debug.LogError("Failed to load Lua Code text asset!!!");
                return;

            }

            // Instantiate the singleton
            new StructureAction(luaCode);
        }

        private void CreateStructureProtoTypes()
        {
            LoadStructureLua(); 
            
            StructurePrototypes = new Dictionary<string, StructureModel>();
            StructureJobPrototypes = new Dictionary<string, JobModel>();
            
            // Read Structure prototype xml file here
            // text here, rather than opening the file ourselves.

            var filePath = Path.Combine(Application.streamingAssetsPath, "Data");
            filePath = Path.Combine(filePath, "Structure.xml");
            var structureXmlText = File.ReadAllText(filePath);
            
            var reader = new XmlTextReader(new StringReader(structureXmlText));
           
            if (reader.ReadToDescendant("Structures")) {
                if (reader.ReadToDescendant("Structure")) {
                    do {
                        var structure = new StructureModel();
                        structure.ReadXmlPrototype(reader);
                        StructurePrototypes[structure.Type] = structure;
                    } while (reader.ReadToNextSibling("Structure"));
                } else {
                    Debug.LogError("the structure prototype definition file doesn't have any 'structure' elements.");
                }
            } else {
                Debug.LogError("Did not find a 'Structures' element in the prototype definition file.");
            }
        }

        public CreatureModel CreateCharacter(TileModel tile, float speed, string type)
        {
            return CreatureManager.Create(tile, speed, type); 
        }

        public StructureModel PlaceStructure(string type, TileModel tile, bool isDoRoomFloodFill = true)
        {
            // TODO : this function assumes 1x1 tiles -- change this later!     
            if (StructurePrototypes.ContainsKey(type) == false) {
                Debug.LogError("doesn't contain a prototype for key: " + type);
                return null;
            }

            var structure = StructureModel.Place(StructurePrototypes[type], tile);
            if (structure == null) {
                // Failed to place object 
                return null;
            }
            
            structure.RegisterOnRemovedCallback(OnStructureRemoved);
            StructureManager.Add(structure);
            
            // do we need to recalculate our rooms?
            if (isDoRoomFloodFill && structure.IsRoomEnclosure) {
                RoomModel.DoRoomFloodFill(structure.Tile);
            }
            
            if (StructureManager.OnStructureCreated(structure) && 
                Math.Abs(structure.MovementCost - 1) > 0
            )  {
                InvalidateTileGraph();
            }

            return structure;
        }

        
        public void RegisterTileChanged(Action<TileModel> callback)
        {
            _callbackTileChanged += callback;
        }
        
        public void UnRegisterTileChanged(Action<TileModel> callback)
        {
            _callbackTileChanged -= callback;
        }

        // Gets call whenever any tile changes
        public void OnTileChanged(TileModel tile)
        {
            _callbackTileChanged?.Invoke(tile);
            
            InvalidateTileGraph();
        }

        // this should be called whenever a change to the world
        // means that our old pathfinding info is invalid.
        public void InvalidateTileGraph()
        {
            TileGraph = null;
        }

        public bool IsStructurePlacementValid(string structureType, TileModel tile)
        {
            return StructurePrototypes[structureType].IsValidPosition(tile);
        }

        public StructureModel GetStructurePrototype(string objectType)
        {
            if (StructurePrototypes.ContainsKey(objectType) == false) {
                Debug.LogError("No structure with type: " + objectType);
            }    
            return StructurePrototypes[objectType];
        }
        
        public TileModel GetTileModelAt(int x, int y, int z)
        {
            if (x >= Width || x < 0 || y >= Height || y < 0 || z >= Depth || z < 0) {
                //Debug.LogError("Tile (" + x + ", " + y + ", " + z + ") is out of range");
                return null;
            }
        
            return _tiles[x, y, z];
        }
       
        public void SetupPathfindingExample()
        {
            Debug.Log("Setup Pathfinding Example");
            
            // make a set of floors/walls to test pathfinding with.
            var l = Width / 2 - 5;
            var b = Height / 2 - 5;

            //
            for (var x = l - 5; x < l + 15; x++) {
                for (var y = b - 5; y < b + 15; y++) {
                    // todo first floor only!!!
                    _tiles[x, y, 0].Type = TileType.Floor;

                    //
                    if (x == l || x == (l + 9) || y == b || y == (b + 9)) {
                        if (x != (l + 9) && y != (b + 4)) {
                            PlaceStructure("BrickWall", _tiles[x, y, 0]);
                        }
                    }
                }
            }
        }

        public void OnStructureRemoved(StructureModel structure)
        {
            StructureManager.Remove(structure);
        }
       
        /*
        private void ReadXmlCharacters(XmlReader reader)
        {
            if (reader.ReadToDescendant("Character") == false) {
                return;
            }

            do {
                var objectType = reader.GetAttribute("Type");
                var x = int.Parse(reader.GetAttribute("X"));
                var y = int.Parse(reader.GetAttribute("Y"));
                var z = int.Parse(reader.GetAttribute("Z"));
                var speed = float.Parse(reader.GetAttribute("Speed"));
                
                var tile = _tiles[x, y, z];

                var character = CreateCharacter(tile, speed, objectType);
                character.ReadXml(reader);
            } while (reader.ReadToNextSibling("Character"));
        } 
        */
        
        /////////////////////////////////////////////////////////////////
        /// IJsonSerializable
        /////////////////////////////////////////////////////////////////
        
        public void FromJson(JToken token)
        {
            var width = (int) token["Width"];
            var height = (int) token["Height"];
            var depth = (int) token["Depth"];

            SetupWorld(width, height, depth);

            // Tile
            TilesFromJson(token["Tiles"]);
            // Structure
            StructureManager.FromJson(token["Structures"]);
            // Job
            JobManager.FromJson(token["Jobs"]);
            // Item
            ItemManager.FromJson(token["Items"]);
            // Creature
            CreatureManager.FromJson(token["Creatures"]);
            // Room
            RoomManager.FromJson(token["Rooms"]);
        }

        public JToken ToJson()
        {
            var json = new JObject {
                // Attribute
                { "Width", Width },
                { "Height", Height },
                { "Depth", Depth },
                // Tile
                { "Tiles", TilesToJson() }, 
                // Structure
                { "Structures", StructureManager.ToJson() },
                // Job
                { "Jobs", JobManager.ToJson() },
                // Item
                { "Items", ItemManager.ToJson() },
                // Creature
                { "Creatures", CreatureManager.ToJson() },
                // Room
                { "Rooms", RoomManager.ToJson() },
            };

            return json;
        }

        private void TilesFromJson(JToken token)
        {
            if (token == null) {
                return;
            } 
            
            foreach (var t in (JArray) token) {
                var x = (int) t["X"];
                var y = (int) t["Y"];
                var z = (int) t["Z"];

                _tiles[x, y, z].FromJson(t);
            }
        }

        private JToken TilesToJson()
        {
            var json = new JArray();
            for (var x = 0; x < Width; x++) {
                for (var y = 0; y < Height; y++) {
                    for (var z = 0; z < Depth; z++) {
                        json.Add(_tiles[x, y, z].ToJson());
                    }
                }
            }
            return json;
        }
    }
}