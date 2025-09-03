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
                // 버스트 내부: 일정 간격으로 여러 번 발사
                spreadBurstTimer += Time.deltaTime;

                if (spreadBurstTimer >= spreadFireInterval)
                {
                    spreadBurstTimer = 0f;

                    // 부채꼴 한 번 발사 (pool 사용)
                    SpreadShot(spreadBulletCount, spreadAngleStep, spreadOffsetAngle);

                    // 매 발사마다 회전 오프셋 누적
                    spreadOffsetAngle += spreadRotationStep;

                    // 이번 버스트에서 몇 번 발사했는지 카운트
                    spreadCurrentBurst++;

                    // 버스트 끝 → 쿨다운 진입
                    if (spreadCurrentBurst >= spreadBurstCount)
                    {
                        spreadBurstCoolDown = true;
                        spreadCurrentBurst = 0;
                        spreadBurstTimer = 0f;
                        spreadFireTimer = 0f; // 쿨다운 타이머 초기화
                    }
                }
            }
            else
            {
                // 버스트 사이 쿨다운
                spreadFireTimer += Time.deltaTime;
                if (spreadFireTimer >= spreadBurstCoolDownTime)
                {
                    spreadBurstCoolDown = false;
                    spreadFireTimer = 0f;
                    spreadBurstTimer = 0f;
                    // 필요하면 회전 누적 초기화:
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

        // 발사 원점
        Vector2 origin;

        if (firePoint != null)
        {
            // firePoint가 할당되어 있으면, 그 위치를 발사 원점으로 사용
            origin = (Vector2)firePoint.position;
        }
        else
        {
            // firePoint가 비어 있으면, 자기 자신(Transform)의 위치를 발사 원점으로 사용
            origin = (Vector2)transform.position;
        }

        // 기준 각도: 월드 위(+Y) = 90도. 
        // 오브젝트 회전 기준으로 하고 싶으면 baseDeg = transform.eulerAngles.z; 로 바꿔도 됨.
        float baseDeg = 90f;

        // 가운데 정렬로 좌우 분포
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
