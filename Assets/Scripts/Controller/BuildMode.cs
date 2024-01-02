using UnityEngine;
using TileType = Type.Tile;
using BuildModeType = Type.BuildMode;
using TileModel = Model.Tile;
using JobModel = Model.Job;
using WorldModel = Model.World;
using StructureAction = Model.Action.Structure;

namespace Controller
{
    public class BuildMode : MonoBehaviour
    {
        // Set Mode
        public BuildModeType BuildModeTypeValue = BuildModeType.Floor;
        private TileType buildModeTileType = TileType.Floor;
        public string BuildModeObjectType;

        private Mouse _mouseController;
        
        private void Start()
        {
            _mouseController = FindObjectOfType<Mouse>();
        }

        public bool IsObjectDraggable()
        {
            if (BuildModeTypeValue == BuildModeType.Floor || BuildModeTypeValue == BuildModeType.Deconstruct) {
                // floors are draggable
                return true;
            }

            var prototype = WorldModel.Current.StructurePrototypes[BuildModeObjectType];
            return prototype.Width == 1 && prototype.Height == 1;
        }

        // SetMode
        public void BuildStructure(string objectType)
        {
            // wall is no a tile!! wall is an "StructureObject" that exists on TOP of a tile.
            //
            BuildModeTypeValue = BuildModeType.Structure;
            BuildModeObjectType = objectType;
            _mouseController.StartbuildMode();
        }
        
        public void BuildDeconstruct()
        {
            BuildModeTypeValue = BuildModeType.Deconstruct;
            _mouseController.StartbuildMode();
        }        
        
        public void BuildFloor()
        {
            BuildModeTypeValue = BuildModeType.Floor;
            buildModeTileType = TileType.Floor;
            _mouseController.StartbuildMode();
        }

        public void Bulldoze()
        {
            BuildModeTypeValue = BuildModeType.Floor;
            buildModeTileType = TileType.Empty;
            _mouseController.StartbuildMode();
        }
        
        public void DoPathfindingTest()
        {
            WorldModel.Current.SetupPathfindingExample();
        } 

        public void DoBuild(TileModel tile)
        {
            switch (BuildModeTypeValue) {
                case BuildModeType.Structure:
                    // create the installed and assign it to the tile

                    // FIXME: this instantly builds the structure;
                    //world?.Place(buildModeObjectType, tile);

                    // can we build the structure in the selected tile?
                    // run the valid palcement function!!!
                    if (
                        WorldModel.Current.IsStructurePlacementValid(BuildModeObjectType, tile) 
                        && tile.PendingStructureJob == null
                    ) {
                        // This tile position is valid for this structure
                    
                        // check if there is existing structure in this tile if so delete it. 
                        // todo possibly return resources. will the deconstruct() method handle
                        // that? if so what will happen if resources drop on top of new non-passable structure.
                        tile.Structure?.Deconstruct();

                        // Create a job for it to be build

                        JobModel job;
                        if (WorldModel.Current.StructureJobPrototypes.ContainsKey(BuildModeObjectType)) {
                            // make clone
                            job = WorldModel.Current.StructureJobPrototypes[BuildModeObjectType].Clone();
                            // assign the correct tile.
                            job.Tile = tile;
                        } else {
                            job = new JobModel(tile, BuildModeObjectType, StructureAction.CompleteJobStructure, 0.1f, null);
                        }

                        job.StructurePrototype = WorldModel.Current.StructurePrototypes[BuildModeObjectType];

                        // FIXME: i don't like having to manually and explicitly set
                        // flags that prevent conflicts. it's too easy to forget to set/clear them!
                        tile.PendingStructureJob = job;
                        job.RegisterJobStopped((theJob) => { theJob.Tile.PendingStructureJob = null; });

                        // Add the job to the queue
                        WorldModel.Current.JobManager.Enqueue(job);
                    }
                    break;
                case BuildModeType.Floor:
                    // tile changing mode
                    tile.Type = buildModeTileType;
                    break;
                case BuildModeType.Deconstruct:
                    tile.Structure?.Deconstruct();
                    break;
                default:
                    // 
                    Debug.LogError("Unimplemented build mode");
                    break;
            }
        }
    }
}
