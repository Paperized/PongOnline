using FishNet;
using System.Collections;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private ListRoomsUI _roomsUI;

    public void MakeNewRoom()
    {
        if(InstanceFinder.IsClient)
        {
            RoomManager.Instance.MakeNewRoomRPC(InstanceFinder.ClientManager.Connection);
        }
    }

    public void JoinSelectedRoom()
    {
        if (InstanceFinder.IsClient)
        {
            if(_roomsUI.CurrentRoomIdSelected >= 0)
            {
                RoomManager.Instance.JoinRoomRPC(_roomsUI.CurrentRoomIdSelected);
            }
        }
    }
}
