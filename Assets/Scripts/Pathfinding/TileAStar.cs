using System.Collections.Generic;
using System.Linq;
using Priority_Queue;
using UnityEngine;
using TileModel = Model.Tile;
using WorldModel = Model.World;

namespace Pathfinding
{
    public class TileAStar
    {
        public Queue<TileModel> Path;
        
        //public AStar(WorldModel world, TileModel start, TileModel end)
        public TileAStar(WorldModel world, TileModel startTile, TileModel endTile, string itemType = null, int desiredAmount = 0, bool canTakeFromStockpile=false)
        {
            // if endTile is null, then we are simply scanning for the nearest objectType.
            // we can do this by ignoring the heuristic component of AStar, which basically
            // just turns this into an over-engineered Dijkstra's algo
            
            // check to see if we have a valid tile graph
            world.TileGraph ??= new TileGraph(world); 
           
            // a dictionary of all valid, walkable nodes.
            var nodes = world.TileGraph.Nodes;

            // make sure our startTile/endTile tiles are in the list of nodes.
            if (nodes.ContainsKey(startTile) == false) {
                Debug.LogError("! ASTAR : startTile tile isn't in the list");
                return;
            }
            
            //
            var start = nodes[startTile];
            // if endTile is null, then we are simply looking for item object
            // so just set goal to null.
            Node<TileModel> goal = null;
            if (endTile != null) {
                if (nodes.ContainsKey(endTile) == false) {
                    Debug.LogError("! ASTAR : endTile tile isn't in the list");
                    return;
                }

                goal = nodes[endTile];
            }

            //
            var closedSet = new List<Node<TileModel>>();
         
            var openSet = new SimplePriorityQueue<Node<TileModel>>();
            openSet.Enqueue(start, 0);

            var cameFrom = new Dictionary<Node<TileModel>, Node<TileModel>>();
            
            var gscore = new Dictionary<Node<TileModel>, float>();
            foreach (var node in nodes.Values) {
                gscore[node] = Mathf.Infinity;
            }
            gscore[start] = 0f;
            
            var fscore = new Dictionary<Node<TileModel>, float>();
            foreach (var node in nodes.Values) {
                fscore[node] = Mathf.Infinity;
            }
            fscore[start] = EstimateHeuristicCost(start, goal);

            while (openSet.Count > 0) {
                var current = openSet.Dequeue();
               
                // if we have a positional goal, check to see if we are there.
                if(goal != null) {
                    if(current == goal) {
                        ReconstructPath(cameFrom, current);
                        return;
                    }
                } else {
                    // we don't have a positional goal, we're just trying to find
                    // some king of item. have we reached it?
                    if (current.Data.Item != null && current.Data.Item.Type == itemType) {
                        // type is correct
                        if (canTakeFromStockpile || current.Data.Structure == null || !current.Data.Structure.IsStockpile()) {
                            // stockpile status is find
                            ReconstructPath(cameFrom, current);
                            return;
                        }
                    }
                }
                
                closedSet.Add(current);

                foreach (var edgeNeighbor in current.Edges) {
                    var neighbor = edgeNeighbor.Node;
                    
                    if (closedSet.Contains(neighbor) == true) {
                        continue; // ignore this already completed neighbor
                    }

                    var movementCostToNeighbor = neighbor.Data.MovementCost * BetweenDistance(current, neighbor);
                    var tentativeGScore = gscore[current] + movementCostToNeighbor;
                    if (openSet.Contains(neighbor) == true && tentativeGScore >= gscore[neighbor]) {
                        continue;
                    }

                    cameFrom[neighbor] = current;
                    gscore[neighbor] = tentativeGScore;
                    fscore[neighbor] = gscore[neighbor] + EstimateHeuristicCost(neighbor, goal);

                    if (openSet.Contains(neighbor) == false) {
                        //Debug.Log("fscore? " + fscore[neighbor]);
                        openSet.Enqueue(neighbor, fscore[neighbor]);
                    } else {
                        openSet.UpdatePriority(neighbor, fscore[neighbor]);    
                    }
                } // foreach
            } // while
            
            // if we reached here, it means that we've burned through the entire
            // openset without ever reaching a point where current == goal.
            // this happens when there is no path from startTile to goal
            // (so there's a wall or missing floor or something).
            
            // we don't have a failure state. maybe? it's just hat the
            // path list will be null.
        }

        private static float EstimateHeuristicCost(Node<TileModel> start, Node<TileModel> end)
        {
            if (end == null) {
                // we have no fixed destination (i.e. probably looking for an inventory item)
                // so just return 0 for the cost estimate (i.e. all directions as just as good)
                return 0;
            }    
            
            return Mathf.Sqrt(
                Mathf.Pow(start.Data.X - end.Data.X, 2) + 
                Mathf.Pow(start.Data.Y - end.Data.Y, 2) +
                Mathf.Pow(start.Data.Z - end.Data.Z, 2) 
            );
        }

        private static float BetweenDistance(Node<TileModel> start, Node<TileModel> end)
        {
            // we can make assumptions because we know we're working
            // on a grid at this point
           
            // hori/vert neighbours have a distance of 1
            if (Mathf.Abs(start.Data.X - end.Data.X) + Mathf.Abs(start.Data.Y - end.Data.Y) == 1 && start.Data.Z == end.Data.Z) {
                return 1f;
            }
            
            // Diag neighbours have a distance of 1.41421356237
            if (
                Mathf.Abs(start.Data.X - end.Data.X) == 1 && Mathf.Abs(start.Data.Y - end.Data.Y) == 1 && start.Data.Z == end.Data.Z) {
                return 1.41421356237f;
            }
            
            // Up/Down neighbors have a distance of 1
            if (start.Data.X == end.Data.X && start.Data.Y == end.Data.Y && Mathf.Abs(start.Data.Z - end.Data.Z) == 1)
            {
                return 1f;
            }
            
            // Otherwise, do the actual meth
            return Mathf.Sqrt(
                Mathf.Pow(start.Data.X - end.Data.X, 2) +
                 Mathf.Pow(start.Data.Y - end.Data.Y, 2) +
                Mathf.Pow(start.Data.Z - end.Data.Z, 2) 
            );
        }

        public void ReconstructPath(Dictionary<Node<TileModel>, Node<TileModel>> cameFrom, Node<TileModel> current)
        {
            // So at this point, current IS the goal.
            // So what we want to do is walk backwards through the Came_From
            // map, until we reach the "end" of that map...which will be
            // our starting node!
            var totalPath = new Queue<TileModel>();
            totalPath.Enqueue(current.Data); // This "final" step is the path is the goal!

            while (cameFrom.ContainsKey(current))
            {
                /*
                 * Came_From is a map, where the
                 * key => value relation is real saying
                 * some_node => we_got_there_from_this_node
                 */

                current = cameFrom[current];
                totalPath.Enqueue(current.Data);
            }

            // At this point, total_path is a queue that is running
            // backwards from the END tile to the START tile, so let's reverse it.
            Path = new Queue<TileModel>(totalPath.Reverse());           
        }

        public TileModel Dequeue()
        {
            if (Path == null) {
                Debug.LogError("! Attempting to dequeue from an null path.");
                return null;
            }

            if (Path.Count <= 0) {
                Debug.LogError("what ??? ??? ?? ");
                return null;
            }
            
            return Path.Dequeue();
        }

        public int Length()
        {
            return Path?.Count ?? 0;
        }

        public TileModel EndTile()
        {
            if (Path == null || Path.Count == 0) {
                return null;
            }

            return Path.Last();
        }        
    }
}