using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Object.Synchronizing;
using System.Collections;
using UnityEngine;
using Utils;

public class BallMovement : NetworkBehaviour
{
    private Rigidbody2D rigidbody2D;
    public float maxVelocity;
    [SyncVar]
    public float initialVelocity;
    public float maxAngle = 75;
    public float velocityIncreasePerHit = 0.8f;
    private float currentSpeed;

    [SerializeField]
    private float offsetCollisionX;

    // Start is called before the first frame update
    void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    public void ServerSetInitialSpeedMult(float mult)
    {
        this.initialVelocity = initialVelocity * mult;
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        TimeManager.OnPostTick += TimeManager_OnPostTick;
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();

        TimeManager.OnPostTick -= TimeManager_OnPostTick;
    }

    private void TimeManager_OnPostTick()
    {
        transform.up = rigidbody2D.velocity;
    }

    public void StartMoving()
    {
        ServerOnSpeedChanged(initialVelocity, transform.rotation.eulerAngles.z);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        // on collision on player
        SpriteRenderer spriteRenderer = collision.gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>();
        ContactPoint2D contactPoint = collision.GetContact(0);
        if (spriteRenderer.bounds.max.x - contactPoint.point.x >= offsetCollisionX)
            return;
        // sprite height
        float heightPlayer = spriteRenderer.bounds.size.y;
        // get contact point to player height, this value is in local space
        float contactPointY = contactPoint.point.y - spriteRenderer.bounds.min.y;
        // get an offset between -1 and 1 depending on the contact point height
        float normalizedOffset = NormalizeFromValue(heightPlayer, contactPointY, 2);
        // get the player offset depending on the side he's playing, to make the ball face the right direction
        float playerSide = collision.gameObject.GetComponent<PlayerPawn>().playerSide;
        // the new angle is the max possible angle multiplied by the normalized offset (center = 0, limits = maxAngle) depending
        // also on the player side
        float outputAngle = playerSide * 90f + maxAngle * normalizedOffset * -playerSide;

        ServerOnSpeedChanged(currentSpeed + velocityIncreasePerHit, outputAngle);
    }

    private void ServerOnSpeedChanged(float speed, float zRotation)
    {
        currentSpeed = speed;
        transform.rotation = Quaternion.Euler(0, 0, zRotation);
        rigidbody2D.velocity = transform.up * currentSpeed;
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
