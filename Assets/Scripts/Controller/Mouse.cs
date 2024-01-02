using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using MouseController = Controller.Mouse;
using BuildModeController = Controller.BuildMode;
using StructureSpriteController = Controller.Sprite.Structure;
using LayerType = Type.Layer;
using MouseModeType = Type.MouseMode;
using BuildModeType = Type.BuildMode;
using WorldModel = Model.World;
using TileModel = Model.Tile;
using TimeManager = Manager.Time;
using SpriteManager = Manager.Sprite;
using ISelectable = Model.Interface.ISelectable;

namespace Controller
{
    public class Mouse : MonoBehaviour
    {
        public static MouseController Instance { get; set; }
        
        [SerializeField] private GameObject cursorPrefab;
        [SerializeField] private Transform cursorTransform;
        
        // The world position of the mouse last frame  
        private Vector3 _lastFramePosition;
        private Vector3 _currentFramePosition;
        
        // The world position start of our left mouse drag operation 
        private Vector3 _dragStartPosition;

        private List<GameObject> _dragPreviewGameObjects;
        
        private BuildModeController _buildModeController;
        
        private StructureSpriteController _structureSpriteController;
        
        private bool _isDragging = false;

        private MouseModeType currentMode = MouseModeType.Select;
        
        private void Start()
        {
            if (Instance == null || Instance == this) {
                Instance = this;
            } else {
               Debug.LogError("! There should never be two mouse controllers.");
            } 
            
            _dragPreviewGameObjects = new List<GameObject>();
            _buildModeController = FindObjectOfType<BuildModeController>();
            _structureSpriteController = FindObjectOfType<StructureSpriteController>();
        }
        
        // Update is called once per frame
        private void Update()
        {
            if (TimeManager.Instance.IsModal) {
                // a modal dialog is open, so dpn't process any game inputs from the input
                return;
            }
            
            _currentFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (Input.GetKeyUp(KeyCode.Escape) || Input.GetMouseButtonUp(1)) {
                switch (currentMode) {
                    case MouseModeType.Build:
                        currentMode = MouseModeType.Select;
                        break;
                    case MouseModeType.Select:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            UpdateSelection();
            UpdateDragging();
            UpdateCameraMoving();
            //UpdateLevel();

            _lastFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        public class SelectionInfo
        {
            public TileModel Tile;
            public ISelectable[] StuffInTile;
            public int SubSelection = 0;
        }

        public SelectionInfo Selection;

        private void UpdateSelection()
        {
            // this handles us left-clicking on furniture or creatures to set a selection.
            if (Input.GetKeyUp(KeyCode.Escape) || UnityEngine.Input.GetMouseButtonUp(1)) {
                Selection = null;
            }
            
            if (currentMode != MouseModeType.Select) {
                return;
            }
            
            // if we're over a UI element, then bail out from this.
            if (EventSystem.current.IsPointerOverGameObject()) {
                return;
            }

            if (!Input.GetMouseButtonUp(0)) {
                return;
            }

            // we just release the mouse button, so that's our queue to update our selection
            var tile = GetMouseOverTile();

            if (tile == null) {
                // no valid under mouse
                return;
            }

            //var lengthOfStuffInTile = RebuildSelectionInfo(tile);
            if (Selection == null || Selection.Tile != tile) {
                // we have just selected a brand new tile, reset the info.
                Selection = new SelectionInfo {
                    Tile = tile
                };

                RebuildSelectionStuffInTile(tile);
                    
                for (var i = 0; i < Selection.StuffInTile.Length; i++) {
                    if (Selection.StuffInTile[i] == null) {
                        continue;
                    }

                    Selection.SubSelection = i;
                    break;
                }
            } else {
                // this is the same tile we already have slected, so cycle the subselection to the next non-null item.
                // not that the tile sub selection can NEVER be null, so we know we'll always find something.
                   
                // Rebuild the array of possible sub-selection in case creatures moved in or out of the tile. 
                RebuildSelectionStuffInTile(tile);
                    
                do {
                    Selection.SubSelection = (Selection.SubSelection + 1) %  Selection.StuffInTile.Length;
                } while (Selection.StuffInTile[Selection.SubSelection] == null);
            }
            //Debug.Log(Selection.SubSelection);
        }

        private void RebuildSelectionStuffInTile(TileModel tile)
        {
            // make sure stuffinTile is big enough to handle all the creatures, plus the 3 extra values
            Selection.StuffInTile = new ISelectable[tile.Creatures.Count + 3];

            // copy the creature references
            for (var i = 0; i < tile.Creatures.Count; i++) {
                Selection.StuffInTile[i] = tile.Creatures[i];
            }

            // Now assign references to the other three sub-selections available
            Selection.StuffInTile[Selection.StuffInTile.Length - 3] = tile.Structure;
            Selection.StuffInTile[Selection.StuffInTile.Length - 2] = tile.Item;
            Selection.StuffInTile[Selection.StuffInTile.Length - 1] = tile;
        }

        private void UpdateDragging()
        {
            // if we're over a UI element, then bail out from this.
            if (EventSystem.current.IsPointerOverGameObject()) {
                return;
            }
            
            // Clean up old drag previews
            while (_dragPreviewGameObjects.Count > 0) {
                var gameObject = _dragPreviewGameObjects[0];
                _dragPreviewGameObjects.RemoveAt(0); 
                Destroy(gameObject);
            }

            if (currentMode != MouseModeType.Build) {
                return;
            }

            // Drag Start
            if (Input.GetMouseButtonDown(0)) {
                _dragStartPosition = _currentFramePosition;
                _isDragging = true;
            } else if (_isDragging == false) {
                _dragStartPosition = _currentFramePosition;
            }

            if (Input.GetMouseButtonUp(1) || Input.GetKeyUp(KeyCode.Escape)) {
                // the RIGHT mouse button was released, so we
                // are cancelling any dragging/build mode.
                _isDragging = false;
            }
            
            //
            if (!_buildModeController.IsObjectDraggable()) { 
                _dragStartPosition = _currentFramePosition;
            }
            
            var startX = Mathf.FloorToInt(_dragStartPosition.x + 0.5f);
            var endX = Mathf.FloorToInt(_currentFramePosition.x + 0.5f);
            var startY = Mathf.FloorToInt(_dragStartPosition.y + 0.5f);
            var endY = Mathf.FloorToInt(_currentFramePosition.y + 0.5f);
               
            // we may be dragging in the "wrong" direction, so flip things if needed.
            if (endX < startX) {
                var tmp = endX;
                endX = startX;
                startX = tmp;
            }
                
            if (endY < startY) {
                var tmp = endY;
                endY = startY;
                startY = tmp;
            }
            
            var z = (int)Camera.main.transform.position.z + 1;
            // Display a preview of the drag area
            for (var x = startX; x <= endX; x++) {
                for (var y = startY; y <= endY; y++) {
                    var tile = WorldModel.Current.GetTileModelAt(x, y, z);
                    if (tile == null) {
                        continue;
                    }

                    // Display the building hint on top of this tile position
                    if (_buildModeController.BuildModeTypeValue == BuildModeType.Structure) {
                        ShowStructureSpriteAtTile(_buildModeController.BuildModeObjectType, tile);
                    } else {
                        // show the generic dragging visuals    
                        var gameObject = Instantiate(cursorPrefab, new Vector3(x, y, z), Quaternion.identity);
                        gameObject.transform.SetParent(cursorTransform);
                        gameObject.GetComponent<SpriteRenderer>().sprite = SpriteManager.Instance.GetSprite("CursorCircle");
                        _dragPreviewGameObjects.Add(gameObject);
                    }
                }
            }
            
            // Drag End
            if (!Input.GetMouseButtonUp(0)) {
                return;
            }
            
            _isDragging = false;
                
            // loop through all the selected tiles
            for (var x = startX; x <= endX; x++) {
                for (var y = startY; y <= endY; y++) {
                    var tile = WorldModel.Current.GetTileModelAt(x, y, z);
                    if (tile != null) {
                        _buildModeController.DoBuild(tile);
                    }
                }
            }
        }

        private void UpdateCameraMoving()
        {
            // Handle screen dragging
            if (UnityEngine.Input.GetMouseButton(1)) { // right button
                Camera.main.transform.Translate(_lastFramePosition - _currentFramePosition);

                Camera.main.transform.position = new Vector3(
                    Mathf.Clamp(Camera.main.transform.position.x, 0, WorldModel.Current.Width),
                    Mathf.Clamp(Camera.main.transform.position.y, 0, WorldModel.Current.Height),
                    Camera.main.transform.position.z
                );
            }
           
            // zoom
            Camera.main.orthographicSize -= Camera.main.orthographicSize * UnityEngine.Input.GetAxis("Mouse ScrollWheel") * 3f;
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 3f, 30f);
        }

        private static void UpdateLevel()
        {
            var position = Camera.main.transform.position;
            if (UnityEngine.Input.GetKey(KeyCode.Alpha0)) { // z 0
                position.z = -1;
            } else if (UnityEngine.Input.GetKey(KeyCode.Alpha1)) { // z 1
                position.z = 0;
            } else if (UnityEngine.Input.GetKey(KeyCode.Alpha2)) { // z 2
                position.z = 1;
            } else if (UnityEngine.Input.GetKey(KeyCode.Alpha3)) { // z 3
                position.z = 2;
            }
            Camera.main.transform.position = position;
        } 

        public TileModel GetMouseOverTile()
        {
            var x = Mathf.FloorToInt(_currentFramePosition.x + 0.5f);
            var y = Mathf.FloorToInt(_currentFramePosition.y + 0.5f);
            var z = Mathf.FloorToInt(Camera.main.transform.position.z + 1);
            return WorldModel.Current.GetTileModelAt(x, y, z);
        }
     
        private void ShowStructureSpriteAtTile(string structureType, TileModel tile)
        {
            var gameObject = new GameObject();
            gameObject.transform.SetParent(cursorTransform);
            _dragPreviewGameObjects.Add(gameObject);
            
            var structurePrototype = WorldModel.Current.StructurePrototypes[structureType];
             
            var spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = _structureSpriteController.GetSpriteForStructure(tile, structureType, structurePrototype);

            spriteRenderer.color =
                WorldModel.Current.IsStructurePlacementValid(structureType, tile) 
                    ? new Color(0.5f, 1f, 0.5f, 0.25f) 
                    : new Color(1f, 0.5f, 0.5f, 0.25f);

            gameObject.transform.position = new Vector3(
                tile.X + (structurePrototype.Width - 1) / 2f,
                tile.Y + (structurePrototype.Height - 1) / 2f,
                tile.Z - LayerType.Structure
            );
        }

        public void StartbuildMode()
        {
            currentMode = MouseModeType.Build;
        }
    }
}
