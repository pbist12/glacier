using System.Collections;

namespace BossSystem
{
    public interface IBossPattern
    {
        string Name { get; }
        bool Uninterruptible { get; } // true�� �����ߴ� �Ұ�
        float Weight { get; }         // ���� ����ġ
        float Cooldown { get; }       // ���� �� ��ٿ�(��)

        // ���� ����. �ܺο��� StopCoroutine �� ���ͷ�Ʈ.
        IEnumerator Play(BossContext ctx);
    }
}
