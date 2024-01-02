using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using ButtonExtended = UI.Extended.Button;
using GameController = Controller.Game;
using WorldController = Controller.World;

namespace UI.Dialog
{
    public class LoadSaveFile : DialogBox
    {
        [SerializeField] private GameObject fileList;
        [SerializeField] private GameObject fileListItemPrefab;
        [SerializeField] private float childHeight = 24f;
        [SerializeField] private bool isLoadFileDialog;
        [SerializeField] private Text placeHolder;
        [SerializeField] private Button delete;
        
        public override void OpenDialog()
        {
            base.OpenDialog();

            var directoryPath = GameController.FileSaveBasePath();

            var saveDir = new DirectoryInfo(directoryPath);
            if (!saveDir.Exists) {
                saveDir.Create();    
            }
            var saveGames = saveDir.GetFiles("*.json").OrderByDescending(file => file.CreationTime).ToArray();
            
            var inputField = gameObject.GetComponentInChildren<InputField>();
            foreach (var save in saveGames) {
                var gameObject = Instantiate(fileListItemPrefab);
                gameObject.transform.SetParent(fileList.transform);
              
                gameObject.GetComponentInChildren<Text>().text = Path.GetFileNameWithoutExtension(save.FullName);
                gameObject.GetComponent<FileListItem>().InputField = inputField;
            }
           
            var sizeDelta = fileList.transform.gameObject.GetComponent<RectTransform>().sizeDelta;
            sizeDelta.y = saveGames.Length * childHeight;
            fileList.transform.gameObject.GetComponent<RectTransform>().sizeDelta = sizeDelta;
        }

        public override void CloseDialog()
        {
            // clear out all the children of our file list
            while (fileList.transform.childCount > 0) {
                var child = fileList.transform.GetChild(0);
                child.SetParent(null);
                Destroy(child.gameObject);
            }

            gameObject.GetComponentInChildren<InputField>().text = null;
            base.CloseDialog();
        }

        public void ConfirmWasClicked()
        {
            var fileName = gameObject.GetComponentInChildren<InputField>().text;
            var filePath = Path.Combine(GameController.FileSaveBasePath(), fileName + ".json");
            
            CloseDialog();

            if (isLoadFileDialog) {
                if (!File.Exists(filePath)) {
                    Debug.LogError("! File doesn't exist. " + filePath);
                    return;
                }
                
                GameController.LoadGameWorld(filePath);
            } else {
                //if (File.Exists(filePath)) {
                //    Debug.LogError("! File Already exists, overwriting?");
                //}
                
                WorldController.Save(filePath);
            }
        }

        public void SetLoadFileDialog(bool isLoadBox)
        {
            isLoadFileDialog = isLoadBox;
        }
        
        public void SetPlaceHolderName(string name)
        {
            placeHolder.text = name;
        }
        
        public void SetDeleteName(string name)
        {
            delete.gameObject.GetComponent<ButtonExtended>().SetName(name);
        }
        
        public void SetDeleteButtonClick(UnityAction call)
        {
            delete.gameObject.SetActive(true);
            delete.GetComponent<ButtonExtended>().SetButtonClick(call);
        }

        public void DeleteWasClicked()
        {
            var fileName = gameObject.GetComponentInChildren<InputField>().text;
            var filePath = Path.Combine(GameController.FileSaveBasePath(), fileName + ".json");
            
            CloseDialog();

            if (File.Exists(filePath)) {
                //Debug.Log("* File delete");
                File.Delete(filePath); 
            }
        }
    }
}
