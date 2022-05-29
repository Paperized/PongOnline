using FishNet;
using FishNet.Managing;
using FishNet.Managing.Scened;
using System.Collections;
using UnityEngine;

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

        if (StartedAsServer)
        {
            networkManager.ServerManager.OnServerConnectionState += ServerManager_OnServerConnectionState;
            networkManager.ServerManager.StartConnection();
        }
        else
        {
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
