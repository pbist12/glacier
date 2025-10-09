using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Data
{
    #region Phase
    public enum SimpleRuleKind { HPBelow, TimerReached }
    public enum TransitionOutcome { Next, End }   // ���� ����: ���� or ���Ḹ

    [Serializable]
    public class SimpleRule
    {
        public SimpleRuleKind kind = SimpleRuleKind.HPBelow;
        [Tooltip("HP: 0~1 / Timer: seconds")]
        public float value = 0.7f;
    }

    [Serializable]
    public class PhaseEntry
    {
        [Header("Phase Data")]
        public BossPhaseSO phase;

        [Header("Stay / Safety")]
        [Min(0f)] public float minSecondsInPhase = 0f;  // �� �ð� ���� ��ȯ X

        [Header("Transitions (Top-Down)")]
        [Tooltip("���������� ù ��ġ ��Ģ���� ��ȯ")]
        public SimpleRule transitions = new();

        [Header("Outcomes")]
        [Tooltip("��Ģ�� ������ ���� �ൿ: �⺻ Next(���� ������), End(����)")]
        public TransitionOutcome onRule = TransitionOutcome.Next;

        [Header("Timeout")]
        [Min(0f)] public float hardTimeoutSeconds = 0f;  // 0=off
        [Tooltip("Ÿ�Ӿƿ� �� �ൿ: Next(����) �Ǵ� End(����)")]
        public TransitionOutcome onTimeout = TransitionOutcome.Next;
    }
    #endregion

    [CreateAssetMenu(menuName = "Monster/Boss/BossAsset", fileName = "BossAsset")]
    public class BossAsset : ScriptableObject
    {
        [Header("Identity")]
        public string bossName;
        [TextArea] public string description;

        [Header("Prefab")]
        public GameObject bossPrefab;

        [Header("Health")]
        [Min(1f)] public float maxHealth = 1000f;

        [Header("Phases (Ordered)")]
        public List<PhaseEntry> phases = new();  // index 0 = ���� ������

        [Header("Optional: Dialogue")]
        public DialogueData introDialogue;
        public DialogueData outroDialogue;

        [Header("Meta")]
        public Sprite icon;
        public string[] tags;
    }
}
