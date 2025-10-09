/*using UnityEngine;

namespace BossSystem
{
    public abstract class PatternAsset : ScriptableObject, IBossPattern
    {
        [SerializeField] private string displayName = "Pattern";
        [SerializeField] private bool uninterruptible = false;
        [SerializeField] private float weight = 1f;
        [SerializeField] private float cooldown = 2f;

        public virtual string Name => displayName;
        public virtual bool Uninterruptible => uninterruptible;
        public virtual float Weight => weight;
        public virtual float Cooldown => cooldown;

        // SO에서는 코루틴 직접 못 돌리니, 외부 Runner가 StartCoroutine 하도록 IEnumerator만 반환.
        public abstract System.Collections.IEnumerator Play(BossContext ctx);
    }
}
*/