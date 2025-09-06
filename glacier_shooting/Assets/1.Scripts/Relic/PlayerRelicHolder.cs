using System.Collections.Generic;
using UnityEngine;

public class PlayerRelicHolder : MonoBehaviour
{
    public RelicData[] allRelic;
    public List<RelicData> playerRelics;  // 시작 시 보유 유물

    void Start()
    {
        foreach (var relic in playerRelics)
        {
            PlayerStatus.Instance.AddStat(relic);
        }
    }

    public void AddRelic()
    {
        RelicData relic = null;
        relic = allRelic[Random.Range(0, allRelic.Length)];

        playerRelics.Add(relic);
        PlayerStatus.Instance.AddStat(relic);
    }
}
