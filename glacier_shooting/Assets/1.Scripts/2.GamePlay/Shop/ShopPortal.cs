using UnityEngine;

public class ShopPortal : MonoBehaviour
{
    [Header("�Ÿ�/���� Ű")]
    public float interactRange = 1.4f;
    public KeyCode interactKey = KeyCode.E;

    private Transform _player;

    void Awake()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) _player = p.transform;
    }

    void Update()
    {
        if (_player == null) return;

        float dist = Vector2.Distance(transform.position, _player.position);
        if (dist <= interactRange && Input.GetKeyDown(interactKey))
        {
            // ���� ����
            //if (GameManager.Instance != null)
                //GameManager.Instance.EnterShop();
        }
    }
}
