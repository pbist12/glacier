using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BossController : MonoBehaviour
{
    #region ����
    public Transform firePoint;

    [Header ("Bullet Prefab")]
    public GameObject spreadBulletPrefab; // ��ä�� ź���� ���� �Ѿ� ������
    public GameObject homingBulletPrefab;

    [Header("Spread")]
    public int spreadBulletCount; // ��ä�� ź���� ���� ����
    public float spreadAngleStep; // ��ä�� ź���� ���� ����
    public float spreadFireInterval; // ź�� �߻� �ð� ����
    [SerializeField] private float spreadFireTimer = 0f; // �߻� �ð� ���� üũ�� ���� Ÿ�̸�
    public float spreadRotationStep; // �߻縶�� ȸ���� ����
    private float spreadOffsetAngle = 0f; // ���� ȸ�� ������
    public int spreadBurstCount; // 1ȸ �߻� �� �� �߻�Ǵ� �Ѿ� ���� 
    public float spreadBurstCoolDownTime; // 1ȸ �߻� �� ���� �����Ǵ� ��ٿ� �ð�
    private bool spreadBurstCoolDown = false; // ��ٿ� üũ�� ���� 
    private int spreadCurrentBurst = 0; // ���� ����Ʈ Ƚ�� ī��Ʈ�� ���� 
    private float spreadBurstTimer; // 

    [Header("Homing")]
    [SerializeField] private float homingCoolDown;

    [Header("Boss Stat")]
    public float moveSpeed = 2f;
    public float hp = 100f;
    [SerializeField] private int phase = 1;

    [Header("Bullet (Pool)")]
    public BulletPool pool;
    public float bulletSpeed = 8f;
    public float bulletLifetime = 5f;

    #endregion

    private void Start()
    {
        StartCoroutine(Homing());
    }

    void Update()
    {
        //MovePattern();
        AttackPattern();
        PhaseCheck();
    }

    void MovePattern()
    {
        // �ܼ� �¿� �̵�
        float x = Mathf.PingPong(Time.time * moveSpeed, 5f) - 2.5f;
        transform.position = new Vector3(x, transform.position.y, transform.position.z);
    }

    void AttackPattern()
    {
        if (phase == 1)
        {
            if (!spreadBurstCoolDown)
            {
                // ����Ʈ ����: ���� �������� ���� �� �߻�
                spreadBurstTimer += Time.deltaTime;

                if (spreadBurstTimer >= spreadFireInterval)
                {
                    spreadBurstTimer = 0f;

                    // ��ä�� �� �� �߻� (pool ���)
                    SpreadShot(spreadBulletCount, spreadAngleStep, spreadOffsetAngle);

                    // �� �߻縶�� ȸ�� ������ ����
                    spreadOffsetAngle += spreadRotationStep;

                    // �̹� ����Ʈ���� �� �� �߻��ߴ��� ī��Ʈ
                    spreadCurrentBurst++;

                    // ����Ʈ �� �� ��ٿ� ����
                    if (spreadCurrentBurst >= spreadBurstCount)
                    {
                        spreadBurstCoolDown = true;
                        spreadCurrentBurst = 0;
                        spreadBurstTimer = 0f;
                        spreadFireTimer = 0f; // ��ٿ� Ÿ�̸� �ʱ�ȭ
                    }
                }
            }
            else
            {
                // ����Ʈ ���� ��ٿ�
                spreadFireTimer += Time.deltaTime;
                if (spreadFireTimer >= spreadBurstCoolDownTime)
                {
                    spreadBurstCoolDown = false;
                    spreadFireTimer = 0f;
                    spreadBurstTimer = 0f;
                    // �ʿ��ϸ� ȸ�� ���� �ʱ�ȭ:
                    // spreadOffsetAngle = 0f;
                }
            }

            //HomingShot();
        }
        else if (phase == 2)
        {

        }
        else if (phase == 3)
        {

        }
    }

    void PhaseCheck()
    {
        if (hp < 70 && phase == 1) phase = 2;
        if (hp < 30 && phase == 2) phase = 3;
    }

    public void TakeDamage(float damage)
    {
        hp -= damage;
        if (hp <= 0) Die();
    }

    void Die()
    {
        Debug.Log("Boss Defeated!");
        Destroy(gameObject);
    }

/*    void SpreadShot(int bulletCount, float angleStep)
    {
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = spreadOffsetAngle + (-angleStep * (bulletCount - 1) / 2 + angleStep * i);
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Instantiate(spreadBulletPrefab, firePoint.position, rotation);
        }
    }*/

    public IEnumerator SetCoolDown()
    {
        while (spreadFireTimer < spreadBurstCoolDownTime)
        {
            spreadFireTimer += Time.deltaTime;
            yield return null;
        }
    }

    void SpreadShot(int count, float angleStep, float offsetDeg)
    {
        if (pool == null) return;

        // �߻� ����
        Vector2 origin;

        if (firePoint != null)
        {
            // firePoint�� �Ҵ�Ǿ� ������, �� ��ġ�� �߻� �������� ���
            origin = (Vector2)firePoint.position;
        }
        else
        {
            // firePoint�� ��� ������, �ڱ� �ڽ�(Transform)�� ��ġ�� �߻� �������� ���
            origin = (Vector2)transform.position;
        }

        // ���� ����: ���� ��(+Y) = 90��. 
        // ������Ʈ ȸ�� �������� �ϰ� ������ baseDeg = transform.eulerAngles.z; �� �ٲ㵵 ��.
        float baseDeg = 90f;

        // ��� ���ķ� �¿� ����
        float half = (count - 1) * 0.5f;
        for (int i = 0; i < count; i++)
        {
            float idx = i - half;
            float deg = baseDeg + offsetDeg + idx * angleStep;
            float rad = deg * Mathf.Deg2Rad;

            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
            pool.Spawn(origin, dir * bulletSpeed, bulletLifetime, zRotationDeg: deg);
        }
    }

    public void HomingShot()
    {
        Instantiate(homingBulletPrefab, firePoint.position, firePoint.rotation);
    }

    public IEnumerator Homing()
    {
        while (phase == 1)
        {
            yield return new WaitForSeconds(homingCoolDown);
            Instantiate(homingBulletPrefab, firePoint.position, firePoint.rotation);
        }
    }
}
