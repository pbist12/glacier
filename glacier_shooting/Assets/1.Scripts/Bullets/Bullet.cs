// File: Bullet.cs
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public BulletPool pool; // 되돌아갈 풀
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
        pool.Despawn(this);
    }

    // 플레이어와 충돌 시 외부에서 이걸 호출하도록 해도 됨(트리거/오버랩 등)
    void OnTriggerEnter2D(Collider2D other)
    {
        // 태그/레이어로 필터
        if (other.CompareTag("Player"))
        {
            // TODO: 플레이어 데미지 처리
            Despawn();
        }
    }
}
