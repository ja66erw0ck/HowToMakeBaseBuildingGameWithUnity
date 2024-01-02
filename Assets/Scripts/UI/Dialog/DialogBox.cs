using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using ButtonExtended = UI.Extended.Button;
using TimeManager = Manager.Time;

namespace UI.Dialog
{
    public class DialogBox : MonoBehaviour
    {
        [SerializeField] private Text title;
        
        [SerializeField] private Button confirm;
        [SerializeField] private Button cancel;
        
        public void SetTitleName(string name)
        {
            title.text = name;
        }
        
        public void SetConfirmName(string name)
        {
            confirm.gameObject.GetComponent<ButtonExtended>().SetName(name);
        }
        
        public void SetConfirmButtonClick(UnityAction call)
        {
            confirm.gameObject.SetActive(true);
            confirm.GetComponent<ButtonExtended>().SetButtonClick(call);
        }
        
        public void SetCancelName(string name)
        {
            cancel.gameObject.GetComponent<ButtonExtended>().SetName(name);
        }
        
        public void SetCancelButtonClick(UnityAction call)
        {
            cancel.gameObject.SetActive(true);
            cancel.GetComponent<ButtonExtended>().SetButtonClick(call);
        }
        
        public virtual void OpenDialog()
        {
            TimeManager.Instance.IsModal = true;
            gameObject.SetActive(true);
        }

        public virtual void CloseDialog()
        {
            TimeManager.Instance.IsModal = false;
            gameObject.SetActive(false);
        }
    }
}
