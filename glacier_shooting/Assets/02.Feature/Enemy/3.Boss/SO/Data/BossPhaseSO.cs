using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Data
{
    public enum PhaseRunMode { Sequential, LoopSequential, WeightedRandom }

    [CreateAssetMenu(menuName = "GameMini/Boss Phase", fileName = "BossPhaseSO")]
    public class BossPhaseSO : ScriptableObject
    {
        [Header("Meta")]
        public string phaseId = "P1";
        [TextArea] public string note;

        [Header("Run & Cadence")]
        public PhaseRunMode runMode = PhaseRunMode.WeightedRandom;
        [Min(0.1f)] public float cadenceSeconds = 2.2f;
        public bool useUnscaledTime = false;

        [Header("Patterns")]
        public List<PatternEntry> patterns = new();

        [Serializable]
        public class PatternEntry
        {
            public PatternSOBase pattern;         // SpreadPatternSO µÓ
            [Min(0f)] public float weight = 1f;   // WeightedRandom¿œ ∂ß
            [Min(0f)] public float delayAfter = 0.3f;
        }
    }
}
