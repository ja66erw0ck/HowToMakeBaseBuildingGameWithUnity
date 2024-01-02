using UnityEngine;
using UnityEngine.SceneManagement;
using SceneLoader = UI.SceneLoader;
using TimeManager = Manager.Time;
using SpriteManager = Manager.Sprite;
using KeyboardManager = Manager.Keyboard;
using MappedInputKeyboardType = Type.Keyboard.MappedInput;
using GameController = Controller.Game;

namespace Controller
{
    public class Game : MonoBehaviour
    {
        private static GameController Instance { get; set; }
        
        public const string GameVersion = "0.0.1";

        //////////////////////////////
        /// MonoBehaviour
        //////////////////////////////

        private void Awake()
        {
            if (Instance == null || Instance == this) {
                Instance = this;
            } else {
                Debug.Log($"! Two {name} exist, deleting the new version rather than the old.");
                Destroy(gameObject);
            }

            DontDestroyOnLoad(this);

            KeyboardManager.Instance.RegisterInputAction(
                "Pause", MappedInputKeyboardType.KeyUp , () => {
                    TimeManager.Instance.IsPaused = !TimeManager.Instance.IsPaused;
                });
        }

        private void Update()
        {
            TimeManager.Instance.Update(Time.deltaTime);
        }
        
        //////////////////////////////
        /// SceneHelper
        //////////////////////////////
        
        private const string GameSceneName = "Game";
        private const string MainSceneName = "Main";
        public static string LoadWorldFromFile { get; set; } 
        public static Vector3 NewWorldSize { get; private set; } 
        public static int Seed { get; private set; }

        public static void NewGameWorld(int width, int height, int depth, int seed)
        {
            NewWorldSize = new Vector3(width, height, depth);
            Seed = seed;
            CleanInstancesBeforeLoadingScene();
            SceneLoader.Instance.LoadScene(GameSceneName); 
        }
        
        // Load a Save File.
        public static void LoadGameWorld(string fileName)
        {
            LoadWorldFromFile = fileName;
            CleanInstancesBeforeLoadingScene();
            SceneLoader.Instance.LoadScene(GameSceneName);
        }

        // Back To Main Menu.
        public static void BackToMainMenu()
        {
            //SceneLoader.Instance.LoadScene(MainSceneName);
            CleanInstancesBeforeLoadingScene();
            SceneManager.LoadSceneAsync(MainSceneName);
        }

        // Quit the app whether in editor or a build version.
        public static void QuitGame()
        {
            // Maybe ask the user if he want to save or is sure they want to quit??
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        
        // Path to the saves folder.
        public static string FileSaveBasePath()
        {
            return System.IO.Path.Combine(Application.streamingAssetsPath, "Saves");
        }
        
        private static void CleanInstancesBeforeLoadingScene()
        {
            TimeManager.Instance.Destroy();
            KeyboardManager.Instance.Destroy();
            SpriteManager.Instance.Destroy();
        }                
    }
}
