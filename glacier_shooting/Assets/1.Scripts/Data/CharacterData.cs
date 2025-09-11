using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayer", menuName = "Game/Player")]
public class CharacterData : ScriptableObject
{
    [Header("기본 정보")]
    public string playerName;         // 캐릭터 이름
    public Sprite portrait;           // 초상화(선택)

    [Header("능력치")]
    public int maxLife = 3;           // 목숨 수
    public int maxBombs = 2;          // 폭탄 수
    public float moveSpeed = 5f;      // 이동 속도
    public float focusSpeed = 2.5f;   // 집중 모드 속도
    public float hitboxRadius = 0.1f; // 피격 판정 반경

    [Header("공격")]
    public float damage = 1f;         // 공격력
    public float fireRate = 10f;      // 초당 발사 수
    public float bulletSpeed = 8f;    // 탄 속도
    public float bulletLifetime = 3f; // 탄 생존 시간

    [Header("폭탄/특수")]
    public float bombDuration = 2f;   // 폭탄 무적 시간
    public string bombDescription;    // 폭탄 설명

    [Header("기타")]
    public Color playerColor = Color.white;   // UI, 기체 색 등
    public AudioClip shotSfx;         // 기본 샷 사운드
    public AudioClip bombSfx;         // 폭탄 사운드
}
