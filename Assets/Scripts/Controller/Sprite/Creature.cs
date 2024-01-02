using System;
using System.Collections.Generic;
using UnityEngine;
using LayerType = Type.Layer;
using DirectionType = Type.Direction;
using TileModel = Model.Tile;
using WorldModel = Model.World;
using CreatureModel = Model.Creature;
using SpriteManager = Manager.Sprite;

namespace Controller.Sprite
{
    public class Creature : MonoBehaviour
    {
        [SerializeField] private Transform creatureTransform;
        
        private Dictionary<CreatureModel, GameObject> _creatureObjects;

        //////////////////////////////
        /// MonoBehaviour
        //////////////////////////////
        
        private void Start()
        {
            // Instantiate our dictionary that tracks which gameObject is rendering which tile data.
            _creatureObjects = new Dictionary<CreatureModel, GameObject>();

            // Register our callback so that our GameObject gets updated whenever
            // sprite changes.
            WorldModel.Current.CreatureManager.RegisterCreatureCreated(OnCreatureCreated);
        
            // check for pre-existing characters, which won't do callback
            foreach (var c in WorldModel.Current.CreatureManager.Creatures) {
                OnCreatureCreated(c);
            }
        }

        private void OnCreatureCreated(CreatureModel creature)
        {
            // create a visual GameObject linked to this data.
            
            // This creates a new GameObject and adds it to our scene
            var creatureObject = new GameObject {
                name = creature.Type
            };

            switch (creature.Type) {
                case "Warrior":
                    creature.SpriteAnimations.Add("Player0_17");
                    creature.SpriteAnimations.Add("Player1_17");
                    break;
                case "Priest":
                    creature.SpriteAnimations.Add("Player0_25");
                    creature.SpriteAnimations.Add("Player1_25");
                    break;
                case "Wizard":
                    creature.SpriteAnimations.Add("Player0_22");
                    creature.SpriteAnimations.Add("Player1_22");
                    break;
            }
            
            
            creature.CurrentSprite = 0;
            //
            
            creatureObject.AddComponent<SpriteRenderer>().sprite =
                SpriteManager.Instance.GetSprite(creature.SpriteAnimations[creature.CurrentSprite]);
          
            // FIXME : set up so fast?
            creatureObject.transform.position = new Vector3(creature.Current.X, creature.Current.Y, creature.Current.Z - LayerType.Creature);
            creatureObject.transform.SetParent(creatureTransform, true);
            
            _creatureObjects.Add(creature, creatureObject);
            
            // register our callback so that our game objects gets updated whenever
            // the object's into changes.
            creature.RegisterOnChanged(OnCreatureChanged);
        }
        
        private static DirectionType GetDirectionType(TileModel current, TileModel next)
        {
            if (current == null || next == null) {
                return DirectionType.None;
            }
            
            var diff = new Vector2(next.X - current.X, next.Y - current.Y);
            return diff.x switch {
                0 when diff.y == 1 => DirectionType.North,
                0 when diff.y == -1 => DirectionType.South,
                1 when diff.y == 0 => DirectionType.East,
                1 when diff.y == 1 => DirectionType.NorthEast,
                1 when diff.y == -1 => DirectionType.SouthEast,
                -1 when diff.y == 0 => DirectionType.West,
                -1 when diff.y == 1 => DirectionType.NorthWest,
                -1 when diff.y == -1 => DirectionType.SouthWest,
                _ => DirectionType.None
            };
        }

        private void OnCreatureChanged(CreatureModel creature)
        {
            // make sure the character's graphics are correct.
            if (_creatureObjects.ContainsKey(creature) == false) {
                Debug.Log("! OnCreatureChanged -- trying to change visuals for character not in our map.");
                return;
            } 
            
            var creatureObject = _creatureObjects[creature];
           
            // FIXME hard coding
            var directionType = GetDirectionType(creature.Current, creature.Next);
            if (creature.MoveDirection != directionType) {
                switch (directionType) {
                    case DirectionType.North:
                    case DirectionType.East:
                    case DirectionType.NorthEast:
                    case DirectionType.SouthEast:
                        creatureObject.GetComponent<SpriteRenderer>().flipX = true;
                        break;
                    case DirectionType.South:
                    case DirectionType.West:
                    case DirectionType.NorthWest:
                    case DirectionType.SouthWest:
                        creatureObject.GetComponent<SpriteRenderer>().flipX = false;
                        // else directionType == DirectionType.None do nothing
                        break;
                    case DirectionType.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                creature.MoveDirection = directionType;
            }

            creatureObject.transform.position = new Vector3(creature.X, creature.Y, creature.Z - LayerType.Creature);
            
            // change sprite for animation?
            // FIXME : need circular list or something
            // too much call??? maybe
            if (creature.AnimationSpeed > 0.5f && creature.CurrentSprite != 0) {
                creatureObject.GetComponent<SpriteRenderer>().sprite =
                    SpriteManager.Instance.GetSprite(creature.SpriteAnimations[0]);
                creature.CurrentSprite = 0;
            } else if (creature.AnimationSpeed <= 0.5f && creature.CurrentSprite != 1) {
                creatureObject.GetComponent<SpriteRenderer>().sprite =
                    SpriteManager.Instance.GetSprite(creature.SpriteAnimations[1]);
                creature.CurrentSprite = 1;
            }
        }
    }
}