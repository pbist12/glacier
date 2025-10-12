using UnityEngine;
using Animancer;

[CreateAssetMenu(fileName = "PlayerAnimationSet", menuName = "Game/Anim/Player Animation Set")]
public class PlayerAnimationSet : ScriptableObject
{
    [Header("Locomotion")]
    [Tooltip("정지 상태")]
    public ClipTransition idle;
    [Tooltip("이동")]
    public ClipTransition moveLeft;
    public ClipTransition middleRight;

    [Header("Defaults")]
    [Tooltip("전환 기본 페이드 시간. 개별 Transition의 FadeDuration이 0이면 이 값 사용")]
    [Min(0f)] public float defaultFade = 0.12f;
}
