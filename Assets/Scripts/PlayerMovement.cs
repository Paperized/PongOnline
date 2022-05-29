using FishNet;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Object.Synchronizing;
using UnityEngine;


public class PlayerMovement : NetworkBehaviour
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
    private float _speed = 10f;
    private Rigidbody2D _rigidbody;

    [SyncVar]
    public int playerSide;
    public float startingXPosition;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        TimeManager.OnTick += TimeManager_OnTick;
        TimeManager.OnPostTick += TimeManager_OnPostTick;
    }

    private void OnDestroy()
    {
        if (TimeManager != null)
        {
            TimeManager.OnTick -= TimeManager_OnTick;
            TimeManager.OnPostTick -= TimeManager_OnPostTick;
        }
    }

    private void TimeManager_OnTick()
    {
        if (base.IsOwner)
        {
            Reconciliation(default, false);
            CheckInput(out MoveData md);
            Move(md, false);
        }
        if (base.IsServer)
        {
            Move(default, true);
        }
    }


    private void TimeManager_OnPostTick()
    {
        if (base.IsServer)
        {
            ReconcileData rd = new ReconcileData(transform.position, _rigidbody.velocity.y);
            Reconciliation(rd, true);
        }
    }

    private void CheckInput(out MoveData md)
    {
        md = default;

        float vertical = Input.GetAxisRaw("Vertical") * _speed;
        md.Vertical = vertical;
    }

    [Replicate]
    private void Move(MoveData md, bool asServer, bool replaying = false)
    {
        //Add extra gravity for faster falls.
        _rigidbody.velocity = new Vector2(0, md.Vertical);
    }

    [Reconcile]
    private void Reconciliation(ReconcileData rd, bool asServer)
    {
        transform.position = rd.Position;
        _rigidbody.velocity = new Vector2(0, rd.VelocityY);
    }
}