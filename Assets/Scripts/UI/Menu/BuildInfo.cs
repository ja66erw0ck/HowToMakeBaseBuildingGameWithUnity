using UnityEngine;
using UnityEngine.UI;
using GameController = Controller.Game;

namespace UI.Menu
{
    public class BuildInfo : MonoBehaviour
    {
        [SerializeField] private Text version; 
        
        private void Start()
        {
            version.gameObject.SetActive(true);
            // todo localization 
            version.text = "Version " + GameController.GameVersion;
        }
    }
}
