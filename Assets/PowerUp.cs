using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : NetworkBehaviour
{
    [SerializeField]
    private float speed = 5f;

    // Start is called before the first frame update
    void Start()
    {
        if(IsServer)
        {
            Vector2 initialDirection = Random.insideUnitCircle.normalized * speed;
            GetComponent<Rigidbody2D>().velocity = initialDirection;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;
        if (!collision.CompareTag("Ball")) return;

        
        Despawn();
    }
}
