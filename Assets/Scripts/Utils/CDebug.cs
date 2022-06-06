using FishNet;
using FishNet.Managing;
using FishNet.Managing.Server;
using UnityEngine;

namespace Utils
{
    public static class CDebug
    {
        public enum CDebugType { All, IsHost, IsClient, IsServer, IsOffline };

        private static string GetStringWithHeader(string str)
        {
            NetworkManager networkManager = InstanceFinder.NetworkManager;
            if (networkManager.IsHost)
                return "[Host]: " + str;
            else if (networkManager.IsServer)
                return "[Server]: " + str;
            else if (networkManager.IsClient)
                return "[Client]: " + str;
            else
                return "[Offline]: " + str;
        }

        private static bool IsApplicationOfType(CDebugType cDebugType)
        {
            NetworkManager networkManager = InstanceFinder.NetworkManager;
            if (cDebugType == CDebugType.All) return true;

            if (cDebugType == CDebugType.IsHost && networkManager.IsHost) return true;
            else if (cDebugType == CDebugType.IsServer && networkManager.IsServer) return true;
            else if (cDebugType == CDebugType.IsClient && networkManager.IsClient) return true;
            else if (cDebugType == CDebugType.IsOffline && networkManager.IsOffline) return true;
            
            return false;
        }

        public static void LogError(string str, CDebugType debugType = CDebugType.All)
        {
            if(IsApplicationOfType(debugType))
                Debug.LogError(GetStringWithHeader(str));
        }

        public static void Log(string str, CDebugType debugType = CDebugType.All)
        {
            if (IsApplicationOfType(debugType))
                Debug.Log(GetStringWithHeader(str));
        }
    }
}
