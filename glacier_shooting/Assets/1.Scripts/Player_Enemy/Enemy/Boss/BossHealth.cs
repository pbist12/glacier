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
    [Tooltip("플레이어 탄만 적용할 경우 활성화하고, 태그명을 맞추세요.")]
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
                TakeDamage(bullet.damage);   // 필요 시 탄마다 데미지 값을 Bullet에 두고 참조
                bullet.Despawn(); // 풀 복귀
            }
        }
    }
}
