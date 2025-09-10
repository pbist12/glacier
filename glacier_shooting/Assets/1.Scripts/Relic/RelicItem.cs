using UnityEngine;

public class RelicItem : MonoBehaviour
{
    public RelicData relic;

    public PlayerRelicHolder holder;

    [Header("Move Settings")]
    public float moveSpeed = 3f;       // �ʴ� �̵� �ӵ�
    public float followRange = 5f;     // �÷��̾�� �� �Ÿ� �̳��� ����

    private Transform player;

    private void Awake()
    {
        holder = GameObject.FindFirstObjectByType<PlayerRelicHolder>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
        }
    }

    private void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // �÷��̾ ���� ���� �ȿ� ������ ������ �ٰ���
        if (dist <= followRange)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                player.position,
                moveSpeed * Time.deltaTime
            );
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            holder.AddRelic(relic);
            Destroy(gameObject);
        }
    }
}
