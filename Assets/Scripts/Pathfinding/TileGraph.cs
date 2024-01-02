using System.Collections.Generic;
using UnityEngine;
using TileModel = Model.Tile;
using WorldModel = Model.World;

namespace Pathfinding
{
    public class TileGraph
    {
        // this class constructs a simple path-finding compatibale graph
        // of our world. each tile is a node. each walkable neghbour
        // from a tile is linked via an edge connection.

        public Dictionary<TileModel, Node<TileModel>> Nodes { get; private set; }
        
        public TileGraph(WorldModel world)
        {
            // loop through all tiles of the world
            // for each tile, create a node
            // do we create nodes for non-floor tiles? No!
            // do we create nodes for tiles that are completely un-walkable (i.e. walls?) No!!!

            Nodes = new Dictionary<TileModel, Node<TileModel>>();

            for (var x = 0; x < world.Width; x++) {
                for (var y = 0; y < world.Height; y++) {
                    for (var z = 0; z < world.Depth; z++) {
                        var tile = world.GetTileModelAt(x, y, z);
                        var node = new Node<TileModel> {
                            Data = tile
                        };
                        Nodes.Add(tile, node);
                    }
                }
            }

            //Debug.Log("TileGraph Created : " + Nodes.Count + " nodes.");
            
            var edgeCount = 0;
            // Now loop through all tiles again
            // Create edges fore neighbours
            foreach (var tile in Nodes.Keys) {
                var node = Nodes[tile];
                var edges = new List<Edge<TileModel>>();
                
                // get a list of neighbours for the tile
                var neighbours = tile.GetNeighbours(); // NOTE : Some of the array spots could be null.
                foreach (var neighbour in neighbours) {
                    if (neighbour == null || !(neighbour.MovementCost > 0) || tile.IsClippingCorner(neighbour)) {
                        continue;
                    }
                    // neighbor exists and is walkable, so create an edge
                       
                    var edge = new Edge<TileModel>();
                    edge.Cost = neighbour.MovementCost;
                    edge.Node = Nodes[neighbour];

                    // add the edge to our temporary list
                    edges.Add(edge);
                    edgeCount++;
                }

                node.Edges = edges.ToArray();
                //
            }
            
            Debug.Log("TileGraph Created : " + edgeCount + " edges.");
        }
    }
}
