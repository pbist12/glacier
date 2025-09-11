// File: Bullet.cs
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [HideInInspector] public BulletPoolHub hub;        // 허브로 복귀
    [HideInInspector] public BulletPoolKey poolKey;    // 내 소속 풀 키

    public Vector2 velocity; // 초당 이동 벡터
    public float lifetime;   // 초 단위 수명

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
        // 이동
        _tf.position += (Vector3)(velocity * Time.deltaTime);

        // 수명 체크
        _age += Time.deltaTime;
        if (_age >= lifetime)
        {
            Despawn();
        }
    }

    // 화면/월드 바깥으로 나갈 때도 제거하고 싶다면 외부에서 호출
    public void Despawn()
    {
        if (!gameObject.activeSelf) return;
        hub.Despawn(this);
    }
}
