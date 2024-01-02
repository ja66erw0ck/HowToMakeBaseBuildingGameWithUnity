using System;
using System.Collections.Generic;
using UnityEngine;
using WorldModel = Model.World;
using TileModel = Model.Tile;
using TileType = Type.Tile;
using LayerType = Type.Layer;
using SpriteManager = Manager.Sprite;

namespace Controller.Sprite
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] private Transform tileTransform;
        private Dictionary<TileModel, GameObject> _tileGameObjects;

        private void Start()
        {
            _tileGameObjects = new Dictionary<TileModel, GameObject>();
            
            // Create a GameObject for each of our tiles, so they show visually.
            for (var x = 0; x < WorldModel.Current.Width; x++) {
                for (var y = 0; y < WorldModel.Current.Height; y++) {
                    for (var z = 0; z < WorldModel.Current.Depth; z++) {
                        // get the tile data
                        var tile = WorldModel.Current.GetTileModelAt(x, y, z);
                        // this creates a new GameObject and adds it to our scene.

                        var tileObject = new GameObject {
                            name = "Tile_" + x + "_" + y + "_" + z
                        };

                        // Add a sprite renderer, but don't bother setting a sprite
                        // because all the tiles are empty right now.
                        tileObject.AddComponent<SpriteRenderer>().sprite =
                            SpriteManager.Instance.GetSprite("Empty");

                        tileObject.transform.position = new Vector3(tile.X, tile.Y, tile.Z - LayerType.Tile);
                        tileObject.transform.SetParent(tileTransform);
                        
                        // Add our tile/gameObject pair to the dictionary.
                        _tileGameObjects.Add(tile, tileObject);
                        
                        // sprite set
                        OnTileChanged(tile);
                    }
                }
            }
   
            // Register our callback so that our GameObject gets updated whenever
            // the tile's type changes.
            WorldModel.Current.RegisterTileChanged(OnTileChanged);
        }

        private void OnTileChanged(TileModel tile)
        {
            if (_tileGameObjects.ContainsKey(tile) == false) {
                Debug.LogError("! tileGameObjects doesn't contain the tile");
                return;
            }

            var tileObject = _tileGameObjects[tile];
            if (tileObject == null) {
                Debug.LogError("! tileGameObjects returned gameObject is null");
                return;
            }
           
            switch (tile.Type) {
                case TileType.Floor:
                    string[] spriteNames = { "Floor_240", "Floor_288", "Floor_336", "Floor_384" };
                    tileObject.GetComponent<SpriteRenderer>().sprite =
                            SpriteManager.Instance.GetSprite(spriteNames[tile.Z]);
                    break;
                case TileType.Empty:
                    tileObject.GetComponent<SpriteRenderer>().sprite =
                            SpriteManager.Instance.GetSprite("Empty");
                    break;
                case TileType.Water:
                    break;
                case TileType.Magma:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}