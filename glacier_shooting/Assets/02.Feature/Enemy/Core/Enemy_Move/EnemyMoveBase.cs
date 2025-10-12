using UnityEngine;

/// <summary>
/// 모든 이동 모드의 부모. 컨트롤러가 매 프레임 Tick을 호출.
/// 파생 클래스는 Inspector 파라미터를 자유롭게 가질 수 있음.
/// </summary>
public abstract class EnemyMoveBase : MonoBehaviour
{
    /// <summary>컨트롤러가 주입하는 실행 컨텍스트</summary>
    protected EnemyMoveController Owner { get; private set; }

    /// <summary>컨트롤러가 활성화시 호출</summary>
    public virtual void Initialize(EnemyMoveController owner)
    {
        Owner = owner;
        OnActivated();
    }

    /// <summary>모드가 활성화될 때 한 번</summary>
    protected virtual void OnActivated() { }

    /// <summary>매 프레임 호출. pos를 갱신하고, 뱅킹용 수평속도(vx)를 반환.</summary>
    public abstract float Tick(ref Vector3 pos, float dt);
}
