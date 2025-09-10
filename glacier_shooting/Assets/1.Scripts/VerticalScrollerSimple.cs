using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class VerticalScrollerSimple : MonoBehaviour
{
    [Tooltip("스크롤 속도(+ = 아래)")]
    public float speed = 2f;

    [Header("Sprites to Cycle")]
    public List<Sprite> sprites = new List<Sprite>(); // 순서대로 사용할 스프라이트 목록
    private int spriteIndex = 0; // 현재 인덱스

    private Transform a, b;
    private SpriteRenderer srA, srB;
    private float spanY;

    void Start()
    {
        // 원본 SpriteRenderer 확보
        var src = GetComponent<SpriteRenderer>();
        if (src == null || (src.sprite == null && sprites.Count == 0))
        {
            Debug.LogError("SpriteRenderer 또는 Sprites List 필요!");
            enabled = false;
            return;
        }

        // 초기 스프라이트 세팅 (리스트가 비어있지 않으면 첫 번째로)
        if (sprites.Count > 0)
            src.sprite = sprites[0];

        // DrawMode 강제 Simple
        src.drawMode = SpriteDrawMode.Simple;

        // A 세그먼트
        var goA = new GameObject("Segment_A");
        a = goA.transform;
        a.SetParent(transform, false);
        srA = goA.AddComponent<SpriteRenderer>();
        CopySR(src, srA);

        // B 세그먼트
        var goB = new GameObject("Segment_B");
        b = goB.transform;
        b.SetParent(transform, false);
        srB = goB.AddComponent<SpriteRenderer>();
        CopySR(src, srB);

        // 높이 계산
        spanY = srA.bounds.size.y;
        if (spanY <= 0f) spanY = 1f;

        // 초기 배치
        a.localPosition = Vector3.zero;
        b.localPosition = new Vector3(0f, spanY, 0f);

        // 원본 SR 숨기기
        src.enabled = false;
    }

    void Update()
    {
        Vector3 delta = Vector3.down * (speed * Time.deltaTime);
        a.localPosition += delta;
        b.localPosition += delta;

        // 래핑 처리
        if (a.localPosition.y <= -spanY)
        {
            a.localPosition += new Vector3(0f, spanY * 2f, 0f);
            NextSprite(srA); // 스프라이트 교체
        }
        if (b.localPosition.y <= -spanY)
        {
            b.localPosition += new Vector3(0f, spanY * 2f, 0f);
            NextSprite(srB);
        }
    }

    void NextSprite(SpriteRenderer sr)
    {
        if (sprites.Count == 0) return;
        spriteIndex = (spriteIndex + 1) % sprites.Count;
        sr.sprite = sprites[spriteIndex];

        // 교체한 스프라이트 높이가 다를 수도 있으므로 span 갱신
        spanY = sr.bounds.size.y;
    }

    void CopySR(SpriteRenderer src, SpriteRenderer dst)
    {
        dst.sprite = src.sprite;
        dst.color = src.color;
        dst.flipX = src.flipX;
        dst.flipY = src.flipY;
        dst.sortingLayerID = src.sortingLayerID;
        dst.sortingOrder = src.sortingOrder;
        dst.sharedMaterial = src.sharedMaterial;
        dst.drawMode = SpriteDrawMode.Simple;
    }
}
