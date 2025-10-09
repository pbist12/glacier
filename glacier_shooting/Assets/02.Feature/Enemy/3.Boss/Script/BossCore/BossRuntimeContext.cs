using System;
using UnityEngine;

namespace Boss
{
    [System.Serializable]
    public class BossRuntimeContext
    {
        public Transform Boss;                  // ���� ��ü
        public Transform Player;                // �÷��̾� Ʈ������
        public Func<float> DeltaTime;           // scaled/unscaled ���ÿ�
        public BulletPoolHub Bullets;           // ź Ǯ (����)
        public BossPatternShooter Spread;       // ���� �������� ����
        public BossLaserShooter Laser;          // ���� ������ ������Ʈ
        public BossMover Mover;                 // �̵� ���� ����(������ Transform ���)

        // ��ƿ
        public Vector2 DirToPlayer() => (Player.position - Boss.position).normalized;
    }
}
