using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [HideInInspector] public BulletPool pool; // 되돌아갈 풀
    [HideInInspector] public Vector2 velocity; // 초당 이동 벡터
    [HideInInspector] public float lifetime;   // 초 단위 수명
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
