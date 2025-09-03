using UnityEngine;

public class HomingBullet : MonoBehaviour
{
    public float speed = 5f;             // 이동 속도
    public float rotateSpeed = 200f;     // 유도 회전 속도(도/초)

    public float homingDelay = 0.6f;     // 유도 시작 전 직진 시간
    public float homingDuration = 1.8f;  // 유도 유지 시간 (끝나면 일반탄으로 전환)

    private Transform target;
    private float timer;
    private State state = State.PreHoming;

    private enum State { PreHoming, Homing, Straight }

    void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player) target = player.transform;
    }

    void Update()
    {
        timer += Time.deltaTime;

        switch (state)
        {
            case State.PreHoming:
                // 유도 전: ↓ 방향 직진
                MoveForward();
                if (timer >= homingDelay && target != null)
                {
                    state = State.Homing;
                    // 유도 단계 시간만을 재기 위해 타이머 리셋(선택)
                    timer = 0f;
                }
                break;

            case State.Homing:
                // 목표 각도 계산 (↓ 스프라이트 → +90f 보정)
                if (target == null)
                {
                    state = State.Straight; // 타깃 없으면 바로 일반탄
                    break;
                }

                Vector2 dir = (target.position - transform.position).normalized;
                float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;

                float newAngle = Mathf.MoveTowardsAngle(
                    transform.eulerAngles.z,
                    targetAngle,
                    rotateSpeed * Time.deltaTime
                );
                transform.rotation = Quaternion.Euler(0, 0, newAngle);

                MoveForward();

                // 유도 시간 끝 → 일반탄 전환
                if (timer >= homingDuration)
                {
                    state = State.Straight;
                }
                break;

            case State.Straight:
                // 더 이상 유도하지 않고 현재 바라보는 방향으로 직진
                MoveForward();
                break;
        }
    }

    // ↓(down)을 앞 방향으로 가정: -transform.up으로 전진
    void MoveForward()
    {
        transform.position += -transform.up * speed * Time.deltaTime;
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Border")
        {
            Destroy(gameObject);
        }
    }
}
