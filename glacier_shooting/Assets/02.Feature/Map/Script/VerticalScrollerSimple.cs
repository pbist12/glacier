using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class VerticalScrollerSimple : MonoBehaviour
{
    [Tooltip("��ũ�� �ӵ�(+ = �Ʒ�)")]
    public float speed = 2f;
    public float maxspeed;
    public float minspeed;
    [SerializeField] float lerpRate = 6f;   // Ŭ���� ��ǥ �ӵ��� ���� ����
    [SerializeField] bool useUnscaledTime = false;

    [Header("Sprites to Cycle")]
    public List<Sprite> sprites = new List<Sprite>(); // ������� ����� ��������Ʈ ���
    private int spriteIndex = 0; // ���� �ε���

    private Transform a, b;
    private SpriteRenderer srA, srB;
    private float spanY;

    public float currentSpeed; // ���� ���� �ӵ�

    void OnEnable()
    {
        currentSpeed = speed; // ���� �� ����ȭ
    }

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
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        // ��ǥ �ӵ�(speed)�� ���� �ε巴�� ����
        currentSpeed = Mathf.Lerp(currentSpeed, speed, dt * lerpRate);

        Vector3 delta = Vector3.down * (currentSpeed * dt);
        a.localPosition += delta;
        b.localPosition += delta;

        // ���� (��ӿ����� �����ϰ� while ����)
        Wrap(ref a, srA);
        Wrap(ref b, srB);
    }

    void Wrap(ref Transform t, SpriteRenderer sr)
    {
        while (t.localPosition.y <= -spanY)
        {
            t.localPosition += new Vector3(0f, spanY * 2f, 0f);
            NextSprite(sr);
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
