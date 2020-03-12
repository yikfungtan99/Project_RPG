using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AwardManager : MonoBehaviour
{
    [HideInInspector]
    public RPGManager playerStats;

    public GameObject thePlayer;
    public static AwardManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        playerStats = thePlayer.GetComponentInChildren<RPGManager>();
    }

    

    public void GiveXP(float amount)
    {
        playerStats.experience.ModifyCur(amount);
    }

}
