// File: BossPatternScheduler.cs
using Game.Data; // BossPhaseSO ���ǰ� ���⿡ �ִٸ� ���. ���ٸ� using ����.
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
        public BulletPoolHub bulletPool; // ����: ���� ���� ����

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
            // �ʿ� �� ���� ����
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

                // �ڷ��׷���(����)
                if (entry.pattern.telegraphSeconds > 0f)
                    yield return Wait(entry.pattern.telegraphSeconds, unscaled);

                // ���� ����
                var runner = registry ? registry.Resolve(entry.pattern.kind) : null;
                if (runner != null)
                    yield return runner.RunOnce(entry.pattern, ctx, () => _stopFlag);
                else
                    yield return Wait(entry.pattern.actionSeconds, unscaled); // ����

                // �ĵ�(����)
                if (entry.pattern.postDelaySeconds > 0f)
                    yield return Wait(entry.pattern.postDelaySeconds, unscaled);

                // ���� �� ������
                if (entry.delayAfter > 0f)
                    yield return Wait(entry.delayAfter, unscaled);

                // (����) ź ����
                // if (bulletPool) bulletPool.BombClearAll();

                // cadence ����(����)
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
