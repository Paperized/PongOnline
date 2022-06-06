using FishNet.Object;
using FishNet.Object.Synchronizing;
using Match.Utils;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : NetworkBehaviour
{
    public TMPro.TextMeshProUGUI scoreLeftText;
    public TMPro.TextMeshProUGUI scoreRightText;
    public TMPro.TextMeshProUGUI winLoseText;

    [SyncVar(Channel = FishNet.Transporting.Channel.Reliable, OnChange = nameof(OnScoreLeftChanged))]
    private int scoreLeft;
    public int ScoreLeft => scoreLeft;
    [SyncVar(Channel = FishNet.Transporting.Channel.Reliable, OnChange = nameof(OnScoreRightChanged))]
    private int scoreRight;
    public int ScoreRight => scoreRight;

    public int maxScore = 3;

    // Start is called before the first frame update
    void Start()
    {
        if(GlobalInitializer.StartedAsClient && (!scoreLeftText || !scoreRightText))
        {
            Debug.LogError("Score labels cannot be null");
        }
    }

    private void OnScoreLeftChanged(int oldValue, int newValue, bool asServer)
    {
        scoreLeftText.text = newValue.ToString();
        if(!asServer && newValue >= maxScore)
        {
            UpdateWinLoseText();
        }
    }

    private void OnScoreRightChanged(int oldValue, int newValue, bool asServer)
    {
        scoreRightText.text = newValue.ToString();
        if (!asServer && newValue >= maxScore)
        {
            UpdateWinLoseText();
        }
    }

    private void UpdateWinLoseText()
    {
        PlayerPawn[] players = FindObjectsOfType<PlayerPawn>();
        if (players.Length != 2)
        {
            Debug.LogError("Player must be 2");
        }

        PlayerPawn owned = players[0].IsOwner ? players[0] : players[1];
        if (scoreLeft >= maxScore) 
        {
            if(owned.playerSide == -1)
            {
                winLoseText.text = "YOU WIN!";
            } else
            {
                winLoseText.text = "YOU LOSE!";
            }
        } else if(scoreRight >= maxScore)
        {
            if (owned.playerSide == 1)
            {
                winLoseText.text = "YOU WIN!";
            }
            else
            {
                winLoseText.text = "YOU LOSE!";
            }
        }
    }

    public bool AddScore(MatchSide side)
    {
        if(side == MatchSide.Left)
        {
            scoreRight++;
            if(scoreRight >= maxScore)
            {
                return true;
            }
        } else
        {
            scoreLeft++;
            if (scoreLeft >= maxScore)
            {
                return true;
            }
        }

        return false;
    }
}
