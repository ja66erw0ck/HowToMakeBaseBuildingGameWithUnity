using System.IO;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using GameController = Controller.Game;
using TimeManager = Manager.Time;
using WorldModel = Model.World;

namespace Controller
{
    public class World : MonoBehaviour
    {
        private WorldModel _world;

        private void OnEnable()
        {
            TimeManager.Instance.OpenedModalCount = 0;
           
            if (GameController.LoadWorldFromFile != null) {
                CreateWorldFromSaveFile(GameController.LoadWorldFromFile);
                GameController.LoadWorldFromFile = null;
            } else {
                CreateEmptyWorld();
            }
        }

        public static void Save(string filePath, bool isBson = false)
        {
            if (!Directory.Exists(GameController.FileSaveBasePath())) {
                Directory.CreateDirectory(GameController.FileSaveBasePath());
            }
            
            var json = WorldModel.Current.ToJson();
            var streamWriter = new StreamWriter(filePath);
            // TODO: make type format.Bson format.Json
            var writer = new JsonTextWriter(streamWriter);
            
            // launch saving operation in a separate thread.
            var thread = new Thread(new ThreadStart(delegate { Save((JObject) json, writer); }));
            thread.Start();
        }

        private static void Save(JObject json, JsonWriter writer)
        {
            // TODO: bson format???
            var serializer = new JsonSerializer() {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            };
            serializer.Serialize(writer, json);
            writer.Flush();
        }

        public void CameraToCenter()
        {
            Camera.main.transform.position = new Vector3(_world.Width / 2, _world.Height / 2, Camera.main.transform.position.z);
        }

        private void CreateEmptyWorld()
        {
            var newWorldSize = GameController.NewWorldSize;
            var seed = GameController.Seed;
            
            _world = newWorldSize == Vector3.zero 
                ? new WorldModel(24, 24, 2) 
                : new WorldModel((int) newWorldSize.x, (int) newWorldSize.y, (int) newWorldSize.z);
            
            CameraToCenter();
        }

        private void CreateWorldFromSaveFile(string loadWorldFromFile)
        {
            var reader = File.OpenText(loadWorldFromFile);

            _world = new WorldModel(); 
            _world.FromJson(JToken.ReadFrom(new JsonTextReader(reader)));
            
            CameraToCenter();
        }

        public void SpawnCharacter()
        {
            var types = new[] {
                "Warrior", "Priest", "Wizard"
            };
            var index = Random.Range(0, types.Length);
        
            _world.CreateCharacter(_world.GetTileModelAt(_world.Width/2, _world.Height/2, 0), 5f, types[index]);
        }
    }
}