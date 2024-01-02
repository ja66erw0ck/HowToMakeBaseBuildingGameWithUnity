using UnityEngine;
using UnityEngine.UI;
using WorldModel = Model.World;
using BuildModeController = Controller.BuildMode;

namespace UI
{
    public class BuildStructureMenu : MonoBehaviour
    {
        [SerializeField]
        private GameObject buttonPrefab;
        [SerializeField]
        private int width = 120;
        [SerializeField]
        private int height = 32;
    
        private void Start()
        {
            var buildModeController = FindObjectOfType<BuildModeController>();
            foreach (var structure in WorldModel.Current.StructurePrototypes.Keys) {
                var gameObject = Instantiate(buttonPrefab, transform, true);

                var objectId = structure;
                var objectName = WorldModel.Current.StructurePrototypes[structure].Name;

                gameObject.name = objectId;
                gameObject.transform.GetComponentInChildren<Text>().text = objectName;

                var button = gameObject.GetComponent<Button>();

                button.onClick.AddListener(
                    delegate {
                        buildModeController.BuildStructure(objectId);
                    });
            }
       
            //
            var count = WorldModel.Current.StructurePrototypes.Count;
            var verticalLayoutGroup = GetComponent<VerticalLayoutGroup>();
            var spacing = verticalLayoutGroup.spacing;
            var rectTransform = GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(width, height * count + spacing * (count - 1));
        }
    }
}
