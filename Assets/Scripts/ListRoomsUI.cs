using Data;
using FishNet;
using System.Collections;
using UnityEngine;

public class ListRoomsUI : MonoBehaviour
{
    [SerializeField]
    private RoomItemListUI roomElementPrefab;
    [SerializeField]
    private GameObject listViewContent;

    private int currentRoomIdSelected = -1;
    public int CurrentRoomIdSelected => currentRoomIdSelected;
    bool isSubscribed;

    private void Start()
    {
        ClearItems();
        StartCoroutine(SubscribeRoomManager(true));
    }

    private IEnumerator SubscribeRoomManager(bool subscribe)
    {
        if(subscribe)
        {
            if (isSubscribed) yield break;
            while (!RoomManager.Instance.IsSpawned)
                yield return null;

            isSubscribed = true;
            RoomManager.Instance._rooms.OnChange += _rooms_OnChange;
            RecreateFullRoomListUI();
            yield break;
        }

        if(isSubscribed)
        {
            isSubscribed = false;
            RoomManager.Instance._rooms.OnChange -= _rooms_OnChange;
        }
    }

    private void OnDestroy()
    {
        StartCoroutine(SubscribeRoomManager(false));
    }

    private void _rooms_OnChange(FishNet.Object.Synchronizing.SyncListOperation op, int index, Data.RoomData oldItem, Data.RoomData newItem, bool asServer)
    {
        switch(op)
        {
            case FishNet.Object.Synchronizing.SyncListOperation.RemoveAt:
                RemoveRoomItemElement(oldItem.id);
                break;

            case FishNet.Object.Synchronizing.SyncListOperation.Add:
            case FishNet.Object.Synchronizing.SyncListOperation.Insert:
                if (!newItem.isReady) break;
                CreateRoomItemElement(newItem);
                break;

            case FishNet.Object.Synchronizing.SyncListOperation.Set:
                RoomItemListUI element = GetRoomItemElementById(newItem.id);
                if (element == null && newItem.isReady)
                {
                    CreateRoomItemElement(newItem);
                    break;
                }

                if(element != null && newItem.isStarted)
                {
                    element.SetAsStarted();
                }
                break;
        }
    }

    private void RecreateFullRoomListUI()
    {
        if (!isSubscribed) return;
        ClearItems();
        foreach(RoomData rd in RoomManager.Instance._rooms)
        {
            CreateRoomItemElement(rd);
        }
    }

    private RoomItemListUI GetRoomItemElementById(int id)
    {
        for (int i = 0; i < listViewContent.transform.childCount; i++)
        {
            GameObject gameObject = listViewContent.transform.GetChild(i).gameObject;
            if (gameObject.name == "Item-" + id)
                return gameObject.GetComponent<RoomItemListUI>();
        }

        return null;
    }

    private void CreateRoomItemElement(Data.RoomData roomData)
    {
        RoomItemListUI newItemUI = Instantiate(roomElementPrefab, listViewContent.transform);
        newItemUI.name = "Item-" + roomData.id;
        newItemUI.idRoom.text = roomData.id.ToString();
        newItemUI.titleRoom.text = roomData.title;
        newItemUI.button.onClick.AddListener(() => OnRoomItemUISelected(roomData.id));
        if (roomData.isStarted)
            newItemUI.SetAsStarted();
    }

    private void RemoveRoomItemElement(int id)
    {
        for (int i = 0; i < listViewContent.transform.childCount; i++)
        {
            GameObject gameObject = listViewContent.transform.GetChild(i).gameObject;
            if (gameObject.name == "Item-" + id)
            {
                RoomItemListUI roomItemListUI = gameObject.GetComponent<RoomItemListUI>();
                roomItemListUI.button.onClick.RemoveAllListeners();
                if(roomItemListUI.idRoom.text.Equals(currentRoomIdSelected.ToString()))
                {
                    currentRoomIdSelected = -1;
                }

                Destroy(gameObject);
            }
        }
    }

    private void OnRoomItemUISelected(int idRoom)
    {
        currentRoomIdSelected = idRoom;
    }

    private void ClearItems()
    {
        for (int i = 0; i < listViewContent.transform.childCount; i++)
        {
            Destroy(listViewContent.transform.GetChild(i).gameObject);
        }

        currentRoomIdSelected = -1;
    }
}
