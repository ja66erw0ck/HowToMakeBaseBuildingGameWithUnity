using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class SceneLoader : MonoBehaviour
    {
        private static SceneLoader _Instance;
        public static SceneLoader Instance {
            get {
                if (_Instance != null) {
                    return _Instance;
                }
                
                var obj = FindObjectOfType<SceneLoader>();
                _Instance = obj != null ? obj : Create();
                return _Instance; 
            }
            private set => _Instance = value;
        }

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Text tip;
        [SerializeField] private Image progressBar;

        private string _loadSceneName;

        private string[] _jabberwocky = new string[] {
            "’Twas brillig, and the slithy toves Did gyre and gimble in the wabe:",
            "All mimsy were the borogoves, And the mome raths outgrabe.",
            
            "“Beware the Jabberwock, my son! The jaws that bite, the claws that catch!",
            "Beware the Jubjub bird, and shun The frumious Bandersnatch!”",
            
            "He took his vorpal sword in hand; Long time the manxome foe he sought—",
            "So rested he by the Tumtum tree And stood awhile in thought.",
            
            "And, as in uffish thought he stood, The Jabberwock, with eyes of flame,",
            "Came whiffling through the tulgey wood, And burbled as it came!",

            "One, two! One, two! And through and through The vorpal blade went snicker-snack!",
            "He left it dead, and with its head He went galumphing back.",

            "“And hast thou slain the Jabberwock? Come to my arms, my beamish boy!",
            "O frabjous day! Callooh! Callay!” He chortled in his joy.",

            "’Twas brillig, and the slithy toves Did gyre and gimble in the wabe:",
            "All mimsy were the borogoves, And the mome raths outgrabe."
        };

        private static SceneLoader Create()
        {
            var prefab = Resources.Load<SceneLoader>("Prefabs/SceneLoader");
            return Instantiate(prefab);
        }
        
        private void Awake()
        {
            if (Instance == null || Instance == this) {
                Instance = this;
            } else {
                Debug.LogError($"! Two {name} exist, deleting the new version rather than the old.");
                Destroy(gameObject);
            }
        }
        
        public void LoadScene(string sceneName)
        {
            gameObject.SetActive(true);
            SceneManager.sceneLoaded += LoadSceneEnd;
            _loadSceneName = sceneName;
            
            StartCoroutine(Load(sceneName));
        }

        private IEnumerator Load(string sceneName)
        {
            progressBar.fillAmount = 0f;
            yield return StartCoroutine(Fade(true));

            var operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;
            var timer = 0.0f;
            while (!operation.isDone) {
                yield return null;
                
                tip.text = _jabberwocky[Convert.ToInt32(timer) % _jabberwocky.Length];

                timer += Time.unscaledDeltaTime;
                if (operation.progress < 0.9f) {
                    progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, operation.progress, timer);
                    if (progressBar.fillAmount >= operation.progress) {
                        timer = 0f;
                    }
                } else {
                    progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, 1f, timer);
                    if (progressBar.fillAmount != 1.0f) {
                        continue;
                    }

                    operation.allowSceneActivation = true;
                    yield break;
                }
            }
        }

        private void LoadSceneEnd(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene.name != _loadSceneName) {
                return;
            }

            SceneManager.sceneLoaded -= LoadSceneEnd;
        }

        private IEnumerator Fade(bool isFadeIn)
        {
            var timer = 0f;
            while (timer <= 1f) {
                yield return null;

                timer += Time.unscaledDeltaTime * 2f;
                canvasGroup.alpha = Mathf.Lerp(isFadeIn ? 0 : 1, isFadeIn ? 1 : 0, timer);
            }
            
            if (!isFadeIn) {
                gameObject.SetActive(false);
            }
        }
    }
}
