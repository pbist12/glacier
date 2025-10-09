using Game.Data;
using System;
using System.Collections;

namespace Boss
{
    public interface IPatternRunner
    {

        /// ���� SO �ϳ��� "�� ��" ����(�߰��� stop() true�� ��� �ߴ�)
        IEnumerator RunOnce(PatternSOBase patternSO, BossRuntimeContext ctx, Func<bool> stop);
    }
}
