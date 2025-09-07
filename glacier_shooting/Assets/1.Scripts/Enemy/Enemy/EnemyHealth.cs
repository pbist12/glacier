using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("HP")]
    public float maxHP = 3f;
    float hp;

    [Header("Auto Despawn Bounds (¿É¼Ç)")]
    public bool useBoundsDespawn = true;
    public Vector2 bounds = new Vector2(20f, 12f);

    void OnEnable() => hp = maxHP;

    public void TakeDamage(float dmg)
    {
        hp -= dmg;
        if (hp <= 0f) Die();
    }

    void Update()
    {
        if (!useBoundsDespawn) return;
        var p = transform.position;
        if (Mathf.Abs(p.x) > bounds.x || Mathf.Abs(p.y) > bounds.y)
            Die();
    }

    void Die()
    {
        GameManager.Instance.AddScore(10);
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Bullet"))
            TakeDamage(1f);
    }
}
