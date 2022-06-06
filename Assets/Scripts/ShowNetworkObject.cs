using FishNet;
using UnityEngine;

public class ShowNetworkObject : MonoBehaviour
{
    public enum ShowTo { Client, Server, Both }
    [Tooltip("Destroy object if not of this type, always show in editor if server!")]
    public ShowTo showTo;

    void Start()
    {
        if(showTo == ShowTo.Client && !GlobalInitializer.StartedAsClient)
            Destroy(gameObject);
#if !UNITY_EDITOR
        else if(showTo == ShowTo.Server && !GlobalInitializer.StartedAsServer)
            Destroy(gameObject);
#endif

    }
}
