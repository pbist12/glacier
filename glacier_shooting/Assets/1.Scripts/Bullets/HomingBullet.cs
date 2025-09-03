using UnityEngine;

public class HomingBullet : MonoBehaviour
{
    public float speed = 5f;             // �̵� �ӵ�
    public float rotateSpeed = 200f;     // ���� ȸ�� �ӵ�(��/��)

    public float homingDelay = 0.6f;     // ���� ���� �� ���� �ð�
    public float homingDuration = 1.8f;  // ���� ���� �ð� (������ �Ϲ�ź���� ��ȯ)

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
                // ���� ��: �� ���� ����
                MoveForward();
                if (timer >= homingDelay && target != null)
                {
                    state = State.Homing;
                    // ���� �ܰ� �ð����� ��� ���� Ÿ�̸� ����(����)
                    timer = 0f;
                }
                break;

            case State.Homing:
                // ��ǥ ���� ��� (�� ��������Ʈ �� +90f ����)
                if (target == null)
                {
                    state = State.Straight; // Ÿ�� ������ �ٷ� �Ϲ�ź
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

                // ���� �ð� �� �� �Ϲ�ź ��ȯ
                if (timer >= homingDuration)
                {
                    state = State.Straight;
                }
                break;

            case State.Straight:
                // �� �̻� �������� �ʰ� ���� �ٶ󺸴� �������� ����
                MoveForward();
                break;
        }
    }

    // ��(down)�� �� �������� ����: -transform.up���� ����
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
