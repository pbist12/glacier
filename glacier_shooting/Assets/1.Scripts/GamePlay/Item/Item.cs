using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemData item;

    [Header("Move Settings")]
    public float moveSpeed = 3f;          // 초당 이동 속도
    public float followRange = 5f;        // 따라오기 시작하는 범위
    public float pickupRange = 0.5f;      // 줍기 판정 거리

    private Transform player;

    #region 아이템 이동
    private void Awake()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }
    private void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // 플레이어가 일정 범위 안에 있으면 따라옴
        if (dist <= followRange)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                player.position,
                moveSpeed * Time.deltaTime
            );
        }

        // 일정 거리 안쪽이면 줍기 처리
        if (dist <= pickupRange)
        {
            Pickup();
        }
    }
    #endregion

    #region 실제 픽업
    private void Pickup()
    {
        var status = GameObject.FindFirstObjectByType<PlayerStatus>();
        var inventory = GameObject.FindFirstObjectByType<PlayerInventory>();

        if (item != null)
        {
            if (item.useMode == UseMode.OnUse && item.consumeOnPickup)
            {
                var ctx = new ItemContext(player.gameObject, inventory, status, Debug.Log);
                ItemRuntime.Use(item, ctx);
                status?.SetStat();
            }
            else
            {
                inventory?.AddToInventory(item, 1);
            }
        }

        Destroy(gameObject);
    }
    #endregion
}
