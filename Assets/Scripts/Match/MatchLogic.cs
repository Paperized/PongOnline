using Data;
using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using UnityEngine;

namespace Match
{
    public class MatchLogic : NetworkBehaviour, IMatchEvents
    {
        [SerializeField]
        private Transform leftPlayerSpawn, rightPlayerSpawn, ballSpawn;
        [SerializeField]
        private float secondsAfterGameFinished = 2;

        private PhysicsScene2D localPhysics2D;
        private ScoreManager scoreManager;
        public NetworkObject playerPrefab;
        public NetworkObject ballPrefab;

        private PlayerMovement leftPlayer;
        private PlayerMovement rightPlayer;
        private BallMovement ballObject;

        private bool isStartingRound;
        private RoomData roomData;

        private void Start()
        {
            scoreManager = GetComponent<ScoreManager>();
            localPhysics2D = gameObject.scene.GetPhysicsScene2D();

            if (Physics2D.simulationMode != SimulationMode2D.Script)
                Physics2D.simulationMode = SimulationMode2D.Script;
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
            localPhysics2D.Simulate(Time.fixedDeltaTime);
        }

        public void OnScorePoint(Utils.MatchSide side)
        {
            if (!scoreManager.AddScore(side))
            {
                StartCoroutine(StartRound());
            }
            else
            {
                StartCoroutine(EndMatch());
            }
        }

        public void OnMatchDestroy()
        {
            Debug.Log("On match ended!");
        }

        public void OnMatchStarted()
        {
            Debug.Log("On match started!");
            StartCoroutine(StartRound());
        }

        public void OnMatchCreated(RoomData roomData)
        {
            Debug.Log("On match created!");
            this.roomData = roomData;
        }

        public void OnPlayerEnter(NetworkConnection conn)
        {
            Debug.Log("A player enter the match!");

            if (!conn.Scenes.Contains(gameObject.scene)) return;
            SpawnPlayerAndStartIfFull(conn);
            if ((leftPlayer && !rightPlayer) || (rightPlayer && !leftPlayer))
            {
                RoomManager.Instance.SetRoomAsReady(roomData.id);
            }
            if (leftPlayer && rightPlayer)
            {
                RoomManager.Instance.SetRoomAsStarted(roomData.id);
            }
        }

        private void SpawnPlayerAndStartIfFull(NetworkConnection conn)
        {
            if (conn == null) return;
            bool spawnLeft = leftPlayer == null;
            Transform tsf = spawnLeft ? leftPlayerSpawn : rightPlayerSpawn;

            NetworkObject nob = Instantiate(playerPrefab, tsf.position, tsf.rotation);
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(nob.gameObject, gameObject.scene);

            ServerManager.Spawn(nob, conn);

            if (!spawnLeft)
            {
                rightPlayer = nob.GetComponent<PlayerMovement>();
                rightPlayer.playerSide = 1;
                rightPlayer.startingXPosition = tsf.position.x;
            }
            else
            {
                leftPlayer = nob.GetComponent<PlayerMovement>();
                leftPlayer.playerSide = -1;
                leftPlayer.startingXPosition = tsf.position.x;
            }

            if (leftPlayer && rightPlayer)
            {
                OnMatchStarted();
            }
        }

        private IEnumerator StartRound()
        {
            if (!isStartingRound)
            {
                isStartingRound = true;
                if (!ballObject)
                {
                    NetworkObject nob = Instantiate(ballPrefab, ballSpawn.position, ballSpawn.rotation);
                    UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(nob.gameObject, gameObject.scene);
                    ServerManager.Spawn(nob);

                    ballObject = nob.GetComponent<BallMovement>();
                }

                yield return new WaitForSeconds(1);

                int rnd = Random.Range(0, 2);
                if (rnd == 0)
                {
                    rnd = -1;
                }

                ballObject.transform.position = ballSpawn.position;
                ballObject.transform.rotation = Quaternion.Euler(0, 0, rnd * 90);
                ballObject.StartMoving();
                isStartingRound = false;
            }
        }

        public void OnPlayerLeave(NetworkConnection conn)
        {
            if (conn == null) return;
            Debug.Log("A player left the match!");
            bool wasPlaying = true;
            if (leftPlayer != null && leftPlayer.CompareOwner(conn))
            {
                leftPlayer.Despawn();
            }
            else if (rightPlayer != null && rightPlayer.CompareOwner(conn))
            {
                rightPlayer.Despawn();
            }
            else wasPlaying = false;

            if(wasPlaying)
            {
                StartCoroutine(EndMatch());
            }
        }

        private IEnumerator EndMatch()
        {
            // Cooldown for player inside the room before getting kicked
            if (roomData.HasAnyPlayer())
            {
                ballObject.StopSmoothly();
                yield return new WaitForSeconds(secondsAfterGameFinished);
            }

            RoomManager.Instance.OnGameFinished(roomData.id);
        }
    }
}
