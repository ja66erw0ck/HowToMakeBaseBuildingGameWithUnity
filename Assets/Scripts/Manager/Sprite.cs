using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;
using SpriteManager = Manager.Sprite;

namespace Manager
{
    public class Sprite
    {
        private static SpriteManager _instance;
        public static SpriteManager Instance => _instance ?? new SpriteManager(); 
        
        private readonly Dictionary<string, UnityEngine.Sprite> _sprites;
        private readonly Dictionary<string, Texture2D> _textures;
        private readonly UnityEngine.Sprite _nullSprite;

        public Sprite()
        {
            _instance = this; 
            
            _sprites = new Dictionary<string, UnityEngine.Sprite>();
            _textures = new Dictionary<string, Texture2D>();
            _nullSprite = Resources.Load<UnityEngine.Sprite>("Sprites/Null");
            
            LoadSpritesFromDirectory(Path.Combine(Application.streamingAssetsPath, "Sprites"));
        }
        
        public void Destroy()
        {
            _instance = null;
        }

        private void LoadSpritesFromDirectory(string filePath)
        {
            var subDirs = Directory.GetDirectories(filePath);
            foreach (var dir in subDirs) {
                LoadSpritesFromDirectory(dir);
            }

            var filesInDir = Directory.GetFiles(filePath);
            foreach (var file in filesInDir) {
                if (file.EndsWith(".png")) {
                    LoadSprite(file);
                }
            }
        }
        
        private Texture2D LoadImage(string filePath)
        {
            filePath = Path.Combine(Application.streamingAssetsPath, filePath);
            if (_textures.ContainsKey(filePath)) {
                return _textures[filePath];
            }
            
            var imageBytes = System.IO.File.ReadAllBytes(filePath);
            var texture = new Texture2D(1, 1); // create some kind of dummy instance of Texture2D
            
            texture.filterMode = FilterMode.Point;
            
            texture.LoadImage(imageBytes);
            _textures[filePath] = texture;
            return texture;
        }

        private void LoadSprite(string filePath)
        {
            var baseImageName = Path.GetFileNameWithoutExtension(filePath);
            var basePath = Path.GetDirectoryName(filePath);
            
            var xmlPath = Path.Combine(basePath, baseImageName + ".xml");
            if (!File.Exists(xmlPath)) {
                Debug.Log("* No xml for that image " + xmlPath);
                return;
            }

            var xmlText = File.ReadAllText(xmlPath);
            var reader = new XmlTextReader(new StringReader(xmlText));
            
            var texture = LoadImage(filePath);

            if (texture == null) {
                Debug.LogError("! No image that xml " + filePath);
                return;
            }
                
            // set our cursor on the first sprite we find.
            if (reader.ReadToDescendant("Sprites") && reader.ReadToDescendant("Sprite")) {
                do {
                    ReadSpriteFromXml(reader, texture);
                } while (reader.ReadToNextSibling("Sprite"));
            } else {
                Debug.LogError("! could not find a proper tag.");
                reader.Close();
                return;
            }

            reader.Close();
        }

        private void ReadSpriteFromXml(XmlReader parent, Texture2D texture)
        {
            var name = parent.GetAttribute("Name");
            var reader = parent.ReadSubtree();
            
            var rect = new Rect(0, 0, 16, 16); // default value? if not exist?
            var pivot = new Vector2(0.5f, 0.5f);
            var pixelsPerUnit = 16;
            while (reader.Read()) {
                switch (reader.Name) {
                    case "Rect":
                        rect.x = int.Parse(reader.GetAttribute("X"));
                        rect.y = int.Parse(reader.GetAttribute("Y"));
                        rect.width = int.Parse(reader.GetAttribute("Width"));
                        rect.height = int.Parse(reader.GetAttribute("Height"));
                        break;
                    case "Pivot":
                        pivot.x = float.Parse(reader.GetAttribute("X"));
                        pivot.y = float.Parse(reader.GetAttribute("Y"));
                        break;
                    case "Unit":
                        pixelsPerUnit = int.Parse(reader.GetAttribute("Pixels"));
                        break;
                }
            }
          
            _sprites[name] = UnityEngine.Sprite.Create(texture, rect, pivot, pixelsPerUnit);
        }


        public UnityEngine.Sprite GetSprite(string name)
        {
            if (name == null) {
                return _nullSprite;
            }
            
            if (_sprites.ContainsKey(name)) {
                return _sprites[name];
            }

            Debug.LogError("! No sprite with name: " + name);
            return _nullSprite;
        }
    }
}
