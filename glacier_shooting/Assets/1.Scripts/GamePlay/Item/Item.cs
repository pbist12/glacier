using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemData item;

    [Header("Move Settings")]
    public float moveSpeed = 3f;          // �ʴ� �̵� �ӵ�
    public float followRange = 5f;        // ������� �����ϴ� ����
    public float pickupRange = 0.5f;      // �ݱ� ���� �Ÿ�

    private Transform player;

    #region ������ �̵�
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

        // �÷��̾ ���� ���� �ȿ� ������ �����
        if (dist <= followRange)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                player.position,
                moveSpeed * Time.deltaTime
            );
        }

        // ���� �Ÿ� �����̸� �ݱ� ó��
        if (dist <= pickupRange)
        {
            Pickup();
        }
    }
    #endregion

    #region ���� �Ⱦ�
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
