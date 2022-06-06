using Data;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Match;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : NetworkBehaviour
{
    public static RoomManager Instance { get; private set; }
    private int maxRoomId = 0;
    [SyncObject(SendRate = 2)]
    public readonly SyncList<RoomData> _rooms = new SyncList<RoomData>();
    private readonly Dictionary<int, Scene> _roomToScene = new Dictionary<int, Scene>();

    private List<GameObject> mainMenuDisabledGameObjects = new List<GameObject>();

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);

        Instance = this;
    }

    [ServerRpc(RequireOwnership = false)]
    public void MakeNewRoomRPC(CreationRoomData creationRoomData, NetworkConnection conn = null)
    {
        if (conn == null) return;
        RoomData roomData = new RoomData(creationRoomData);
        roomData.id = maxRoomId++;
        roomData.owner = conn;
        roomData.AddPlayer(conn);

        SceneLoadData sceneLoadData = new SceneLoadData("SampleScene");
        sceneLoadData.ReplaceScenes = ReplaceOption.None;
        sceneLoadData.Options.DisallowStacking = false;
        sceneLoadData.Options.AutomaticallyUnload = false;
        sceneLoadData.Options.LocalPhysics = LocalPhysicsMode.Physics2D;
        sceneLoadData.Params.ServerParams = new object[] { roomData };
        SceneManager.LoadConnectionScenes(conn, sceneLoadData);
    }

    [ServerRpc(RequireOwnership = false)]
    public void JoinRoomRPC(int roomId, NetworkConnection conn = null)
    {
        if (conn == null || roomId < 0) return;
        RoomData room = _rooms.Find(x => x.id == roomId);
        if (room == null || room.isFinished || room.isStarted) return;
        room.AddPlayer(conn);

        Scene scene = _roomToScene[roomId];
        SceneLoadData sceneLoadData = new SceneLoadData(scene.handle);
        sceneLoadData.Params.ServerParams = new object[] { room };
        SceneManager.LoadConnectionScenes(conn, sceneLoadData);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ServerUnloadPlayerFromRoom(int roomId, NetworkConnection conn = null)
    {
        if (roomId < 0) return;
        Scene matchScene = _roomToScene[roomId];
        if (matchScene == null) return;
        int roomIndex = _rooms.FindIndex(x => x.id == roomId);
        if (!_rooms[roomIndex].ContainsPlayer(conn)) return;

        SceneUnloadData sceneUnloadData = new SceneUnloadData(matchScene);
        sceneUnloadData.Params.ServerParams = new object[] { roomId };
        SceneManager.UnloadConnectionScenes(conn, sceneUnloadData);
    }

    public void OnGameFinished(int roomId)
    {
        if (roomId < 0) return;
        Scene matchScene = _roomToScene[roomId];
        if (matchScene == null) return;

        int roomIndex = _rooms.FindIndex(x => x.id == roomId);
        _rooms[roomIndex].isFinished = true;
        SceneUnloadData sceneUnloadData = new SceneUnloadData(matchScene);
        sceneUnloadData.Params.ServerParams = new object[] { _rooms[roomIndex].id };
        SceneManager.UnloadConnectionScenes(_rooms[roomIndex].GetAllPlayersInside(), sceneUnloadData);
    }

    public void RemovePlayerFromRoom(NetworkConnection conn)
    {
        if (conn == null) return;
        RoomData room = _rooms.Find(x => x.ContainsPlayer(conn));
        if (room == null) return;
        room.RemovePlayer(conn);
        room.matchEvents.OnPlayerLeave(conn);
    }

    public void SetRoomAsReady(int id)
    {
        for(int i = 0; i < _rooms.Count; i++)
        {
            if(_rooms[i].id == id)
            {
                _rooms[i].isReady = true;
                _rooms[i] = _rooms[i];
            }
        }
    }

    public void SetRoomAsStarted(int id)
    {
        for (int i = 0; i < _rooms.Count; i++)
        {
            if (_rooms[i].id == id)
            {
                _rooms[i].isStarted = true;
                _rooms[i] = _rooms[i];
            }
        }
    }

    #region Server Events
    public override void OnStartServer()
    {
        base.OnStartServer();

        SceneManager.OnLoadEnd += ServerSceneManager_OnLoadEnd;
        SceneManager.OnUnloadStart += ServerSceneManager_OnUnloadStart;
        SceneManager.OnClientPresenceChangeStart += ServerSceneManager_OnClientPresenceChangeStart;
        SceneManager.OnClientPresenceChangeEnd += ServerSceneManager_OnClientPresenceChangeEnd;

        ServerManager.OnRemoteConnectionState += ServerManager_OnRemoteConnectionState;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        SceneManager.OnLoadEnd -= ServerSceneManager_OnLoadEnd;
        SceneManager.OnUnloadStart -= ServerSceneManager_OnUnloadStart;
        SceneManager.OnClientPresenceChangeStart -= ServerSceneManager_OnClientPresenceChangeStart;
        SceneManager.OnClientPresenceChangeEnd -= ServerSceneManager_OnClientPresenceChangeEnd;

        ServerManager.OnRemoteConnectionState -= ServerManager_OnRemoteConnectionState;
    }

    private void ServerSceneManager_OnClientPresenceChangeStart(ClientPresenceChangeEventArgs obj)
    {
        if (obj.Scene.name != "SampleScene" || obj.Added) return;
        RemovePlayerFromRoom(obj.Connection);
    }

    private void ServerSceneManager_OnClientPresenceChangeEnd(ClientPresenceChangeEventArgs obj)
    {
        if (obj.Scene.name != "SampleScene" || !obj.Added) return;
        RoomData room = _rooms.Find(x => x.ContainsPlayer(obj.Connection));
        if (room == null) return;
        room.matchEvents.OnPlayerEnter(obj.Connection);
    }

    private void ServerManager_OnRemoteConnectionState(NetworkConnection conn, FishNet.Transporting.RemoteConnectionStateArgs rc)
    {
        if (rc.ConnectionState == FishNet.Transporting.RemoteConnectionStates.Stopped)
        {
            RemovePlayerFromRoom(conn);
        }
    }

    private void ServerSceneManager_OnLoadEnd(SceneLoadEndEventArgs obj)
    {
        foreach(Scene loadedScene in obj.LoadedScenes)
        {
            if (loadedScene.name == "SampleScene")
            {
                RoomData roomData = (RoomData)obj.QueueData.SceneLoadData.Params.ServerParams[0];
                _rooms.Add(roomData);
                _roomToScene.Add(roomData.id, obj.LoadedScenes[0]);
                foreach (GameObject go in obj.LoadedScenes[0].GetRootGameObjects())
                {
                    MatchLogic matchLogic = go.GetComponent<MatchLogic>();
                    if (matchLogic)
                    {
                        roomData.matchEvents = matchLogic;
                        matchLogic.OnMatchCreated(roomData);
                    }
                }
            }
        }
    }

    private void ServerSceneManager_OnUnloadStart(SceneUnloadStartEventArgs obj)
    {
        foreach (SceneLookupData sceneLookup in obj.QueueData.SceneUnloadData.SceneLookupDatas)
        {
            if (sceneLookup.Name == "SampleScene")
            {
                object[] matchParams = obj.QueueData.SceneUnloadData.Params.ServerParams;
                if (matchParams.Length == 0) return;

                int matchId = (int)matchParams[0];
                int roomIndex = _rooms.FindIndex(x => x.id == matchId);
                _rooms[roomIndex].playersInside.Clear();
                _rooms[roomIndex].matchEvents.OnMatchDestroy();
                _rooms.RemoveAt(roomIndex);
                _roomToScene.Remove(matchId);
            }
        }
    }
    #endregion


    #region Client Events
    public override void OnStartClient()
    {
        base.OnStartClient();

        SceneManager.OnLoadStart += ClientSceneManager_OnLoadStart;
        SceneManager.OnLoadEnd += ClientSceneManager_OnLoadEnd;
        SceneManager.OnUnloadEnd += ClientSceneManager_OnUnloadEnd;
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        SceneManager.OnLoadStart -= ClientSceneManager_OnLoadStart;
        SceneManager.OnLoadEnd -= ClientSceneManager_OnLoadEnd;
        SceneManager.OnUnloadEnd -= ClientSceneManager_OnUnloadEnd;
    }

    private void ClientSceneManager_OnLoadStart(SceneLoadStartEventArgs obj)
    {
        foreach(SceneLookupData sceneLookup in obj.QueueData.SceneLoadData.SceneLookupDatas)
        {
            if (sceneLookup.Name == "SampleScene")
            {
                mainMenuDisabledGameObjects.Clear();
                Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName("MainMenu");
                foreach (GameObject go in scene.GetRootGameObjects())
                {
                    if (go.activeSelf)
                    {
                        mainMenuDisabledGameObjects.Add(go);
                        go.SetActive(false);
                    }
                }
            }
        }
    }

    private void ClientSceneManager_OnLoadEnd(SceneLoadEndEventArgs obj)
    {
        if (obj.LoadedScenes[0].name == "SampleScene")
        {
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(obj.LoadedScenes[0]);
        }
    }

    private void ClientSceneManager_OnUnloadEnd(SceneUnloadEndEventArgs obj)
    {
        foreach (SceneLookupData sceneLookup in obj.QueueData.SceneUnloadData.SceneLookupDatas)
        {
            if (sceneLookup.Name == "SampleScene")
            {
                Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName("MainMenu");
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
                foreach (GameObject go in mainMenuDisabledGameObjects)
                    go.SetActive(true);
            }
        }
    }
    #endregion
}
