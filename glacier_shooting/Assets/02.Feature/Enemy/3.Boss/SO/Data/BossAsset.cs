using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Data
{
    #region Phase
    public enum SimpleRuleKind { HPBelow, TimerReached }
    public enum TransitionOutcome { Next, End }   // 점프 제거: 다음 or 종료만

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
        [Min(0f)] public float minSecondsInPhase = 0f;  // 이 시간 전엔 전환 X

        [Header("Transitions (Top-Down)")]
        [Tooltip("위에서부터 첫 매치 규칙으로 전환")]
        public SimpleRule transitions = new();

        [Header("Outcomes")]
        [Tooltip("규칙이 만족될 때의 행동: 기본 Next(다음 페이즈), End(종료)")]
        public TransitionOutcome onRule = TransitionOutcome.Next;

        [Header("Timeout")]
        [Min(0f)] public float hardTimeoutSeconds = 0f;  // 0=off
        [Tooltip("타임아웃 시 행동: Next(다음) 또는 End(종료)")]
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
        public List<PhaseEntry> phases = new();  // index 0 = 시작 페이즈

        [Header("Optional: Dialogue")]
        public DialogueData introDialogue;
        public DialogueData outroDialogue;

        [Header("Meta")]
        public Sprite icon;
        public string[] tags;
    }
}
