using Data;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using UnityEngine;
using Utils;

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
        private bool isEnding;
        private RoomData roomData;

        private int idRoomClient;

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

            TimeManager.OnTick += UpdatePhysics;
        }

        public override void OnStopNetwork()
        {
            base.OnStopNetwork();

            TimeManager.OnTick -= UpdatePhysics;
        }

        private void UpdatePhysics()
        {
            localPhysics2D.Simulate((float)TimeManager.TickDelta);
        }

        [ObserversRpc(BufferLast = true)]
        public void NotifyRoomIdToClients(int roomId)
        {
            idRoomClient = roomId;
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
            NotifyRoomIdToClients(roomData.id);
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
                rightPlayer.ServerSetSpeedMult(roomData.playerSpeedMultiplier);
                rightPlayer.playerSide = 1;
                rightPlayer.startingXPosition = tsf.position.x;
            }
            else
            {
                leftPlayer = nob.GetComponent<PlayerMovement>();
                leftPlayer.ServerSetSpeedMult(roomData.playerSpeedMultiplier);
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
                    ballObject.ServerSetInitialSpeedMult(roomData.ballSpeedMultiplier);
                }

                yield return new WaitForSeconds(1);

                int rnd = Random.Range(0, 2);
                if (rnd == 0)
                {
                    rnd = -1;
                }

                ballObject.transform.SetPositionAndRotation(ballSpawn.position, Quaternion.Euler(0, 0, rnd * 90));
                ballObject.StartMoving();
                isStartingRound = false;
            }
        }

        public void OnPlayerLeave(NetworkConnection conn)
        {
            if(DespawnPlayerFromMatch(conn) && roomData.playersInside.Count <= 1)
                StartCoroutine(EndMatch());
        }

        public void OnPlayerLeave(NetworkConnection[] conn)
        {
            bool despawned = false;
            if(conn != null && conn.Length > 0)
            {
                for (int i = 0; i < conn.Length; i++)
                {
                    bool isDesp = DespawnPlayerFromMatch(conn[i]);
                    if (isDesp)
                        despawned = true;
                }

                if(despawned && roomData.playersInside.Count <= 1)
                    StartCoroutine(EndMatch());
            }
        }

        private bool DespawnPlayerFromMatch(NetworkConnection conn)
        {
            if (conn == null) return false;
            Debug.Log("A player left the match!");
            bool wasDeleted = true;
            if (leftPlayer != null && leftPlayer.CompareOwner(conn))
            {
                leftPlayer.Despawn();
                CDebug.Log("Left player despawned");
            }
            else if (rightPlayer != null && rightPlayer.CompareOwner(conn))
            {
                rightPlayer.Despawn();
                CDebug.Log("Right player despawned");
            }
            else wasDeleted = false;

            return wasDeleted;
        }

        private IEnumerator EndMatch()
        {
            if (isEnding) yield break;
            CDebug.Log("Ending game...");
            isEnding = true;
            // Cooldown for player inside the room before getting kicked
            if (roomData.HasAnyPlayer())
            {
                ballObject.StopSmoothly();
                yield return new WaitForSeconds(secondsAfterGameFinished);
            }

            RoomManager.Instance.OnGameFinished(roomData.id);
        }

        public void ClientLocalPlayerQuit()
        {
            if(IsClient)
            {
                RoomManager.Instance.ServerUnloadPlayerFromRoom(idRoomClient);
            }
        }
    }
}
