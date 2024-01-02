using UnityEngine;
using UnityEngine.UI;
using MouseController = Controller.Mouse;

namespace UI
{
    public class MouseOverCreatureInfo : MonoBehaviour
    {
        [SerializeField] private Text creatureInfoText;

        private void Update()
        {
            var tile = MouseController.Instance.GetMouseOverTile();
            
            if (tile == null || tile.Creatures.Count == 0) {
                creatureInfoText.gameObject.SetActive(false);
                return;
            }

            string creatureInfo = null;
            foreach (var creature in tile.Creatures) {
                creatureInfo += creature.Type;
                if (creature.Item != null) {
                    creatureInfo += " (" + creature.Item.StackSize + "/" + creature.Item.MaxStackSize + ")";
                }
                creatureInfo += "\n";
            } 
            
            creatureInfoText.gameObject.SetActive(true);
            creatureInfoText.text = creatureInfo;
        }
    }
}
