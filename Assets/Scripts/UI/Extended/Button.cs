using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Extended
{
    public class Button : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Button button;
        [SerializeField] private bool usePressedSprite = true;
        [SerializeField] private bool useSelectedSprite = true;
        [SerializeField] private Text buttonName;
        
        private void Awake()
        {
            if (!usePressedSprite || !useSelectedSprite) {
                SetUseSprite(usePressedSprite, useSelectedSprite);
            }
        }

        public void SetUseSprite(bool isUsePressed, bool isUseSelected)
        {
            var spriteState = new SpriteState();
            spriteState = gameObject.GetComponent<UnityEngine.UI.Button>().spriteState;
            
            if (!isUsePressed) {
                spriteState.pressedSprite = null;
            }
            
            if (!isUseSelected) {
                spriteState.selectedSprite = null;
            }
            
            gameObject.GetComponent<UnityEngine.UI.Button>().spriteState = spriteState;
            usePressedSprite = isUsePressed;
            useSelectedSprite = isUseSelected;
        }

        public void SetName(string name)
        {
            buttonName.text = name;
        }

        public void SetButtonClick(UnityAction call)
        {
            button.onClick.AddListener(call);
        }
    }
}
