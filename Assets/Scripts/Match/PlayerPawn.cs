using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Object.Synchronizing;
using UnityEngine;
using Utils;

public class PlayerPawn : NetworkBehaviour
{
    #region Types.
    public struct MoveData
    {
        public float Vertical;

        public MoveData(float vertical)
        {
            Vertical = vertical;
        }
    }

    public struct ReconcileData
    {
        public Vector3 Position;
        public float VelocityY;
        public ReconcileData(Vector3 position, float velocityY)
        {
            Position = position;
            VelocityY = velocityY;
        }
    }
    #endregion

    [SerializeField]
    [SyncVar]
    private float _speed = 17f;
    public float Speed => _speed;

    private Rigidbody2D _rigidbody;

    [SyncVar]
    public int playerSide;
    public float startingXPosition;

    private MoveData _moveData = default;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    public void ServerSetSpeedMult(float mult)
    {
        _speed = _speed * mult;
    }

    public void SetVerticalVelocity(float velocity)
    {
        _moveData.Vertical = velocity;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        TimeManager.OnTick += TimeManager_ServerOnTick;
        TimeManager.OnPostTick += TimeManager_ServerOnPostTick;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        TimeManager.OnTick -= TimeManager_ServerOnTick;
        TimeManager.OnPostTick -= TimeManager_ServerOnPostTick;
    }

    public override void OnOwnershipClient(NetworkConnection prevOwner)
    {
        base.OnOwnershipClient(prevOwner);

        if(IsOwner)
        {
            TimeManager.OnTick += TimeManager_ClientOnTick;
        }
        else if(prevOwner == LocalConnection)
        {
            TimeManager.OnTick -= TimeManager_ClientOnTick;
        }
    }

    private void TimeManager_ClientOnTick()
    {
        Reconciliation(default, false);
        Move(_moveData, false);
    }

    private void TimeManager_ServerOnTick()
    {
        Move(default, true);
    }


    private void TimeManager_ServerOnPostTick()
    {
        ReconcileData rd = new ReconcileData(transform.position, _rigidbody.velocity.y);
        Reconciliation(rd, true);
    }

    [Replicate]
    private void Move(MoveData md, bool asServer, bool replaying = false)
    {
        _rigidbody.velocity = new Vector2(0, md.Vertical);
    }

    [Reconcile]
    private void Reconciliation(ReconcileData rd, bool asServer)
    {
        transform.position = rd.Position;
        _rigidbody.velocity = new Vector2(0, rd.VelocityY);
    }
}