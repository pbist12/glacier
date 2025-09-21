using UnityEngine;

public class Item : MonoBehaviour
{
    public RelicDatabase database;
    public RelicData data;

    [Header("Fallback Move Settings (플레이어 마그넷 없을 때만 사용)")]
    public float moveSpeed = 3f;   // 기본 끌림 속도
    public float followRange = 5f; // 끌림 시작 범위
    public float pickupRange = 0.5f; // 줍기 판정 거리

    private Transform player;
    private PlayerItemMagnet magnet;     // 플레이어 마그넷(있으면 우선)
    private PlayerStatus statusCached;
    private PlayerInventory inventoryCached;

    void Awake()
    {
        var pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj)
        {
            player = pObj.transform;
            magnet = pObj.GetComponent<PlayerItemMagnet>();     // 있으면 참조
            statusCached = pObj.GetComponent<PlayerStatus>();   // 가능하면 캐시
            inventoryCached = pObj.GetComponent<PlayerInventory>();
        }

        // 싱글톤/전역 인스턴스를 쓰는 프로젝트라면 아래 캐시도 선택적으로:
        if (!statusCached) statusCached = GameObject.FindFirstObjectByType<PlayerStatus>();
        if (!inventoryCached) inventoryCached = GameObject.FindFirstObjectByType<PlayerInventory>();

        data = database.relics[Random.Range(0, database.relics.Count)];
    }

    void Update()
    {
        if (!player) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // --- 플레이어 마그넷 우선 ---
        if (magnet)
        {
            // 끌림/줍기 범위는 플레이어 값 우선, 없으면 Fallback
            float fr = magnet.followRange > 0 ? magnet.followRange : followRange;
            float pr = magnet.pickupRange > 0 ? magnet.pickupRange : pickupRange;

            if (dist <= fr)
            {
                // 거리 기반 속도 계산
                float pull = magnet.GetPullSpeed(dist);
                float speed = pull > 0 ? pull : moveSpeed;

                transform.position = Vector3.MoveTowards(
                    transform.position,
                    player.position,
                    speed * Time.deltaTime
                );
            }

            if (dist <= pr)
            {
                Pickup();
            }

            return;
        }

        // --- Fallback(플레이어 마그넷이 없을 때) ---
        if (dist <= followRange)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                player.position,
                moveSpeed * Time.deltaTime
            );
        }

        if (dist <= pickupRange)
        {
            Pickup();
        }
    }

    // 실제 픽업 처리
    private void Pickup()
    {
        var status = statusCached ? statusCached : GameObject.FindFirstObjectByType<PlayerStatus>();
        var inventory = inventoryCached ? inventoryCached : GameObject.FindFirstObjectByType<PlayerInventory>();

        if (data != null)
        {
            inventory?.AddRelicToInventory(data);
        }

        Destroy(gameObject);
    }
}
