using System;
using UnityEngine;

namespace Boss
{
    [System.Serializable]
    public class BossRuntimeContext
    {
        public Transform Boss;                  // 보스 본체
        public Transform Player;                // 플레이어 트랜스폼
        public Func<float> DeltaTime;           // scaled/unscaled 선택용
        public BulletPoolHub Bullets;           // 탄 풀 (선택)
        public BossPatternShooter Spread;       // 기존 스프레드 슈터
        public BossLaserShooter Laser;          // 기존 레이저 컴포넌트
        public BossMover Mover;                 // 이동 제어 래퍼(없으면 Transform 사용)

        // 유틸
        public Vector2 DirToPlayer() => (Player.position - Boss.position).normalized;
    }
}
