// File: BossHealth.cs
using System;
using UnityEngine;

[DisallowMultipleComponent]
public class BossHealth : MonoBehaviour
{
    [Header("HP")]
    public float maxHP = 100f;
    public float hp;

    [Header("Hit Filter")]
    [Tooltip("�÷��̾� ź�� ������ ��� Ȱ��ȭ�ϰ�, �±׸��� ���߼���.")]
    public string playerBulletTag = "PlayerBullet";

    [Header("BossUI")]
    public BossUI BossUI;

    public event Action onDeath;
    public event Action<float, float> onHpChanged; // (hp, max)

    void OnEnable()
    {
        BossUI = FindAnyObjectByType<BossUI>();
        hp = maxHP;
        onHpChanged?.Invoke(hp, maxHP);
        BossUI.BindBoss(this);
    }

    public void TakeDamage(float damage)
    {
        hp -= damage;
        onHpChanged?.Invoke(hp, maxHP);

        if (hp <= 0f)
        {
            onDeath?.Invoke();
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag(playerBulletTag))
        {
            var bullet = col.GetComponent<Bullet>();
            if (bullet)
            {
                TakeDamage(bullet.damage);   // �ʿ� �� ź���� ������ ���� Bullet�� �ΰ� ����
                bullet.Despawn(); // Ǯ ����
            }
        }
    }
}
