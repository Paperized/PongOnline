using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    private PlayerPawn playerPawn;
    public PlayerPawn PlayerPawn => playerPawn;

    [ObserversRpc(BufferLast = true, IncludeOwner = true, RunLocally = true)]
    public void ChangePlayerPawnControlled(PlayerPawn playerPawn)
    {
        this.playerPawn = playerPawn;
    }

    // Update is called once per frame
    void Update()
    {
        if(IsOwner)
            HandleInput();
    }

    private void HandleInput()
    {
        if (playerPawn == null) return;
        float vertical = Input.GetAxisRaw("Vertical") * playerPawn.Speed;
        playerPawn.SetVerticalVelocity(vertical);
    }
}
