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

                    StartCoroutine(SetCoolDown());
                }
            }
            else if (spreadFireTimer >= spreadBurstCoolDownTime)
            {
                spreadBurstCoolDown = false;
                spreadBurstTimer = 0f;
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

    void SpreadShot(int bulletCount, float angleStep)
    {
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = spreadOffsetAngle + (-angleStep * (bulletCount - 1) / 2 + angleStep * i);
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            Instantiate(spreadBulletPrefab, firePoint.position, rotation);
        }
    }

    public IEnumerator SetCoolDown()
    {
        while (spreadFireTimer < spreadBurstCoolDownTime)
        {
            spreadFireTimer += Time.deltaTime;
            yield return null;
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
