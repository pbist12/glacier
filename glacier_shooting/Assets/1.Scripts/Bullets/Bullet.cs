using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [HideInInspector] public BulletPool pool; // �ǵ��ư� Ǯ
    [HideInInspector] public Vector2 velocity; // �ʴ� �̵� ����
    [HideInInspector] public float lifetime;   // �� ���� ����
    public float speed = 5f;

    float _age;
    Transform _tf;

    void Awake()
    {
        pool = GameObject.FindFirstObjectByType<BulletPool>();
        _tf = transform;
    }

    void OnEnable()
    {
        _age = 0f;
    }

    void Update()
    {
        transform.Translate(Vector3.down * speed * Time.deltaTime);

        _age += Time.deltaTime;
        if (_age >= lifetime)
        {
            Despawn();
        }
    }

    public void Despawn()
    {
        if (!gameObject.activeSelf) return;
        pool.Despawn(this);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Border"))
        {
            Despawn();
        }
    }
}
