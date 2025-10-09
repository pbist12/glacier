using Game.Data;
using System;
using System.Collections;

namespace Boss
{
    public interface IPatternRunner
    {

        /// 패턴 SO 하나를 "한 번" 실행(중간에 stop() true면 즉시 중단)
        IEnumerator RunOnce(PatternSOBase patternSO, BossRuntimeContext ctx, Func<bool> stop);
    }
}
