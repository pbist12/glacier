// File: PatternRunnerRegistry.cs
using Game.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Boss
{
    [DisallowMultipleComponent]
    public class PatternRunnerRegistry : MonoBehaviour
    {
        // (선택) 인스펙터 확인용으로만 보여줄 리스트
        [SerializeField] private List<MonoBehaviour> debugRunners = new();

        private readonly Dictionary<PatternKind, IPatternRunner> _map = new();

        void Awake()
        {
            BuildMapFromChildren();
        }

        private void BuildMapFromChildren()
        {
            _map.Clear();
            debugRunners.Clear();

            // 비활성 자식까지 포함해서 모두 수집
            var monos = GetComponentsInChildren<MonoBehaviour>(true);

            foreach (var mb in monos)
            {
                if (mb is not IPatternRunner runner) continue;
                if (mb is not ISupportsPatternKind sk)
                {
                    Debug.LogWarning($"[PatternRunnerRegistry] '{mb.name}' 러너가 ISupportsPatternKind를 구현하지 않았습니다. 스킵.", mb);
                    continue;
                }

                var kind = sk.Kind;
                if (_map.ContainsKey(kind))
                {
                    Debug.LogWarning($"[PatternRunnerRegistry] Kind '{kind}'가 중복입니다. 기존 러너를 유지하고 '{mb.name}'는 무시합니다.", mb);
                    continue;
                }

                _map[kind] = runner;
                debugRunners.Add(mb); // 인스펙터에서 어떤 러너가 잡혔는지 확인용
            }

            if (_map.Count == 0)
                Debug.LogWarning("[PatternRunnerRegistry] 등록된 러너가 없습니다.", this);
        }

        public IPatternRunner Resolve(PatternKind kind)
            => _map.TryGetValue(kind, out var r) ? r : null;
    }
}
