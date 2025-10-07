using System.Collections;
using UnityEngine;
using BossSystem;

[CreateAssetMenu(menuName="Boss/Pattern/Laser Sweep")]
public class LaserSweepPatternAsset : PatternAsset
{
    [Header("Flags")]
    public bool makeUninterruptible = true;

    [Header("Telegraph")]
    public float windup = 1.2f;     // 충전/예고 시간
    public float sweepDuration = 2f;
    public float sweepAngle = 90f;  // 좌→우로 스윕 총각

    public AnimationCurve sweepCurve = AnimationCurve.Linear(0,0,1,1);

    public GameObject laserPrefab;   // 충돌 판정 포함
    public float laserLength = 20f;

    public override bool Uninterruptible => makeUninterruptible || base.Uninterruptible;

    public override IEnumerator Play(BossContext ctx)
    {
        // 시작 방향: 플레이어를 조준
        var dir = ctx.DirToPlayer();
        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

        // 레이저 생성(비주얼/콜라이더)
        var laser = GameObject.Instantiate(laserPrefab);
        laser.transform.position = ctx.Boss.position;
        laser.transform.rotation = Quaternion.Euler(0,0,baseAngle - sweepAngle * 0.5f);
        laser.SetActive(false);

        // 텔레그래프(보스 몸에 빛/사운드 등은 여기서)
        yield return new WaitForSeconds(windup);

        // 발사 + 스윕
        laser.SetActive(true);

        float t = 0f;
        while (t < sweepDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / sweepDuration);
            float offset = sweepCurve.Evaluate(k) * sweepAngle;
            laser.transform.rotation = Quaternion.Euler(0,0,baseAngle - sweepAngle*0.5f + offset);
            yield return null;
        }

        GameObject.Destroy(laser);
    }
}
