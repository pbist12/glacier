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

        // SO������ �ڷ�ƾ ���� �� ������, �ܺ� Runner�� StartCoroutine �ϵ��� IEnumerator�� ��ȯ.
        public abstract System.Collections.IEnumerator Play(BossContext ctx);
    }
}
*/