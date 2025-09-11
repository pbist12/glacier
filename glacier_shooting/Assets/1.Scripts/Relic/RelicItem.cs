using UnityEngine;

public class RelicItem : MonoBehaviour
{
    public RelicData relic;

    public PlayerRelicHolder holder;

    [Header("Move Settings")]
    public float moveSpeed = 3f;       // 초당 이동 속도
    public float followRange = 5f;     // 플레이어와 이 거리 이내면 따라감

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

        // 플레이어가 일정 범위 안에 있으면 서서히 다가감
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
