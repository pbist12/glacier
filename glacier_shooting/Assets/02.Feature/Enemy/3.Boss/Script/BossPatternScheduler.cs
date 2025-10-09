// File: BossPatternScheduler.cs
using Game.Data; // BossPhaseSO 정의가 여기에 있다면 사용. 없다면 using 제거.
using System.Collections;
using UnityEngine;

namespace Boss
{
    [DisallowMultipleComponent]
    public class BossPatternScheduler : MonoBehaviour
    {
        [Header("Refs")]
        public PatternRunnerRegistry registry;
        public BossRuntimeContextProvider ctxProvider;
        public BulletPoolHub bulletPool; // 선택: 패턴 사이 정리

        private Coroutine _phaseCo;
        private bool _stopFlag;
        private BossPhaseSO _phase;
        private int _seqIndex;

        void Reset()
        {
            if (!ctxProvider) ctxProvider = GetComponent<BossRuntimeContextProvider>();
            if (!registry) registry = GetComponent<PatternRunnerRegistry>();
            if (!bulletPool) bulletPool = FindFirstObjectByType<BulletPoolHub>();
        }

        public void StartPhase(BossPhaseSO phase)
        {
            StopPhase();
            _phase = phase;
            _seqIndex = 0;
            _stopFlag = false;
            _phaseCo = StartCoroutine(PhaseLoop());
        }

        public void StopPhase()
        {
            _stopFlag = true;
            if (_phaseCo != null) { StopCoroutine(_phaseCo); _phaseCo = null; }
            // 필요 시 안전 정리
            // if (bulletPool) bulletPool.BombClearAll();
        }

        private IEnumerator PhaseLoop()
        {
            if (_phase == null || _phase.patterns == null || _phase.patterns.Count == 0)
                yield break;

            var ctx = ctxProvider ? ctxProvider.Build() : new BossRuntimeContext { Boss = transform, DeltaTime = () => Time.deltaTime };
            bool unscaled = _phase.useUnscaledTime;
            float cadence = Mathf.Max(0.1f, _phase.cadenceSeconds);

            while (!_stopFlag)
            {
                var entry = Pick(_phase);
                if (entry?.pattern == null) { yield return null; continue; }

                // 텔레그래프(공통)
                if (entry.pattern.telegraphSeconds > 0f)
                    yield return Wait(entry.pattern.telegraphSeconds, unscaled);

                // 러너 실행
                var runner = registry ? registry.Resolve(entry.pattern.kind) : null;
                if (runner != null)
                    yield return runner.RunOnce(entry.pattern, ctx, () => _stopFlag);
                else
                    yield return Wait(entry.pattern.actionSeconds, unscaled); // 폴백

                // 후딜(공통)
                if (entry.pattern.postDelaySeconds > 0f)
                    yield return Wait(entry.pattern.postDelaySeconds, unscaled);

                // 패턴 간 딜레이
                if (entry.delayAfter > 0f)
                    yield return Wait(entry.delayAfter, unscaled);

                // (선택) 탄 정리
                // if (bulletPool) bulletPool.BombClearAll();

                // cadence 박자(간단)
                yield return Wait(cadence, unscaled);
            }
        }

        private BossPhaseSO.PatternEntry Pick(BossPhaseSO phase)
        {
            var list = phase.patterns;
            if (list == null || list.Count == 0) return null;

            switch (phase.runMode)
            {
                case PhaseRunMode.Sequential:
                    return list[Mathf.Min(_seqIndex++, list.Count - 1)];
                case PhaseRunMode.LoopSequential:
                    var e = list[_seqIndex % list.Count];
                    _seqIndex++;
                    return e;
                case PhaseRunMode.WeightedRandom:
                default:
                    float total = 0f;
                    for (int i = 0; i < list.Count; i++) total += Mathf.Max(0f, list[i].weight);
                    if (total <= 0f) return list[0];
                    float r = Random.value * total, acc = 0f;
                    for (int i = 0; i < list.Count; i++)
                    {
                        acc += Mathf.Max(0f, list[i].weight);
                        if (r <= acc) return list[i];
                    }
                    return list[list.Count - 1];
            }
        }

        private static object Wait(float sec, bool unscaled)
            => unscaled ? (object)new WaitForSecondsRealtime(sec)
                        : new WaitForSeconds(sec);
    }

}
