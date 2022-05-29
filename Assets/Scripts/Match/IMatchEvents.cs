using Data;
using FishNet.Connection;

namespace Match
{
    public interface IMatchEvents
    {
        // Called during OnLoadEnd of SceneManager (Called only once)
        void OnMatchCreated(RoomData roomData);
        // Called inside the MatchLogic after all requirements are meet (Called only once)
        void OnMatchStarted();
        // Called inside the MatchLogic after all requirements are meet (Called only once)
        void OnMatchDestroy();

        // Called during OnClientPresenceChangeEnd of SceneManager
        void OnPlayerEnter(NetworkConnection conn);
        // Called during a Disconnection or Player leaving on their own
        void OnPlayerLeave(NetworkConnection conn);

        // Called from some trigger inside the Match Scene
        void OnScorePoint(Utils.MatchSide side);
    }
}
