using System.Collections.Generic;
using UI.Dialog;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Button = UI.Extended.Button;
using TimeManager = Manager.Time;
using GameController = Controller.Game;

namespace UI.Menu
{
    public class Main : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private GameObject dialogBox;

        private GameObject _buttonPrefab;
        private GameObject _newGameDialogPrefab;
        private GameObject _loadSaveDialogPrefab;
        private GameObject _popupDialogPrefab;
        private GameObject _optionDialogPrefab;
        
        private void Start()
        {
            _buttonPrefab = Resources.Load("Prefabs/UI/Button") as GameObject;
            _newGameDialogPrefab = Resources.Load("Prefabs/UI/Dialog/NewGame") as GameObject;
            _loadSaveDialogPrefab = Resources.Load("Prefabs/UI/Dialog/LoadSaveFile") as GameObject;
            _popupDialogPrefab = Resources.Load("Prefabs/UI/Dialog/Popup") as GameObject;
            _optionDialogPrefab = Resources.Load("Prefabs/UI/Dialog/Option") as GameObject;
                
            SetupMenu();
        }

        private void SetupMenu()
        {
            var actions = new Dictionary<string, UnityAction>();
            switch (SceneManager.GetActiveScene().name) {
                // Main Scene
                case "Main":
                    actions["New"] = SetupNewGameButton();
                    actions["Resume"] = delegate {
                        Debug.Log("Resume Clicked !!!");
                        // load recent file
                    };
                    actions["Load"] = SetupLoadButton();
                    actions["Option"] = SetupOptionButton();
                    actions["Quit"] = SetupQuitButton();
                    break;
                // Game Scene
                case "Game":
                    actions["Resume"] = CloseMenu;
                    actions["Save"] = SetupSaveButton();
                    actions["Load"] = SetupLoadButton();
                    actions["Option"] = SetupOptionButton();
                    actions["Main"] = GameController.BackToMainMenu;
                    actions["Quit"] = SetupQuitButton();
                    break;
            }
            
            foreach (var pair in actions) {
                var gameObject = Instantiate(_buttonPrefab, transform, true);
                if (gameObject is null) {
                    Debug.LogError("! Instantiate failed");
                    return;
                }

                gameObject.name = pair.Key;
                gameObject.GetComponent<Button>().SetUseSprite(true, false);
                gameObject.GetComponent<Button>().SetName(pair.Key); // todo localization
                gameObject.GetComponent<Button>().SetButtonClick(pair.Value);
            }
        }


        private UnityAction SetupNewGameButton()
        {
            var gameObject = Instantiate(_newGameDialogPrefab, dialogBox.transform, false);
            gameObject.name = "NewGameDialogBox";
            gameObject.GetComponent<NewGame>().SetTitleName("New Game"); // todo localization
            gameObject.GetComponent<NewGame>().SetConfirmName("Confirm"); // todo localization
            gameObject.GetComponent<NewGame>().SetConfirmButtonClick(
                delegate {
                    GameController.NewGameWorld(64, 64, 4, 0);
                }
            );
            gameObject.GetComponent<NewGame>().SetCancelName("Cancel"); // todo localization
            gameObject.GetComponent<NewGame>().SetCancelButtonClick(
                delegate {
                    gameObject.GetComponent<NewGame>().CloseDialog();
                }
            );
            gameObject.SetActive(false);
            
            return delegate {
                gameObject.GetComponent<NewGame>().OpenDialog();
            };
        }

        private UnityAction SetupLoadButton()
        {
            var gameObject = Instantiate(_loadSaveDialogPrefab, dialogBox.transform, false);
            gameObject.name = "LoadFileDialogBox";
            gameObject.GetComponent<LoadSaveFile>().SetTitleName("Load File"); // todo localization
            gameObject.GetComponent<LoadSaveFile>().SetPlaceHolderName("Select a File"); // todo localization
            gameObject.GetComponent<LoadSaveFile>().SetConfirmName("Confirm"); // todo localization
            gameObject.GetComponent<LoadSaveFile>().SetConfirmButtonClick(
                delegate {
                    gameObject.GetComponent<LoadSaveFile>().ConfirmWasClicked();
                }
            );
            gameObject.GetComponent<LoadSaveFile>().SetDeleteName("Delete"); // todo localization
            gameObject.GetComponent<LoadSaveFile>().SetDeleteButtonClick(
                delegate {
                    gameObject.GetComponent<LoadSaveFile>().DeleteWasClicked();
                }
            );
            gameObject.GetComponent<LoadSaveFile>().SetCancelName("Cancel"); // todo localization
            gameObject.GetComponent<LoadSaveFile>().SetCancelButtonClick(
                delegate {
                    gameObject.GetComponent<LoadSaveFile>().CloseDialog();
                }
            );
            gameObject.GetComponent<LoadSaveFile>().SetLoadFileDialog(true);
            gameObject.SetActive(false);
           
            return delegate {
                gameObject.GetComponent<LoadSaveFile>().OpenDialog();
            };
        }
        
        private UnityAction SetupSaveButton()
        {
            var gameObject = Instantiate(_loadSaveDialogPrefab, dialogBox.transform, false);
            gameObject.name = "SaveFileDialogBox";
            gameObject.GetComponent<LoadSaveFile>().SetTitleName("Save File"); // todo localization
            gameObject.GetComponent<LoadSaveFile>().SetPlaceHolderName("Select a File"); // todo localization
            gameObject.GetComponent<LoadSaveFile>().SetConfirmName("Confirm"); // todo localization
            gameObject.GetComponent<LoadSaveFile>().SetConfirmButtonClick(
                delegate {
                    gameObject.GetComponent<LoadSaveFile>().ConfirmWasClicked();
                }
            );
            gameObject.GetComponent<LoadSaveFile>().SetDeleteName("Delete"); // todo localization
            gameObject.GetComponent<LoadSaveFile>().SetDeleteButtonClick(
                delegate {
                    gameObject.GetComponent<LoadSaveFile>().DeleteWasClicked();
                }
            );
            gameObject.GetComponent<LoadSaveFile>().SetCancelName("Cancel"); // todo localization
            gameObject.GetComponent<LoadSaveFile>().SetCancelButtonClick(
                delegate {
                    gameObject.GetComponent<LoadSaveFile>().CloseDialog();
                }
            );
            gameObject.GetComponent<LoadSaveFile>().SetLoadFileDialog(false);
            gameObject.SetActive(false);
           
            return delegate {
                gameObject.GetComponent<LoadSaveFile>().OpenDialog();
            };
        }

        private UnityAction SetupOptionButton()
        {
            var gameObject = Instantiate(_optionDialogPrefab, dialogBox.transform, false);
            gameObject.name = "OptionDialogBox";
            gameObject.GetComponent<Option>().SetTitleName("Option"); // todo localization
            gameObject.GetComponent<Option>().SetConfirmName("Confirm"); // todo localization
            gameObject.GetComponent<Option>().SetConfirmButtonClick(
                delegate { gameObject.GetComponent<Option>().CloseDialog(); }
            );
            gameObject.GetComponent<Option>().SetCancelName("Cancel"); // todo localization
            gameObject.GetComponent<Option>().SetCancelButtonClick(
                delegate { gameObject.GetComponent<Option>().CloseDialog(); }
            );
            gameObject.SetActive(false);

            return delegate { gameObject.GetComponent<Option>().OpenDialog(); };
        }

        private UnityAction SetupQuitButton()
        {
            var gameObject = Instantiate(_popupDialogPrefab, dialogBox.transform, false);
            gameObject.name = "QuitPopupDialogBox";
            gameObject.GetComponent<Popup>().SetTitleName("Quit Game ?"); // todo localization
            gameObject.GetComponent<Popup>().SetConfirmName("Confirm"); // todo localization
            gameObject.GetComponent<Popup>().SetConfirmButtonClick(GameController.QuitGame);
            gameObject.GetComponent<Popup>().SetCancelName("Cancel"); // todo localization
            gameObject.GetComponent<Popup>().SetCancelButtonClick(
                delegate {
                    gameObject.GetComponent<Popup>().CloseDialog();
                }
            );
            gameObject.SetActive(false);
            
            return delegate {
                gameObject.GetComponent<Popup>().OpenDialog();
            };
        }

        public void OpenMenu()
        {
            TimeManager.Instance.IsModal = true;
            background.gameObject.SetActive(true);
            gameObject.SetActive(true);
        }

        private void CloseMenu()
        {
            TimeManager.Instance.IsModal = false;
            background.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}
