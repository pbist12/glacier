using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class VerticalScrollerSimple : MonoBehaviour
{
    [Tooltip("��ũ�� �ӵ�(+ = �Ʒ�)")]
    public float speed = 2f;

    [Header("Sprites to Cycle")]
    public List<Sprite> sprites = new List<Sprite>(); // ������� ����� ��������Ʈ ���
    private int spriteIndex = 0; // ���� �ε���

    private Transform a, b;
    private SpriteRenderer srA, srB;
    private float spanY;

    void Start()
    {
        // ���� SpriteRenderer Ȯ��
        var src = GetComponent<SpriteRenderer>();
        if (src == null || (src.sprite == null && sprites.Count == 0))
        {
            Debug.LogError("SpriteRenderer �Ǵ� Sprites List �ʿ�!");
            enabled = false;
            return;
        }

        // �ʱ� ��������Ʈ ���� (����Ʈ�� ������� ������ ù ��°��)
        if (sprites.Count > 0)
            src.sprite = sprites[0];

        // DrawMode ���� Simple
        src.drawMode = SpriteDrawMode.Simple;

        // A ���׸�Ʈ
        var goA = new GameObject("Segment_A");
        a = goA.transform;
        a.SetParent(transform, false);
        srA = goA.AddComponent<SpriteRenderer>();
        CopySR(src, srA);

        // B ���׸�Ʈ
        var goB = new GameObject("Segment_B");
        b = goB.transform;
        b.SetParent(transform, false);
        srB = goB.AddComponent<SpriteRenderer>();
        CopySR(src, srB);

        // ���� ���
        spanY = srA.bounds.size.y;
        if (spanY <= 0f) spanY = 1f;

        // �ʱ� ��ġ
        a.localPosition = Vector3.zero;
        b.localPosition = new Vector3(0f, spanY, 0f);

        // ���� SR �����
        src.enabled = false;
    }

    void Update()
    {
        Vector3 delta = Vector3.down * (speed * Time.deltaTime);
        a.localPosition += delta;
        b.localPosition += delta;

        // ���� ó��
        if (a.localPosition.y <= -spanY)
        {
            a.localPosition += new Vector3(0f, spanY * 2f, 0f);
            NextSprite(srA); // ��������Ʈ ��ü
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

        // ��ü�� ��������Ʈ ���̰� �ٸ� ���� �����Ƿ� span ����
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
