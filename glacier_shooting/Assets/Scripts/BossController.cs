using UnityEngine;

public class BossController : MonoBehaviour
{
    public Transform firePoint;

    [Header("Spread")]
    public GameObject spreadBulletPrefab; // ��ä�� ź���� ���� �Ѿ� ������
    public int spreadBulletCount; // ��ä�� ź���� ���� ����
    public float spreadAngleStep; // ��ä�� ź���� ���� ����
    public float spreadFireInterval; // ź�� �߻� �ð� ����
    private float spreadFireTimer = 0f; // �߻� �ð� ���� üũ�� ���� Ÿ�̸�
    public float spreadRotationStep; // �߻縶�� ȸ���� ����
    private float spreadOffsetAngle = 0f; // ���� ȸ�� ������
    public int spreadBurstCount; // 1ȸ �߻� �� �� �߻�Ǵ� �Ѿ� ���� 
    public float spreadBurstCoolDownTime; // 1ȸ �߻� �� ���� �����Ǵ� ��ٿ� �ð�
    private bool spreadBurstCoolDown = false; // ��ٿ� üũ�� ���� 
    private int spreadCurrentBurst = 0; // ���� ����Ʈ Ƚ�� ī��Ʈ�� ���� 
    private float spreadBurstTimer; // 

    [Header("Homing")]
    public GameObject homingBulletPrefab;


    public float moveSpeed = 2f;
    public float hp = 100f;

    private int phase = 1;

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
                spreadBurstTimer += Time.deltaTime;
                if (spreadBurstTimer >= spreadFireInterval)
                {
                    SpreadShot(spreadBulletCount, spreadAngleStep);
                    spreadOffsetAngle += spreadRotationStep; // �߻� ���� �������� ���� 
                    spreadFireTimer = 0f;
                    spreadCurrentBurst++;

                    if(spreadCurrentBurst >= spreadBurstCount)
                    {
                        spreadBurstCoolDown = true;
                        spreadCurrentBurst = 0;
                        spreadBurstTimer = 0f;
                    }
                }
            }
            else if (spreadFireTimer >= spreadBurstCoolDownTime)
            {
                spreadBurstCoolDown = false;
                spreadBurstTimer = 0f;
            }

            HomingShot();
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

    void SpreadShot(int bulletCount, float angleStep)
    {
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = spreadOffsetAngle + (-angleStep * (bulletCount - 1) / 2 + angleStep * i);
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Instantiate(spreadBulletPrefab, firePoint.position, rotation);
        }
    }

    public void HomingShot()
    {
        Instantiate(homingBulletPrefab, firePoint.position, firePoint.rotation);
    }
}
