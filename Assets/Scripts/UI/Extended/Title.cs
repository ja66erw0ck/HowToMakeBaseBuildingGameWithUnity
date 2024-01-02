using UnityEngine;
using UnityEngine.UI;

namespace UI.Extended
{
    public class Title : MonoBehaviour
    {
        [SerializeField] private Text title;
        [SerializeField] private Text subHeading;
        
        public void SetTitle(string name)
        {
            title.text = name;
        }
        
        public void SetSubHeading(string name)
        {
            subHeading.text = name;
        }
    }
}
