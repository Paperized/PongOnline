using FishNet;
using Match;
using Match.Utils;
using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    public MatchSide triggerSide;
    private MatchLogic matchLogic;

    private void Start()
    {
        matchLogic = FindObjectOfType<MatchLogic>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(InstanceFinder.IsServer)
        {
            matchLogic.OnScorePoint(triggerSide);
        }
    }
}
