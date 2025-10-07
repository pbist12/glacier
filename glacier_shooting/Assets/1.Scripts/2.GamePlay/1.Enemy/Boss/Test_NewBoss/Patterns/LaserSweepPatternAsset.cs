using System.Collections;
using UnityEngine;
using BossSystem;

[CreateAssetMenu(menuName="Boss/Pattern/Laser Sweep")]
public class LaserSweepPatternAsset : PatternAsset
{
    [Header("Flags")]
    public bool makeUninterruptible = true;

    [Header("Telegraph")]
    public float windup = 1.2f;     // ����/���� �ð�
    public float sweepDuration = 2f;
    public float sweepAngle = 90f;  // �¡��� ���� �Ѱ�

    public AnimationCurve sweepCurve = AnimationCurve.Linear(0,0,1,1);

    public GameObject laserPrefab;   // �浹 ���� ����
    public float laserLength = 20f;

    public override bool Uninterruptible => makeUninterruptible || base.Uninterruptible;

    public override IEnumerator Play(BossContext ctx)
    {
        // ���� ����: �÷��̾ ����
        var dir = ctx.DirToPlayer();
        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

        // ������ ����(���־�/�ݶ��̴�)
        var laser = GameObject.Instantiate(laserPrefab);
        laser.transform.position = ctx.Boss.position;
        laser.transform.rotation = Quaternion.Euler(0,0,baseAngle - sweepAngle * 0.5f);
        laser.SetActive(false);

        // �ڷ��׷���(���� ���� ��/���� ���� ���⼭)
        yield return new WaitForSeconds(windup);

        // �߻� + ����
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
