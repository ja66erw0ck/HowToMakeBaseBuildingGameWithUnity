using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class FileListItem : MonoBehaviour, IPointerClickHandler
    {
        public InputField InputField;

        ///
        /// IPointerClickHandler
        ///
        public void OnPointerClick(PointerEventData eventData)
        {
            // our job is to take our text label and 
            // copy it into a target field.
            
            InputField.text = transform.GetComponentInChildren<Text>().text;
        }
    }
}
