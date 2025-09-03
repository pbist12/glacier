// File: Bullet.cs
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public BulletPool pool; // �ǵ��ư� Ǯ
    public Vector2 velocity; // �ʴ� �̵� ����
    public float lifetime;   // �� ���� ����

    float _age;
    Transform _tf;

    void Awake()
    {
        _tf = transform;
    }

    void OnEnable()
    {
        _age = 0f;
    }

    void Update()
    {
        // �̵�
        _tf.position += (Vector3)(velocity * Time.deltaTime);

        // ���� üũ
        _age += Time.deltaTime;
        if (_age >= lifetime)
        {
            Despawn();
        }
    }

    // ȭ��/���� �ٱ����� ���� ���� �����ϰ� �ʹٸ� �ܺο��� ȣ��
    public void Despawn()
    {
        if (!gameObject.activeSelf) return;
        pool.Despawn(this);
    }

    // �÷��̾�� �浹 �� �ܺο��� �̰� ȣ���ϵ��� �ص� ��(Ʈ����/������ ��)
    void OnTriggerEnter2D(Collider2D other)
    {
        // �±�/���̾�� ����
        if (other.CompareTag("Player"))
        {
            // TODO: �÷��̾� ������ ó��
            Despawn();
        }
    }
}
