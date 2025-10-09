// File: PatternRunnerRegistry.cs
using Game.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Boss
{
    [DisallowMultipleComponent]
    public class PatternRunnerRegistry : MonoBehaviour
    {
        // (����) �ν����� Ȯ�ο����θ� ������ ����Ʈ
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

            // ��Ȱ�� �ڽı��� �����ؼ� ��� ����
            var monos = GetComponentsInChildren<MonoBehaviour>(true);

            foreach (var mb in monos)
            {
                if (mb is not IPatternRunner runner) continue;
                if (mb is not ISupportsPatternKind sk)
                {
                    Debug.LogWarning($"[PatternRunnerRegistry] '{mb.name}' ���ʰ� ISupportsPatternKind�� �������� �ʾҽ��ϴ�. ��ŵ.", mb);
                    continue;
                }

                var kind = sk.Kind;
                if (_map.ContainsKey(kind))
                {
                    Debug.LogWarning($"[PatternRunnerRegistry] Kind '{kind}'�� �ߺ��Դϴ�. ���� ���ʸ� �����ϰ� '{mb.name}'�� �����մϴ�.", mb);
                    continue;
                }

                _map[kind] = runner;
                debugRunners.Add(mb); // �ν����Ϳ��� � ���ʰ� �������� Ȯ�ο�
            }

            if (_map.Count == 0)
                Debug.LogWarning("[PatternRunnerRegistry] ��ϵ� ���ʰ� �����ϴ�.", this);
        }

        public IPatternRunner Resolve(PatternKind kind)
            => _map.TryGetValue(kind, out var r) ? r : null;
    }
}
