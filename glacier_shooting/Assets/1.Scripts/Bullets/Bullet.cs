// File: Bullet.cs
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [HideInInspector] public BulletPoolHub hub;        // ���� ����
    [HideInInspector] public BulletPoolKey poolKey;    // �� �Ҽ� Ǯ Ű

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
        hub.Despawn(this);
    }
}
