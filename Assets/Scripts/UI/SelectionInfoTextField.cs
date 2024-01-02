using Controller;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SelectionInfoTextField : MonoBehaviour
    {
        [SerializeField] private Text selectionInfoText;
        [SerializeField] private CanvasGroup canvasGroup;

        private Mouse _mouseController;
        
        private void Start()
        {
            _mouseController = FindObjectOfType<Mouse>();
        }

        private void Update()
        {
            if (_mouseController.Selection == null) {
                canvasGroup.alpha = 0;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                return;
            }
            
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            var actualSelection = _mouseController.Selection.StuffInTile[_mouseController.Selection.SubSelection];
            selectionInfoText.text = actualSelection.GetName() + "\n" + actualSelection.GetDescription() + "\n" + actualSelection.GetHitPointString();
        }
    }
}
