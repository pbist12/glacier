// File: PatternRunnerRegistry.cs
using Game.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Boss
{
    [System.Serializable]
    public class RunnerBinding
    {
        public PatternKind kind;         // Spread, Move, Laser...
        public MonoBehaviour runner;     // IPatternRunner 구현
    }

    [DisallowMultipleComponent]
    public class PatternRunnerRegistry : MonoBehaviour
    {
        [SerializeField] private List<RunnerBinding> bindings = new();
        private readonly Dictionary<PatternKind, IPatternRunner> _map = new();

        void Awake()
        {
            _map.Clear();
            foreach (var b in bindings)
            {
                if (b.runner is IPatternRunner r) _map[b.kind] = r;
                else if (b.runner != null)
                    Debug.LogWarning($"{b.runner.name} 는 IPatternRunner가 아닙니다.", b.runner);
            }
        }

        public IPatternRunner Resolve(PatternKind kind)
            => _map.TryGetValue(kind, out var r) ? r : null;
    }
}
