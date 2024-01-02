using Model;
using UnityEngine;
using TileType = Type.Tile;
using WorldModel = Model.World;
using TileModel = Model.Tile;

namespace Controller
{
    public class Sound : MonoBehaviour
    {
        private float _soundCooldown = 0f;
        
        private void Start()
        {
            WorldModel.Current.StructureManager.RegisterStructureCreated(OnStructureCreated);
            WorldModel.Current.RegisterTileChanged(OnTileChanged);
        }

        private void Update()
        {
            _soundCooldown -= Time.deltaTime;
        }

        private void OnTileChanged(TileModel tile)
        {
            if (_soundCooldown > 0f) {
                return;
            }

            var soundName = "Sounds/stone3"; // TileType.Floor
            if (tile.Type == TileType.Empty) {
                soundName = "Sounds/stone2"; 
            }
            
            // FIXME
            var effect = Resources.Load<AudioClip>(soundName);
            AudioSource.PlayClipAtPoint(effect, Camera.main.transform.position);
            _soundCooldown = 0.1f;
        }
        
        private void OnStructureCreated(Structure structure)
        {
            if (_soundCooldown > 0f) {
                return;
            }
           
            var soundName = "Sounds/stone1"; //
            if (structure.Type == "BrickWall")
            {
                soundName = "Sounds/stone1"; // structure object type Wall
            }
            
            // FIXME
            var effect = Resources.Load<AudioClip>(soundName);
            if (effect == null) {
                // use default sound
            }
            AudioSource.PlayClipAtPoint(effect, Camera.main.transform.position);
            _soundCooldown = 0.1f;
        }
    }
}
