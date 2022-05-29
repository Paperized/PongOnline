using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Object.Synchronizing;
using System.Collections;
using UnityEngine;

public class BallMovement : NetworkBehaviour
{
    #region Types.
    public struct ReconcileData
    {
        public Vector3 Position;
        public Vector2 Velocity;
        public ReconcileData(Vector3 position, Vector2 velocity)
        {
            Position = position;
            Velocity = velocity;
        }
    }
    #endregion

    private Rigidbody2D rigidbody2D;
    public float maxVelocity;
    public float initialVelocity;
    public float maxAngle = 75;
    public float velocityIncreasePerHit = 0.8f;

    [SyncVar(OnChange = nameof(OnCurrentSpeedChanged))]
    private float currentSpeed;

    // Start is called before the first frame update
    void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        TimeManager.OnTick += TimeManager_OnTick;
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();

        TimeManager.OnTick -= TimeManager_OnTick;
    }

    private void TimeManager_OnTick()
    {
        // keep facing the velocity direction
        transform.up = rigidbody2D.velocity;
    }

    public void StartMoving()
    {
        currentSpeed = initialVelocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.tag.Equals("Player")) return;
        // on collision on player
        SpriteRenderer spriteRenderer = collision.gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>();
        // sprite height
        float heightPlayer = spriteRenderer.bounds.size.y;
        // get contact point to player height, this value is in local space
        float contactPoint = collision.GetContact(0).point.y - spriteRenderer.bounds.min.y;
        // get an offset between -1 and 1 depending on the contact point height
        float normalizedOffset = NormalizeFromValue(heightPlayer, contactPoint, 2);
        // get the player offset depending on the side he's playing, to make the ball face the right direction
        float playerSide = collision.gameObject.GetComponent<PlayerMovement>().playerSide;
        // the new angle is the max possible angle multiplied by the normalized offset (center = 0, limits = maxAngle) depending
        // also on the player side
        float outputAngle = playerSide * 90f + maxAngle * normalizedOffset * -playerSide;
        // rotate the new angle
        transform.rotation = Quaternion.Euler(0, 0, outputAngle);

        currentSpeed += velocityIncreasePerHit;
    }

    private void OnCurrentSpeedChanged(float oldValue, float newValue, bool asServer)
    {
        rigidbody2D.velocity = transform.up * newValue;
    }

    private float NormalizeFromValue(float max, float value, int ratio)
    {
        float res = value / max;
        // normalize between 0 and 1
        res = Mathf.Clamp01(res);

        // normalize between (-ratio/2, ratio/2) -> in my case from -1 to 1
        return (res * ratio) - ratio / 2;
    }

    public void StopSmoothly()
    {
        StartCoroutine(StopSmooth());
    }

    private IEnumerator StopSmooth()
    {
        while(rigidbody2D.velocity.magnitude > 0)
        {
            Vector2 speedReduction = transform.up * 3f;
            Vector2 nextVelocity = rigidbody2D.velocity - speedReduction;
            if (nextVelocity.x < 0) nextVelocity.x = 0;
            if (nextVelocity.y < 0) nextVelocity.y = 0;
            yield return null;
        }
    }
}
