using FishNet.Connection;
using Match;
using System;
using System.Collections.Generic;

namespace Data
{
    [Serializable]
    public struct CreationRoomData
    {
        public string title;
        public float ballSpeedMultiplier;
        public float playerSpeedMultiplier;

        public CreationRoomData(string title = "", float ballMult = 1, float playerMult = 1)
        {
            this.title = title;
            this.ballSpeedMultiplier = ballMult;
            this.playerSpeedMultiplier = playerMult;
        }

        public bool IsValid()
        {
            return title != null && title.Length > 0 && ballSpeedMultiplier >= 0.5f && ballSpeedMultiplier <= 3 
                && playerSpeedMultiplier >= 0.5f && playerSpeedMultiplier <= 3;
        }
    }


    [Serializable]
    public class RoomData
    {
        public int id;
        public string title;
        public float ballSpeedMultiplier;
        public float playerSpeedMultiplier;
        public bool isReady;
        public bool isStarted;
        [NonSerialized]
        public bool isFinished;

        [NonSerialized]
        public NetworkConnection owner;
        [NonSerialized]
        public readonly List<NetworkConnection> playersInside = new List<NetworkConnection>(2);
        [NonSerialized]
        public IMatchEvents matchEvents;

        public RoomData() { }
        public RoomData(CreationRoomData creationRoomData)
        {
            title = creationRoomData.title;
            ballSpeedMultiplier = creationRoomData.ballSpeedMultiplier;
            playerSpeedMultiplier = creationRoomData.playerSpeedMultiplier;
        }

        public void AddPlayer(NetworkConnection conn)
        {
            playersInside.Add(conn);
        }

        public NetworkConnection[] GetAllPlayersInside()
        {
            return playersInside.ToArray();
        }

        public bool ContainsPlayer(NetworkConnection conn)
        {
            if (conn == null) return false;

            return playersInside.Contains(conn);
        }

        public void RemovePlayer(NetworkConnection conn)
        {
            playersInside.Remove(conn);
        }

        public bool HasAnyPlayer() => playersInside.Count > 0;
    }
}
