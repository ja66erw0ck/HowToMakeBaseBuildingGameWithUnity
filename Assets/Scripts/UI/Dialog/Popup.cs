using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using ButtonExtended = UI.Extended.Button;

namespace UI.Dialog
{
    public class Popup : DialogBox
    {
        [SerializeField] private Button okay;
        
        public void SetOkayName(string name)
        {
            okay.gameObject.GetComponent<ButtonExtended>().SetName(name);
        }

        public void SetOkayButtonClick(UnityAction call)
        {
            okay.gameObject.SetActive(true);
            okay.GetComponent<ButtonExtended>().SetButtonClick(call);
        }
    }
}
