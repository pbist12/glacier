// File: EnemyHealth.cs
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemyHealth : MonoBehaviour
{
    // ���� ���� ����: Ȱ�� �� ����Ʈ/ī��Ʈ (EnemyRegistry ��ü)
    public static readonly List<EnemyHealth> All = new();
    public static int ActiveCount => All.Count;

    [Header("HP")]
    public float maxHP = 3f;
    float hp;

    [Header("Auto Despawn Bounds (�ɼ�)")]
    public bool useBoundsDespawn = true;
    public Vector2 bounds = new Vector2(20f, 12f);

    [Header("Collision Radius (�߾� �浹��)")]
    public float radius = 0.25f;

    [Header("DropItem (�ɼ�)")]
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

    public void Hit(float damage)   // �߾� �Ŵ����� ȣ��
    {
        hp -= damage;
        if (hp <= 0f) Die();
    }

    void Die()
    {
        if (GameManager.Instance) GameManager.Instance.AddScore(10);
        if (enemyDrop) enemyDrop.DropItem();
        Destroy(gameObject); // Ǯ�� ���ٸ� SetActive(false)�� �ٲ㵵 OK
    }

    // Ʈ���� ����� �� �̻� ������� ����(�߾� �Ŵ����� ó��)
    // void OnTriggerEnter2D(Collider2D col) { ... }
}
