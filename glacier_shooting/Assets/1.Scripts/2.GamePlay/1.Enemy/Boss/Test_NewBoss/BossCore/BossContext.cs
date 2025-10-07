using UnityEngine;

namespace BossSystem
{
    [System.Serializable]
    public class BossContext
    {
        public Transform Boss;            // 보스 본체
        public Transform Player;          // 플레이어 트랜스폼
        public BulletPoolHub hub;          // 탄환 풀
        public MonoBehaviour Runner;      // 코루틴 실행 주체
        public System.Action<string> Log; // 디버그용

        // 유틸
        public Vector2 DirToPlayer() => (Player.position - Boss.position).normalized;
    }
}
