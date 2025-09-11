// File: EnemyHealth.cs
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemyHealth : MonoBehaviour
{
    // ── 전역 관리: 활성 적 리스트/카운트 (EnemyRegistry 대체)
    public static readonly List<EnemyHealth> All = new();
    public static int ActiveCount => All.Count;

    [Header("HP")]
    public float maxHP = 3f;
    float hp;

    [Header("Auto Despawn Bounds (옵션)")]
    public bool useBoundsDespawn = true;
    public Vector2 bounds = new Vector2(20f, 12f);

    [Header("Collision Radius (중앙 충돌용)")]
    public float radius = 0.25f;

    [Header("DropItem (옵션)")]
    public EnemyDrop enemyDrop;

    void OnEnable()
    {
        hp = maxHP;
        if (!All.Contains(this)) All.Add(this);
    }

    void OnDisable()
    {
        All.Remove(this);
    }

    void Update()
    {
        if (!useBoundsDespawn) return;

        Vector3 p = transform.position;
        if (Mathf.Abs(p.x) > bounds.x || Mathf.Abs(p.y) > bounds.y)
            Die();
    }

    public void Hit(float damage)   // 중앙 매니저가 호출
    {
        hp -= damage;
        if (hp <= 0f) Die();
    }

    void Die()
    {
        if (GameManager.Instance) GameManager.Instance.AddScore(10);
        if (enemyDrop) enemyDrop.DropItem();
        Destroy(gameObject); // 풀을 쓴다면 SetActive(false)로 바꿔도 OK
    }

    // 트리거 방식은 더 이상 사용하지 않음(중앙 매니저가 처리)
    // void OnTriggerEnter2D(Collider2D col) { ... }
}
