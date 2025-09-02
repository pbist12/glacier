using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BossController : MonoBehaviour
{
    #region 변수
    public Transform firePoint;

    [Header ("Bullet Prefab")]
    public GameObject spreadBulletPrefab; // 부채꼴 탄막에 사용될 총알 프리팹
    public GameObject homingBulletPrefab;

    [Header("Spread")]
    public int spreadBulletCount; // 부채꼴 탄막의 라인 개수
    public float spreadAngleStep; // 부채꼴 탄막의 라인 간격
    public float spreadFireInterval; // 탄막 발사 시간 간격
    [SerializeField] private float spreadFireTimer = 0f; // 발사 시간 간격 체크에 사용될 타이머
    public float spreadRotationStep; // 발사마다 회전할 각도
    private float spreadOffsetAngle = 0f; // 현재 회전 오프셋
    public int spreadBurstCount; // 1회 발사 할 때 발사되는 총알 개수 
    public float spreadBurstCoolDownTime; // 1회 발사 후 공격 정지되는 쿨다운 시간
    private bool spreadBurstCoolDown = false; // 쿨다운 체크용 변수 
    private int spreadCurrentBurst = 0; // 현재 버스트 횟수 카운트용 변수 
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
        // 단순 좌우 이동
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
                    spreadOffsetAngle += spreadRotationStep; // 발사 각도 오프셋을 변경 
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
