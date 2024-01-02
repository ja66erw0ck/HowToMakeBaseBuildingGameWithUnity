using System;
using System.Collections.Generic;
using UnityEngine;
using SpriteManager = Manager.Sprite;

namespace UI.Menu
{
    public class Background : MonoBehaviour
    {
        private readonly List<BackgroundObject> _backgroundObjectList = new List<BackgroundObject>();
        private int _nextUpdateSecond = 1;
        private Vector2 _resolution;
        
        // sprite animation 0
        private List<string> _backgroundSprite0NameList = new List<string> {
            // Left
            // Player1
            "Player0_17", "Player0_18", "Player0_19",
            "Player0_20", "Player0_21", "Player0_22",
            "Player0_24", "Player0_25", "Player0_26",
            // Player2
            "Player0_52", "Player0_53", "Player0_54",
            "Player0_55", "Player0_56",
            // Player3 
            "Player0_59", "Player0_60", "Player0_61",
            "Player0_62", "Player0_63", "Player0_64",
            "Player0_65",
            // Player4
            "Player0_71", "Player0_72", "Player0_73",
            "Player0_74", "Player0_75", "Player0_76",
            // Player5
            "Player0_67", "Player0_68",
            //Quadraped1
            "Quadraped0_9", "Quadraped0_11", "Quadraped0_16",
            "Quadraped0_22", "Quadraped0_27", "Quadraped0_29",
            "Quadraped0_43",
            // Aquatic1
            "Aquatic0_1", "Aquatic0_5", "Aquatic0_7",
            "Aquatic0_10", "Aquatic0_11", "Aquatic0_15",
            "Aquatic0_17",
            
            // right
            // Chest1
            "Chest0_1", "Chest0_4", "Chest0_6",
            "Chest0_8", "Chest0_14", "Money_16",
            // Decor0
            "Decor0_145", "Decor0_64", "Decor0_66",
            "Decor0_69",
            // Door1
            "Door0_0", "Door0_8", "Door0_12",
            "Door0_16", "Door0_37", "Door0_43",
            // Item1
            "Ammo_8", "Ammo_10", "Ammo_9",
            "Ammo_11", "Shield_4", "MedWep_0",
            // Item2
            "Armor_0", "Armor_2", "Armor_4",
            "Armor_6", "Armor_8", "Armor_24",
            "Armor_26", "Armor_28",
            // Item3
            "Book_0", "Book_2", "Book_4",
            "Book_6", "Book_8", "Book_10",
            "Book_12", "Book_14", "Book_16",
            // Item4
            "Food_0", "Food_2", "Food_4",
            "Food_6", "Food_8", "Food_10",
            "Food_12", "Food_14", "Food_16",
        };
            
        // sprite animation 1
        private List<string> _backgroundSprite1NameList = new List<string> {
            // Left
            // Player1
            "Player1_17", "Player1_18", "Player1_19", "Player1_20", "Player1_21",
            "Player1_22", "Player1_24", "Player1_25", "Player1_26",
            // Player2
            "Player1_52", "Player1_53", "Player1_54", "Player1_55", "Player1_56",
            // Player3 
            "Player1_59", "Player1_60", "Player1_61", "Player1_62", "Player1_63",
            "Player1_64", "Player1_65",
            // Player4
            "Player1_71", "Player1_72", "Player1_73", "Player1_74", "Player1_75",
            "Player1_76",
            // Player5
            "Player1_67", "Player1_68",
            //Quadraped1
            "Quadraped1_9", "Quadraped1_11", "Quadraped1_16",
            "Quadraped1_22", "Quadraped1_27", "Quadraped1_29",
            "Quadraped1_43",
            // Aquatic1
            "Aquatic1_1", "Aquatic1_5", "Aquatic1_7",
            "Aquatic1_10", "Aquatic1_11", "Aquatic1_15",
            "Aquatic1_17",
            
            // right
            // Chest1
            "Chest1_1", "Chest1_4", "Chest1_6",
            "Chest1_8", "Chest1_14", "Money_17",
            // Decor1
            "Decor1_145", "Decor1_64", "Decor1_66",
            "Decor1_69",
            // Door1
            "Door1_0", "Door1_8", "Door1_12",
            "Door1_16", "Door1_37", "Door1_43",
            // Item1
            "Ammo_14", "Ammo_16", "Ammo_15",
            "Ammo_17", "Shield_5", "MedWep_1",
            // Item2
            "Armor_1", "Armor_3", "Armor_5",
            "Armor_7", "Armor_9", "Armor_25",
            "Armor_27", "Armor_29",
            // Item3
            "Book_1", "Book_3", "Book_5",
            "Book_7", "Book_9", "Book_11",
            "Book_13", "Book_15", "Book_17",
            // Item4
            "Food_1", "Food_3", "Food_5",
            "Food_7", "Food_9", "Food_11",
            "Food_13", "Food_15", "Food_17",
        };

        // sprite position
        private List<Vector3> _backgroundSpriteVectorList = new List<Vector3> {
            // Left
            // Player1
            new Vector3(-4.25f, 1.5f, 0), new Vector3(-4, 1.5f, 0), new Vector3(-3.75f, 1.5f, 0),
            new Vector3(-3.5f, 1.5f, 0), new Vector3(-3.25f, 1.5f, 0), new Vector3(-3, 1.5f, 0),
            new Vector3(-2.75f, 1.5f, 0), new Vector3(-2.5f, 1.5f, 0), new Vector3(-2.25f, 1.5f, 0),
            // Player2
            new Vector3(-4.25f, 1.25f, 0), new Vector3(-4, 1.25f, 0), new Vector3(-3.75f, 1.25f, 0),
            new Vector3(-2.75f, 1.25f, 0), new Vector3(-2.25f, 1.25f, 0),
            // Player3 
            new Vector3(-4.25f, 1, 0), new Vector3(-3.75f, 1, 0), new Vector3(-3.5f, 1, 0),
            new Vector3(-3, 1, 0), new Vector3(-2.75f, 1, 0), new Vector3(-2.5f, 1, 0),
            new Vector3(-2.25f, 1, 0),
            // Player4
            new Vector3(-4.25f, 0.5f, 0), new Vector3(-4, 0.5f, 0), new Vector3(-3.25f, 0.5f, 0),
            new Vector3(-3, 0.5f, 0), new Vector3(-2.5f, 0.5f, 0), new Vector3(-2.25f, 0.5f, 0),
            // Player5
            new Vector3(-3.75f, 0.25f, 0), new Vector3(-3.5f, 0.25f, 0),
            //Quadraped1
            new Vector3(-4.25f, 0, 0), new Vector3(-4, 0, 0), new Vector3(-3.5f, 0, 0),
            new Vector3(-3.25f, 0, 0), new Vector3(-3, 0, 0), new Vector3(-2.5f, 0, 0),
            new Vector3(-2.25f, 0, 0),
            // Aquatic1
            new Vector3(-4.25f, -0.5f, 0), new Vector3(-4, -0.5f, 0), new Vector3(-3.5f, -0.5f, 0),
            new Vector3(-3.25f, -0.5f, 0), new Vector3(-2.75f, -0.5f, 0), new Vector3(-2.5f, -0.5f, 0),
            new Vector3(-2.25f, -0.5f, 0),
            
            // right
            // Chest1
            new Vector3(4.25f, 1.5f, 0), new Vector3(4, 1.5f, 0), new Vector3(3.25f, 1.5f, 0),
            new Vector3(3, 1.5f, 0), new Vector3(2.75f, 1.5f, 0), new Vector3(2.25f, 1.5f, 0),
            // Decor1
            new Vector3(4, 1, 0), new Vector3(3.5f, 1, 0), new Vector3(2.75f, 1, 0),
            new Vector3(2.5f, 1, 0),
            // Door1
            new Vector3(4.25f, 0.75f, 0), new Vector3(4, 0.75f, 0), new Vector3(3.75f, 0.75f, 0),
            new Vector3(3.25f, 0.75f, 0), new Vector3(3, 0.75f, 0), new Vector3(2.25f, 0.75f, 0),
            // Item1
            new Vector3(4.25f, 0.25f, 0), new Vector3(4, 0.25f, 0), new Vector3(3.5f, 0.25f, 0),
            new Vector3(3.25f, 0.25f, 0), new Vector3(2.75f, 0.25f, 0), new Vector3(2.25f, 0.25f, 0),
            // Item2
            new Vector3(4.25f, 0, 0), new Vector3(3.75f, 0, 0), new Vector3(3.5f, 0, 0),
            new Vector3(3.25f, 0, 0), new Vector3(3, 0, 0), new Vector3(2.75f, 0, 0),
            new Vector3(2.5f, 0, 0), new Vector3(2.25f, 0, 0),
            // Item3
            new Vector3(4.25f, -0.25f, 0), new Vector3(4, -0.25f, 0), new Vector3(3.75f, -0.25f, 0),
            new Vector3(3.5f, -0.25f, 0), new Vector3(3.25f, -0.25f, 0), new Vector3(3, -0.25f, 0),
            new Vector3(2.75f, -0.25f, 0), new Vector3(2.5f, -0.25f, 0), new Vector3(2.25f, -0.25f, 0), 
            // Item4
            new Vector3(4.25f, -0.5f, 0), new Vector3(4, -0.5f, 0), new Vector3(3.75f, -0.5f, 0),
            new Vector3(3.5f, -0.5f, 0), new Vector3(3.25f, -0.5f, 0), new Vector3(3, -0.5f, 0),
            new Vector3(2.75f, -0.5f, 0), new Vector3(2.5f, -0.5f, 0), new Vector3(2.25f, -0.5f, 0), 
        };

        private void Start()
        {
            _resolution = new Vector2(Screen.width, Screen.height);
            
            SetupBackgroundSprite();
        }
        
        private void Update()
        {
            if (Math.Abs(_resolution.x - Screen.width) > 0 || Math.Abs(_resolution.y - Screen.height) > 0) {
                _resolution.x = Screen.width;
                _resolution.y = Screen.height;
                
                SetupBackgroundSprite();
            }
            
            
            if (!(Time.time >= _nextUpdateSecond)) {
                return;
            }

            _nextUpdateSecond = Mathf.FloorToInt(Time.time) + 1;
            
            foreach (var backgroundObject in _backgroundObjectList) {
                backgroundObject.ChangeSprite(_nextUpdateSecond % 2);
            }
        }
        
        private void SetupBackgroundSprite()
        {
            if (_backgroundObjectList.Count > 0) {
                for (var i = 0; i < _backgroundObjectList.Count; i++) {
                    _backgroundObjectList[i].GameObject.transform.position = _backgroundSpriteVectorList[i];
                }
            } else {
                for (var i = 0; i < _backgroundSpriteVectorList.Count; i++) {
                    var spriteList = new List<Sprite> {
                        SpriteManager.Instance.GetSprite(_backgroundSprite0NameList[i]),
                        SpriteManager.Instance.GetSprite(_backgroundSprite1NameList[i])
                    };

                    var spriteObject = new GameObject();

                    spriteObject.AddComponent<SpriteRenderer>().sprite = spriteList[0];
                    
                    spriteObject.transform.position = _backgroundSpriteVectorList[i];
                    spriteObject.transform.localScale = new Vector3(0.25f, 0.25f, 0);
                    
                    spriteObject.transform.SetParent(transform, true);

                    var backgroundObject = new BackgroundObject {
                        GameObject = spriteObject,
                        SpriteList = spriteList
                    };

                    _backgroundObjectList.Add(backgroundObject);
                }
            }
        }
    }
    
    public struct BackgroundObject
    {
        public GameObject GameObject;
        public List<Sprite> SpriteList;

        public void ChangeSprite(int index)
        {
            GameObject.GetComponent<SpriteRenderer>().sprite = SpriteList[index];
        }
    }
}
