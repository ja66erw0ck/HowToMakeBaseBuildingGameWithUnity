using UnityEngine;
using TitleExtended =  UI.Extended.Title;

namespace UI.Menu
{
    public class Title : MonoBehaviour
    {
        private void Start()
        {
            var titlePrefab = Resources.Load("Prefabs/UI/Title") as GameObject;
            
            var titleObject = Instantiate(titlePrefab, transform, false);
            if (titleObject is null) {
                Debug.LogError("! There is No Title Prefab");
                return;
            }

            titleObject.GetComponent<TitleExtended>().SetTitle("Unity Base Building Game"); // todo localization
            titleObject.GetComponent<TitleExtended>().SetSubHeading("Tutorial"); // todo localization
        }
    }
}
