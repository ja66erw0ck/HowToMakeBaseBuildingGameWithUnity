using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu
{
    public class Copyright : MonoBehaviour
    {
        [SerializeField] private Text license; 
        [SerializeField] private Text sound;
        [SerializeField] private Text dawnlike;
        
        private void Start()
        {
            //license.gameObject.SetActive(true);
            //sound.gameObject.SetActive(true);
            
            dawnlike.gameObject.SetActive(true);
            // todo localization
            dawnlike.text = "Graphic Assets \"DawnLike - 16x16 Universal Rogue-like tileset v1.81\"";
            dawnlike.text += " by DawnBringer & DragonDePlatino licensed CC-BY 4.0.";
            dawnlike.text += "\n";
            dawnlike.text += "https://opengameart.org/content/dawnlike-16x16-universal-rogue-like-tileset-v181";
        }
    }
}
