using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BossSystem
{
    public class BossController : MonoBehaviour
    {
        [Header("Setup")]
        public Transform player;
        public BulletPoolHub bulletPool;

        [Header("Phases (순서대로)")]
        public BossPhase[] phases;

        [Header("HP")]
        public float maxHP = 1000f;
        public float currentHP = 1000f;

        [Header("Debug")]
        public bool autoStart = true;

        private int _phaseIndex = 0;
        private Dictionary<IBossPattern, float> _cooldowns = new();
        private Coroutine _loopCo;
        private bool _stunned = false;

        BossContext _ctx;

        void Awake()
        {
            _ctx = new BossContext
            {
                Boss = transform,
                Player = player,
                hub = bulletPool,
                Runner = this,
                Log = (msg) => Debug.Log($"[Boss] {msg}")
            };
        }

        void Start()
        {
            if (autoStart) _loopCo = StartCoroutine(PhaseLoop());
        }

        public void TakeDamage(float dmg)
        {
            currentHP = Mathf.Max(0, currentHP - dmg);
            // 페이즈 체크
            while (_phaseIndex < phases.Length &&
                   currentHP <= phases[_phaseIndex].endHpRatio * maxHP)
            {
                _phaseIndex++;
                _ctx.Log?.Invoke($"Phase -> #{_phaseIndex}");
            }

            if (currentHP <= 0) Die();
        }

        public void Stun(float seconds)
        {
            if (_stunned) return;
            _stunned = true;
            // 인터럽트 가능 패턴이면 바로 중단
            StopAllCoroutines();
            StartCoroutine(RecoverStunAfter(seconds));
        }

        IEnumerator RecoverStunAfter(float s)
        {
            yield return new WaitForSeconds(s);
            _stunned = false;
            _loopCo = StartCoroutine(PhaseLoop()); // 루프 재개
        }

        void Die()
        {
            StopAllCoroutines();
            // TODO: 연출
            _ctx.Log?.Invoke("Boss Dead");
        }

        IEnumerator PhaseLoop()
        {
            // 페이즈 끝까지 반복
            while (_phaseIndex < phases.Length && currentHP > 0)
            {
                var phase = phases[_phaseIndex];
                var pattern = PickPattern(phase.patterns);

                if (pattern == null)
                {
                    // 전부 쿨다운이면 살짝 대기
                    yield return new WaitForSeconds(0.3f);
                    continue;
                }

                // 패턴 실행
                var co = pattern.Play(_ctx);

                if (pattern.Uninterruptible) // 취소 불가 패턴은 별도 보호 실행
                    yield return StartCoroutine(co);
                else
                {
                    // 인터럽트 가능 – 스턴 시 StopAllCoroutines로 끊김
                    yield return StartCoroutine(co);
                    if (_stunned) yield break; // 스턴 진입 시 루프 종료(복귀 코루틴이 재개)
                }

                // 쿨다운 기록
                _cooldowns[pattern] = Time.time + pattern.Cooldown;

                // 패턴 사이 대기
                var wait = Random.Range(phase.idleBetweenPatterns.x, phase.idleBetweenPatterns.y);
                yield return new WaitForSeconds(wait);
            }
        }

        IBossPattern PickPattern(PatternAsset[] list)
        {
            // 사용 가능(쿨다운 종료) + 가중치 합산 후 룰렛
            var candidates = new List<IBossPattern>();
            float total = 0f;

            foreach (var p in list)
            {
                if (p == null) continue;
                if (_cooldowns.TryGetValue(p, out var until) && Time.time < until) continue;
                candidates.Add(p);
                total += Mathf.Max(0.0001f, p.Weight);
            }
            if (candidates.Count == 0) return null;

            float r = Random.value * total;
            foreach (var p in candidates)
            {
                r -= Mathf.Max(0.0001f, p.Weight);
                if (r <= 0) return p;
            }
            return candidates[candidates.Count - 1];
        }
    }
}
