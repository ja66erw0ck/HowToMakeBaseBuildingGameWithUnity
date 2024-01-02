using UnityEngine;
using UnityEngine.UI;
using MouseController = Controller.Mouse;
using WorldModel = Model.World;

namespace UI
{
    public class MouseOverTileInfo : MonoBehaviour
    {
        [SerializeField] private Text tileInfoText;
        [SerializeField] private Text structureInfoText;
        [SerializeField] private Text itemInfoText;
        [SerializeField] private Text roomInfoText;
        
        // Update is called once per frame
        private void Update()
        {
            var tile = MouseController.Instance.GetMouseOverTile();

            if (tile == null) {
                return;
            }
            
            tileInfoText.text = tile.Type.ToString() + " (" + tile.X + "," + tile.Y + "," + tile.Z + ")";
            structureInfoText.text = tile.Structure?.Name;
            itemInfoText.text = tile.Item != null ? tile.Item.Type + " " + tile.Item.StackSize + "/" + tile.Item.MaxStackSize : "";
            var roomInfo = "room : " + WorldModel.Current.RoomManager.GetRoomId(tile.Room);
            /* TODO
            if (WorldModel.Current.Rooms.IndexOf(tile.Room) > 0 && tile.Room.GetGasNames().Length > 0) {
                roomInfo += " (";
                var last = tile.Room.GetGasNames().Last();
                foreach (var gas in tile.Room.GetGasNames()) {
                    roomInfo += tile.Room.GetGasAmount(gas).ToString("F2"); //F 소수점 N 천단위로 콤마 표현
                    roomInfo += ":" + ((int) tile.Room.GetGasPercentage(gas)) + "%";
                    if (gas.Equals(last)) {
                        roomInfo += ")";
                    } else {
                        roomInfo += ",";
                    }
                } 
            }*/
            roomInfoText.text = roomInfo;
        }
    }
}
