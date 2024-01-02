using System.Collections.Generic;
using UnityEngine;
using WorldModel = Model.World;
using TileModel = Model.Tile;
using StructureModel = Model.Structure;
using LayerType = Type.Layer;
using SpriteManager = Manager.Sprite;

namespace Controller.Sprite
{
    public class Structure : MonoBehaviour
    {
        [SerializeField] private Transform structureTransform;
        
        private Dictionary<StructureModel, GameObject> _structureObjects { get; set; }

        private void Start()
        {
            // Instantiate our dictionary that tracks which gameObject is rendering which tile data.
            _structureObjects = new Dictionary<StructureModel, GameObject>();
            
            // Register our callback so that our GameObject gets updated whenever
            // the tile's type changes.
            WorldModel.Current.StructureManager.RegisterStructureCreated(OnStructureCreated);
            
            // go through any existing structure (i.e. from a save that was loaded ) all the structure and call the OnCreated event manually?
            foreach(var structure in WorldModel.Current.StructureManager.Structures) {
                OnStructureCreated(structure);    
            }
        }

        public void OnStructureCreated(StructureModel structure)
        {
            // create a visual GameObject linked to this data.
            
            // This creates a new GameObject and adds it to our scene
            var structureObject = new GameObject {
                name = structure.Type + "_" + structure.Tile.X + "_" + structure.Tile.Y + "_" + structure.Tile.Z
            };

            var spriteRenderer = structureObject.AddComponent<SpriteRenderer>();

            spriteRenderer.sprite = GetSpriteForStructure(structure.Tile, structure.Type, structure);
            spriteRenderer.color = structure.Tint;

            // FIXME : set up so fast?
            structureObject.transform.position = new Vector3(
                structure.Tile.X + (structure.Width - 1) / 2f ,
                structure.Tile.Y + (structure.Height - 1) / 2f,
                structure.Tile.Z - LayerType.Structure
            );
                
            structureObject.transform.SetParent(structureTransform, true);

            _structureObjects.Add(structure, structureObject);
            
            // register our callback so that our game objects gets updated whenever
            // the object's into changes.
            
            structure.RegisterOnChangedCallback(OnStructureChanged);
            structure.RegisterOnRemovedCallback(OnStructureRemoved);
        }

        // FIXEME: structureModel param hardcoded
        public UnityEngine.Sprite GetSpriteForStructure(TileModel tile, string objectType, StructureModel structure)
        {
            string spriteName = null;
            string direction = null;
            switch (objectType) {
                case "BrickWall":
                    direction = GetDirectionByTile(tile, objectType);
                    spriteName = structure.GetSpriteName(direction);
                    break;
                case "Door":
                    direction = GetDirectionByTile(tile, "BrickWall");
                    if (structure.GetSpriteName(direction) == null) {
                        direction = "";
                    }
                    
                    if (structure != null && structure.GetParameter("Openness") > 0.5f) {
                        spriteName = structure.GetSpriteName(direction + "_OPEN");
                    } else {
                        spriteName = structure.GetSpriteName(direction);
                    }
                    break;
                case "Stockpile":
                    direction = GetDirectionByTile(tile, objectType); 
                    spriteName = structure.GetSpriteName(direction);
                    break;
                case "Fireplace":
                    if (structure != null && structure.GetParameter("AnimationSpeed") > 0.5f) {
                        spriteName = "Decor0_66";
                    } else {
                        spriteName = "Decor1_66";
                    }
                    break;
                case "BrickMaker":
                    spriteName = "Decor0_106";
                    break;
            }
            
            return SpriteManager.Instance.GetSprite(spriteName);
        }

        private static string GetDirectionByTile(TileModel tile, string objectType)
        {
            var direction = "";
            if (tile.West != null && tile.West.Structure != null && tile.West.Structure.Type == objectType) {
                direction += "W";
            }
                    
            if (tile.East != null && tile.East.Structure != null && tile.East.Structure.Type == objectType) {
                direction += "E";
            }
            
            if (tile.North != null && tile.North.Structure != null && tile.North.Structure.Type == objectType) {
                direction += "N";
            }
            
            if (tile.South != null && tile.South.Structure != null && tile.South.Structure.Type == objectType) {
                direction += "S";
            }

            return direction;
        }

        private void OnStructureChanged(StructureModel structure)
        {
            // make sure the structure's graphics are correct.
            if (_structureObjects.ContainsKey(structure) == false) {
                Debug.LogError("OnStructureChanged -- trying to change visuals for structure not in our map.");
                return;
            } 
            
            var structureObject = _structureObjects[structure];

            var spriteRenderer = structureObject.GetComponent<SpriteRenderer>();

            spriteRenderer.sprite = GetSpriteForStructure(structure.Tile, structure.Type, structure);
            spriteRenderer.color = structure.Tint;
        }
        
        private void OnStructureRemoved(StructureModel structure)
        {
            if (_structureObjects.ContainsKey(structure) == false) {
                Debug.LogError("OnStructureChanged -- trying to change visuals for structure not in our map.");
                return;
            } 
            
            var structureObject = _structureObjects[structure];
            Destroy(structureObject);
            _structureObjects.Remove(structure);
        }
    }
}