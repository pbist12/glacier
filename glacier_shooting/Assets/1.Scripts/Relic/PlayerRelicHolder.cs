using System.Collections.Generic;
using UnityEngine;

public class PlayerRelicHolder : MonoBehaviour
{
    public RelicData[] allRelic;
    public List<RelicData> playerRelics;  // ���� �� ���� ����

    void Start()
    {
        foreach (var relic in playerRelics)
        {
            PlayerStatus.Instance.AddStat(relic);
        }
    }

    public void AddRelic(RelicData relic)
    {
        playerRelics.Add(relic);
        PlayerStatus.Instance.AddStat(relic);
    }
}
