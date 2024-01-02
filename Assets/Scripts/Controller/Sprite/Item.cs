using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using LayerType = Type.Layer;
using WorldModel = Model.World;
using ItemModel  = Model.Item;
using SpriteManager = Manager.Sprite;

namespace Controller.Sprite
{
    public class Item : MonoBehaviour
    {
        [SerializeField] private GameObject itemUIPrefab;
        [SerializeField] private Transform itemTransform;
        
        //
        public Dictionary<ItemModel, GameObject> ItemObjects { get; protected set; }

        private void Start()
        {
            // Instantiate our dictionary that tracks which gameObject is rendering which tile data.
            ItemObjects = new Dictionary<ItemModel, GameObject>();

            // Register our callback so that our GameObject gets updated whenever
            // the tile's type changes.
            WorldModel.Current.ItemManager.RegisterItemCreated(OnItemCreated);
            
            // check for pre-existing items, which won't do the callback
            foreach (var item in WorldModel.Current.ItemManager.Items.Keys.SelectMany(
                type => WorldModel.Current.ItemManager.Items[type]
            )) {
                OnItemCreated(item);
            }
        }
        
        public void OnItemCreated(ItemModel item)
        {
            // create a visual GameObject linked to this data.
            
            // This creates a new GameObject and adds it to our scene
            var itemObject = new GameObject {
                name = item.Type
            };
            
            // TODO
            string sprite = null;
            if (item.Type == "Brick") {
                sprite = "Wall_32";
            }

            itemObject.AddComponent<SpriteRenderer>().sprite = SpriteManager.Instance.GetSprite(sprite);
            
            // FIXME : set up so fast?
            itemObject.transform.position = new Vector3(item.Tile.X, item.Tile.Y, item.Tile.Z - LayerType.Item);
            itemObject.transform.SetParent(itemTransform, true);

            ItemObjects.Add(item, itemObject);

            if (item.MaxStackSize > 1) {
                // This is a stackable object, so let's add a ItemUI component
                // (which is text that shows the current stackSize.)
                var itemUIObject = Instantiate(itemUIPrefab);
                itemUIObject.transform.SetParent(itemObject.transform);
                itemUIObject.transform.localPosition = Vector3.zero; // if we change the sprite anchor, this may need to be modified
                itemUIObject.GetComponentInChildren<Text>().text = item.StackSize.ToString();
            }
            
            // register our callback so that our game objects gets updated whenever
            // the object's into changes.
            item.RegisterChangedCallback(OnItemChanged);
        }

        private void OnItemChanged(ItemModel item)
        {
            // make sure the item's graphics are correct.
            if (ItemObjects.ContainsKey(item) == false) {
                Debug.Log("! OnItemChanged -- trying to change visuals for item not in our map.");
                return;
            }

            var itemObject = ItemObjects[item];
            if (item.StackSize > 0) {
                var text = itemObject.GetComponentInChildren<Text>();
                if (text != null) {
                    text.text = item.StackSize.ToString();
                }
            } else {
                // this stack has gone to zero, so remove the sprite!
                item.UnregisterChangedCallback(OnItemChanged);
                ItemObjects.Remove(item);
                Destroy(itemObject);
            }
        }
    }
}