using UnityEngine;

namespace BossSystem
{
    [System.Serializable]
    public class BossContext
    {
        public Transform Boss;            // ���� ��ü
        public Transform Player;          // �÷��̾� Ʈ������
        public BulletPoolHub hub;          // źȯ Ǯ
        public MonoBehaviour Runner;      // �ڷ�ƾ ���� ��ü
        public System.Action<string> Log; // ����׿�

        // ��ƿ
        public Vector2 DirToPlayer() => (Player.position - Boss.position).normalized;
    }
}
