using FishNet;
using UnityEngine;

public class ShowNetworkObject : MonoBehaviour
{
    public enum ShowTo { Client, Server, Both }
    public ShowTo showTo;

    void Start()
    {
        if(showTo == ShowTo.Client && !GlobalInitializer.StartedAsClient)
            Destroy(gameObject);
        else if(showTo == ShowTo.Server && !GlobalInitializer.StartedAsServer)
            Destroy(gameObject);
    }
}
