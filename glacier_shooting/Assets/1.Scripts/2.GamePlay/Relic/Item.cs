using UnityEngine;

public class Item : MonoBehaviour
{
    public RelicDatabase database;
    public RelicData data;

    [Header("Fallback Move Settings (�÷��̾� ���׳� ���� ���� ���)")]
    public float moveSpeed = 3f;   // �⺻ ���� �ӵ�
    public float followRange = 5f; // ���� ���� ����
    public float pickupRange = 0.5f; // �ݱ� ���� �Ÿ�

    private Transform player;
    private PlayerItemMagnet magnet;     // �÷��̾� ���׳�(������ �켱)
    private PlayerStatus statusCached;
    private PlayerInventory inventoryCached;

    void Awake()
    {
        var pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj)
        {
            player = pObj.transform;
            magnet = pObj.GetComponent<PlayerItemMagnet>();     // ������ ����
            statusCached = pObj.GetComponent<PlayerStatus>();   // �����ϸ� ĳ��
            inventoryCached = pObj.GetComponent<PlayerInventory>();
        }

        // �̱���/���� �ν��Ͻ��� ���� ������Ʈ��� �Ʒ� ĳ�õ� ����������:
        if (!statusCached) statusCached = GameObject.FindFirstObjectByType<PlayerStatus>();
        if (!inventoryCached) inventoryCached = GameObject.FindFirstObjectByType<PlayerInventory>();

        data = database.relics[Random.Range(0, database.relics.Count)];
    }

    void Update()
    {
        if (!player) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // --- �÷��̾� ���׳� �켱 ---
        if (magnet)
        {
            // ����/�ݱ� ������ �÷��̾� �� �켱, ������ Fallback
            float fr = magnet.followRange > 0 ? magnet.followRange : followRange;
            float pr = magnet.pickupRange > 0 ? magnet.pickupRange : pickupRange;

            if (dist <= fr)
            {
                // �Ÿ� ��� �ӵ� ���
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

        // --- Fallback(�÷��̾� ���׳��� ���� ��) ---
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

    // ���� �Ⱦ� ó��
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
