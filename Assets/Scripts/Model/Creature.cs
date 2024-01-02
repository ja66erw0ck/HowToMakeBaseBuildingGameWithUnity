using System.Collections.Generic;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using UnityEngine;
using DirectionType = Type.Direction;
using EnterableType = Type.Enterable;
using JobModel = Model.Job;
using ItemModel = Model.Item;
using TileModel = Model.Tile;
using WorldModel = Model.World;
using CreatureModel = Model.Creature;
using TileAStar = Pathfinding.TileAStar;
using TimeManager = Manager.Time;

namespace Model
{
    [MoonSharpUserData]
    public class Creature : global::Model.Interface.IJsonSerializable, global::Model.Interface.ISelectable
    {
        public float X
        {
            get {
                if (Next == null) {
                    return Current.X;
                }
                return Mathf.Lerp(Current.X, Next.X, MovementPercentage);
            }
        }
        public float Y
        {
            get {
                if (Next == null) {
                    return Current.Y;
                }
                return Mathf.Lerp(Current.Y, Next.Y, MovementPercentage);
            }
        }
        public float Z => Current.Z;

        //
        private TileModel _current;
        public TileModel Current {
            get => _current;
            private set {
                if (_current == value) {
                    return;
                }

                _current?.Creatures.Remove(this);
                _current = value;
                _current?.Creatures.Add(this);
            }
        }

        // if we aren't moving, then Destination == Current
        private TileModel _destination;

        private TileModel Destination {
            get => _destination;
            set {
                if (_destination == value) {
                    return;
                }
                
                _destination = value;
                Path = null; // if this is a new destination, then we need to invalidate pathfinding.
            }
        }

        //  the next tile in the pathfinding sequence
        public TileModel Next { get; set; }
        private TileAStar Path { get; set; }

        // goes from 0 to 1 as we move from Current to Destination
        private float MovementPercentage { get; set; }
        private float Speed { get; set; }
        
        //
        private System.Action<CreatureModel> _callbackCharacterChanged;

        public JobModel Job { get; private set; }
       
        // the item we are carrying (not gear/equipment)
        public ItemModel Item { get; set; }
        
        public List<string> SpriteAnimations { get; private set; }
        public float AnimationSpeed { get; private set; }
        public int CurrentSprite { get; set; }
       
        public string Type { get; private set; }
        
        public DirectionType MoveDirection { get; set; }
        
        //////////////////////////////
        /// Constructor
        //////////////////////////////
        
        public Creature(TileModel tile, float speed, string objectType)
        {
            SetupCharacter(tile, speed, objectType);
        }

        public Creature()
        {
        }

        ~Creature()
        {
            TimeManager.Instance.EveryFrameUnpaused -= Update;
        }
        
        private void SetupCharacter(TileModel tile, float speed, string objectType)
        {
            Current = Next = Destination = tile;
            Speed = speed; // tiles per second
            Type = objectType;
            
            SpriteAnimations = new List<string>();
            AnimationSpeed = 1f;
            MoveDirection = DirectionType.None;
            
            //
            TimeManager.Instance.EveryFrameUnpaused += Update;
        }
        
        //////////////////////////////
        /// Update
        //////////////////////////////
        
        private void Update(float deltaTime)
        {
            DoJob(deltaTime);
            Move(deltaTime);
            
            _callbackCharacterChanged?.Invoke(this);
            
            // todo: animation change speed, maybe to lua script???
            if (AnimationSpeed > 0f) {
                AnimationSpeed -= deltaTime;
            } else {
                AnimationSpeed = 1f;
            }
        }

        private bool GetNewJob()
        {
            Job = WorldModel.Current.JobManager.Dequeue()
                ?? new JobModel(
                    Current, "Waiting"
                    , null, UnityEngine.Random.Range(1f, 5f)
                    , null, false
                );

            Destination = Job.Tile;
            Job.RegisterJobStopped(OnJobStopped);
                
            // immediately check to see if the job tile is reachable.
            // NOTE: we might not be pathing to it right away (due to
            // requiring materials), but we still need to verify that the
            // final location can be reached
            
            // this will calculate a path from current to destination
            Path = new TileAStar(WorldModel.Current, Current, Destination);
            if (Path.Length() != 0) {
                return true;
            }
            
            Debug.Log("! AStar returned no path to target job tile!!!!");
            AbandonJob();
            Destination = Current;
            return false;
        }

        private void DoJob(float deltaTime)
        {
            // do i have a job?
            if (Job == null) {
                if (!GetNewJob()) {
                    return;
                }
            }
            
            if (!CheckForJobMaterials()) {
                return;
            }

            // If we get here, then the job has all the material that it needs.
            // Lets make sure that our destination tile is the job site tile.
            Destination = Job.Tile;

            // Are we there yet?
            if (Current == Job.Tile)
            {
                // We are at the correct tile for our job, so 
                // execute the job's "DoWork", which is mostly
                // going to countdown jobTime and potentially
                // call its "Job Complete" callback.
                Job.DoWork(deltaTime);
            }
        }
        
        // Checks weather the current job has all the materials in place and if not instructs the working character to get the materials there first.
        // Only ever returns true if all materials for the job are at the job location and thus signals to the calling code, that it can proceed with job execution.
        private bool CheckForJobMaterials()
        {
            if (Job.HasAllMaterial()) {
                return true; //we can return early
            }

            // At this point we know, that the job still needs materials.
            // First we check if we carry any materials the job wants by chance.
            if (Item != null) {
                if (Job.DesiresItemType(Item) > 0) {
                    // If so, deliver the goods.
                    // Walk to the job tile, then drop off the stack into the job.
                    if (Current == Job.Tile) {
                        // We are at the job's site, so drop the inventory
                        WorldModel.Current.ItemManager.Place(Job, Item);
                        Job.DoWork(0); // This will call all cbJobWorked callbacks, because even though
                                       // we aren't progressing, it might want to do something with the fact
                                       // that the requirements are being met.

                        //at this point we should dump anything in our inventory
                        DumpExcessItem();
                    } else {
                        // We still need to walk to the job site.
                        Destination = Job.Tile;
                        return false;
                    }
                } else {
                    // We are carrying something, but the job doesn't want it!
                    // Dump the inventory so we can be ready to carry what the job actually wants.
                    DumpExcessItem();                
                }
            } else {
                // At this point, the job still requires inventory, but we aren't carrying it!
                // Are we standing on a tile with goods that are desired by the job?
                Debug.Log("Standing on Tile check");
                if (Current.Item != null &&
                    Job.DesiresItemType(Current.Item) > 0 &&
                    (JobModel.CanTakeFromStockpile || Current.Structure == null || Current.Structure.IsStockpile() == false))
                {
                    // Pick up the stuff!
                    Debug.Log("Pick up the stuff");

                    WorldModel.Current.ItemManager.Place(
                        this,
                        Current.Item,
                        Job.DesiresItemType(Current.Item)
                    );
                } else {
                    // Walk towards a tile containing the required goods.
                    Debug.Log("Walk to the stuff");

                    // Find the first thing in the Job that isn't satisfied.
                    var desired = Job.GetFirstDesiredItem();

                    if (Current != Next) {
                        // We are still moving somewhere, so just bail out.
                        return false;
                    }

                    // Any chance we already have a path that leads to the items we want?
                    if (Path != null && Path.EndTile() != null && Path.EndTile().Item != null && Path.EndTile().Item.Type == desired.Type) {
                        // We are already moving towards a tile that contains what we want!
                        // so....do nothing?
                    } else {
                        var newPath = WorldModel.Current.ItemManager.GetPathToClosestItemOfType(
                            desired.Type, Current,
                            desired.MaxStackSize - desired.StackSize, JobModel.CanTakeFromStockpile
                        );

                        if (newPath == null || newPath.Length() < 1) {
                            //Debug.Log("Path is null and we have no path to object of type: " + desired.objectType);
                            // Cancel the job, since we have no way to get any raw materials!
                            Debug.Log("No tile contains objects of type '" + desired.Type + "' to satisfy job requirements.");
                            AbandonJob();
                            return false;
                        }

                        Debug.Log("Path returned with length of: " + newPath.Length());                    
                        Destination = newPath.EndTile();

                        // Since we already have a path calculated, let's just save that.
                        Path = newPath;

                        // Ignore first tile, because that's what we're already in.
                        Next = newPath.Dequeue();
                    }

                    // One way or the other, we are now on route to an object of the right type.
                    return false;
                }
            }

            return false; // We can't continue until all materials are satisfied.
        }

        // This function instructs the character to null its inventory.
        // However in the fuure it should actually look for a place to dump the materials and then do so.
        private void DumpExcessItem()
        {
            // TODO: Look for Places accepting the inventory in the following order:
            // - Jobs also needing this item (this could serve us when building Walls, as the character could transport ressources for multiple walls at once)
            // - Stockpiles (as not to clutter the floor)
            // - Floor

            //if (World.current.inventoryManager.PlaceInventory(CurrTile, inventory) == false)
            //{
            //    Debug.LogError("Character tried to dump inventory into an invalid tile (maybe there's already something here). FIXME: Setting inventory to null and leaking for now");
            //    // FIXME: For the sake of continuing on, we are still going to dump any
            //    // reference to the current inventory, but this means we are "leaking"
            //    // inventory.  This is permanently lost now.
            //}

            Item = null;
        }

        private void AbandonJob()
        {
            Next = Destination = Current;
            Job.CancelJob();
            Job = null;
        }

        private void Move(float deltaTime)
        {
            if (Current == Destination) {
                Path = null;
                return; // already arrived
            }
            
            // Current = tile that i am currently in (and may be in the process of leaving)
            // Next = tile that i am currently entering
            // Destination = Our final destination --- we never walk here directly, but instead use it for the pathfinding.

            if (Next == null || Next == Current) {
                // get the next tile from the pathfinder
                if (Path == null || Path.Length() == 0) {
                    // generate a path to our destination
                    // this will calculate a path from current to destination
                    Path = new TileAStar(WorldModel.Current, Current, Destination);
                    if (Path.Length() == 0) {
                        Debug.Log("! AStar returned no path to destination!!!!");
                        // FIXME: job should maybe be re-enqueued instead???
                        AbandonJob();
                        return;
                    }
                
                    // let's ignore the first tile, because that's the tile we're currently in.
                    Next = Path.Dequeue();
                }

                // grab the next waypoint from the pathfinding system!
                Next = Path.Dequeue();
       
                /*
                if (Next == Current) {
                    Debug.LogError("! Move next is current??");
                } 
                */
            }

            /*
            if (Path.Length() == 1) {
                return;
            }
            */
            
            // at this point we should have a valid path
            
            // what's total distance from a to b
            // we are goint to use euclidean distance for now ...
            // but when we do the apthfinding system, we'll likely 
            // switch to something like manhattan or Chebyshev distance
            var distanceToTravel = Mathf.Sqrt(
                Mathf.Pow(Current.X - Next.X, 2) + Mathf.Pow(Current.Y - Next.Y, 2)
            );

            if (Next.IsEnterable() == EnterableType.Never) {      
                // Most likely a wall got built, so we just need to reset our pathfinding information.
                // FIXME: ideally, when a wall gets spawned, we should invalidate our path immediately,
                //      so that we don't waste a bunch of time walking towards a dead end.
                //      To save CPU, maybe we can only check every so often?
                //      Or maybe we should register a callback to the OnTileChanged event?
                // debug log
                //Debug.LogError("FIXME: character was trying to enter an unwalkable tile.");
                Next = null;
                Path = null;
                return;
            } else if (Next.IsEnterable() == EnterableType.Soon) { 
                // we can't enter the now, but we should be able to in the future.
                // this is likely a door.
                // so we don't bail on our movement/path, but we do return
                // now and don't actually process the movement.
                return;
            }
            
            // how much distance can be travel this update?
            var distanceThisFrame = Speed / Next.MovementCost * deltaTime;
            // how much is that in terms of percentage to our destination?
            var percentThisFrame = distanceThisFrame / distanceToTravel;

            // add that to overall percentage traveleled.
            MovementPercentage += percentThisFrame;
            if (MovementPercentage >= 1) {
                // we have reached our destination

                // TODO: Get the next from the pathfinding system. 
                //      if there are no more tiles, then we have truly
                //      reached our destination.

                Current = Next;
                MovementPercentage = 0;
                // FIXME? do we actually want to retain any overshot movement???
            }
        }
        
        public void SetDestination(TileModel tile)
        {
            if (Current.IsNeighbour(tile) == false) {
                Debug.Log("! Character::SetDestination --- our Destination tile isn't actually our neighbours");
            }

            Destination = tile;
        }

        public void RegisterOnChanged(System.Action<CreatureModel> callback)
        {
            _callbackCharacterChanged += callback;
        }

        public void UnregisterOnChanged(System.Action<CreatureModel> callback) 
        {
            _callbackCharacterChanged -= callback;
        }

        private void OnJobStopped(JobModel job) {
            // Job completed (if non-repeating) or was cancelled.
           
            job.UnregisterJobStopped(OnJobStopped);
            
            if(job != Job) {
                Debug.LogError("! Character being told about job that isn't his. You forgot to unregister something.");
                return;
            }

            Job = null;
        }

        /////////////////////////////////////////////////////////////////
        /// ISelectable
        /////////////////////////////////////////////////////////////////
        
        public string GetName()
        {
            return "Harry the hammer";
        }

        public string GetDescription()
        {
            return "a human warrior!";
        }

        public string GetHitPointString()
        {
            return "100/100";
        }
        
        //////////////////////////////
        /// IJsonSerializable
        //////////////////////////////

        public void FromJson(JToken token)
        {
            //todo jabberwock --- stats !!!
        }

        public JToken ToJson()
        {
            return new JObject {
                { "X", Current.X },
                { "Y", Current.Y },
                { "Z", Current.Z },
                { "Type", Type },
                { "Speed", Speed },
            };
        }
    }
}
