using FishNet;
using FishNet.Managing;
using FishNet.Managing.Scened;
using FishNet.Managing.Timing;
using FishNet.Transporting.Tugboat;
using System.Collections;
using UnityEngine;
using Utils;

public class GlobalInitializer : MonoBehaviour
{
    public static bool StartedAsClient { get; private set; }
    public static bool StartedAsServer { get; private set; }

    public TMPro.TextMeshProUGUI statusText;
    public bool startAsServer = false;
    private NetworkManager networkManager;
    private bool isConnecting;

    // Start is called before the first frame update
    void Start()
    {
        networkManager = FindObjectOfType<NetworkManager>();

        StartedAsClient = !startAsServer;
        StartedAsServer = startAsServer;

        InitializeFiles filesInit = FindObjectOfType<InitializeFiles>();
        filesInit.SetupDefaultGameConfig();
#if !UNITY_EDITOR
        GameConfiguration config = filesInit.LoadGameConfiguration();
#else
        GameConfiguration config = GameConfiguration.InstanceGame;
#endif

        Tugboat networkLayer = FindObjectOfType<Tugboat>();
        networkLayer.SetPort((ushort)config.port);
        networkLayer.SetTimeout(config.timeoutAfterSeconds, StartedAsServer);

        TimeManager timeManager = FindObjectOfType<TimeManager>();
        timeManager.SetTickRate((ushort)config.tickRate);

        if (StartedAsServer)
        {
            networkLayer.SetServerBindAddress(config.bindAddress, FishNet.Transporting.IPAddressType.IPv4);
            networkLayer.SetMaximumClients(config.maximumClient);

            CDebug.LogError($"Port {config.port}, Timeout {config.timeoutAfterSeconds}, Bind {networkLayer.GetServerBindAddress(FishNet.Transporting.IPAddressType.IPv4)}, Max Clients {networkLayer.GetMaximumClients()}");
            CDebug.LogError(config.ToString());

            networkManager.ServerManager.OnServerConnectionState += ServerManager_OnServerConnectionState;
            networkManager.ServerManager.StartConnection();
        }
        else
        {
            networkLayer.SetClientAddress(config.bindAddress);

            CDebug.LogError($"Port {config.port}, Bind {networkLayer.GetServerBindAddress(FishNet.Transporting.IPAddressType.IPv4)}");
            CDebug.LogError(config.ToString());

            networkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
            networkManager.ClientManager.StartConnection();
        }
    }

    private void OnDestroy()
    {
        if (StartedAsServer)
            networkManager.ServerManager.OnServerConnectionState -= ServerManager_OnServerConnectionState;
        else
            networkManager.ClientManager.OnClientConnectionState -= ClientManager_OnClientConnectionState;
    }

    private void ServerManager_OnServerConnectionState(FishNet.Transporting.ServerConnectionStateArgs obj)
    {
        if (obj.ConnectionState == FishNet.Transporting.LocalConnectionStates.Started)
        {
            statusText.text = "Online";
            isConnecting = false;
            StartMainScene();
        }
        else if (obj.ConnectionState == FishNet.Transporting.LocalConnectionStates.Stopped)
        {
            statusText.text = "Offline";
            isConnecting = false;
            StartCoroutine(RetryReconnection(2));
        }
        else if (obj.ConnectionState == FishNet.Transporting.LocalConnectionStates.Starting)
        {
            statusText.text = "Trying to connect...";
            isConnecting = true;
        }
        else if (obj.ConnectionState == FishNet.Transporting.LocalConnectionStates.Stopping)
        {
            statusText.text = "Disconnecting";
            isConnecting = true;
        }
    }

    private void StartMainScene()
    {
        foreach (GameObject go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            if (!go.CompareTag("Singletons"))
                Destroy(go);

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu", UnityEngine.SceneManagement.LoadSceneMode.Additive);
    }

    private void ClientManager_OnClientConnectionState(FishNet.Transporting.ClientConnectionStateArgs obj)
    {
        if (obj.ConnectionState == FishNet.Transporting.LocalConnectionStates.Started)
        {
            statusText.text = "Online";
            isConnecting = false;

            StartMainScene();
        }
        else if (obj.ConnectionState == FishNet.Transporting.LocalConnectionStates.Stopped)
        {
            statusText.text = "Offline";
            isConnecting = false;
            StartCoroutine(RetryReconnection(2));
        }
        else if (obj.ConnectionState == FishNet.Transporting.LocalConnectionStates.Starting)
        {
            statusText.text = "Trying to connect...";
            isConnecting = true;
        }
        else if (obj.ConnectionState == FishNet.Transporting.LocalConnectionStates.Stopping)
        {
            statusText.text = "Disconnecting";
            isConnecting = true;
        }
    }

    private IEnumerator RetryReconnection(float seconds)
    {
        do
        {
            yield return new WaitForSeconds(seconds);
            if (StartedAsClient && InstanceFinder.ClientManager.Started) yield break;
            if (StartedAsServer && InstanceFinder.ServerManager.Started) yield break;
            if (isConnecting) continue;
            if(StartedAsClient)
                InstanceFinder.ClientManager.StartConnection();
            else
                InstanceFinder.ServerManager.StartConnection();

        } while ((StartedAsClient && !InstanceFinder.ClientManager.Started)
                    || (StartedAsServer && !InstanceFinder.ServerManager.Started));
    }
}
