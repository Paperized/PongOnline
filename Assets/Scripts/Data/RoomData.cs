using FishNet.Connection;
using Match;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    [Serializable]
    public class RoomData
    {
        public int id;
        public string title;
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
