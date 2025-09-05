using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TelegraphAOE : MonoBehaviour
{
    [Header("Shape / Visual")]
    public SpriteRenderer sr;            // ���� ǥ�� ��������Ʈ
    public float maxRadius = 2.5f;       // ���� �ݰ� (���� �����Ϸ� �ݿ�)
    public Color warnColor = new Color();
    public Color activeColor = new Color();
    public float pulseSpeed = 6f;        // ��� �� ������ �ӵ�

    [Header("Timings")]
    public float warnTime = 1.2f;        // ��� �ð�
    public float homingStopTime = 1.0f;
    public float activeTime = 1.6f;      // ���� ���� �ð�
    public float fadeOutTime = 0.25f;    // ���� ���̵�

    [Header("Targeting")]
    public bool trackPlayerDuringWarn = true;  // ��� �� �÷��̾� ����
    public float trackLerp = 10f;              // ���� ���� �ӵ�

    [Header("Damage")]
    public string playerTag = "Player";

    private CircleCollider2D col;
    private Transform player;

    void Awake()
    {
        col = GetComponent<CircleCollider2D>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        col.isTrigger = true;
        col.enabled = false; // ��� ������ ��Ȱ��
        transform.localScale = Vector3.zero; // �۰� ����(Ȯ�� ����)
    }

    void OnEnable()
    {
        var p = GameObject.FindGameObjectWithTag(playerTag);
        if (p) player = p.transform;
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        // WARN: ũ�� Ű���, �� �޽�, �ʿ� �� �÷��̾� ����
        float t = 0f;
        while (t < warnTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / warnTime);
            // �ε巯�� Ȯ��
            float scale = Mathf.Lerp(0f, maxRadius * 2f, k); // �� ��������Ʈ ���� ���� ������
            transform.localScale = new Vector3(scale, scale, 1f);

            // �� �޽�(��� ����)
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed);
            var c = warnColor; c.a *= Mathf.Lerp(0.4f, 0.8f, pulse);
            sr.color = c;

            // ��� �� ����
            if (trackPlayerDuringWarn && player != null && t <= homingStopTime)
            {
                transform.position = Vector3.Lerp(transform.position, player.position, trackLerp * Time.deltaTime);
            }
            yield return null;
        }

        // ACTIVE: �浹 �Ѱ�, �� ����
        col.enabled = true;
        sr.color = activeColor;

        t = 0f;
        while (t < activeTime)
        {
            t += Time.deltaTime;
            yield return null;
        }

        // FADE OUT: �浹 ���� ������ �����
        col.enabled = false;

        float startA = sr.color.a;
        t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(startA, 0f, t / fadeOutTime);
            var c = sr.color; c.a = a; sr.color = c;
            yield return null;
        }

        Destroy(gameObject);
    }

}
