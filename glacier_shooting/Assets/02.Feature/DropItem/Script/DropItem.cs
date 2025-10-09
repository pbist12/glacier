using UnityEngine;

public class DropItem : MonoBehaviour
{

    public enum ItemType 
    {
        None,
        UseOnGet,
        Relic
    }
    public enum UseType
    {
        None,
        Health,
        Score,
        Money
    }

    public ItemType type;
    public UseType useType;

    public bool isBigger;

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

        if (type == ItemType.UseOnGet)
        {
            switch (useType)
            {
                case UseType.Health:
                    AddHealth();
                    Debug.Log("ȹ��");
                    break;
                case UseType.Score:
                    Debug.Log("ȹ��");
                    AddScore();
                    break;
                case UseType.Money:
                    Debug.Log("ȹ��");
                    AddMoney();
                    break;
            }
        }

        Destroy(gameObject);
    }

    private void AddHealth()
    {
        if (isBigger)
        {
            statusCached.Heal(3);
        }
        else
        {
            statusCached.Heal(1);
        }
    }

    private void AddMoney()
    {
        if (isBigger)
        {
            inventoryCached.gold += 20;
        }
        else
        {
            inventoryCached.gold += 10;
        }
    }

    private void AddScore()
    {
        if (isBigger)
        {
            GameManager.Instance.Score += 1000;
        }
        else
        {
            GameManager.Instance.Score += 100;
        }
    }
}
