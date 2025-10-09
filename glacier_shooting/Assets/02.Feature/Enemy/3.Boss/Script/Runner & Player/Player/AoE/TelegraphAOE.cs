using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TelegraphAOE : MonoBehaviour
{
    [Header("Shape / Visual")]
    public SpriteRenderer sr;            // 원형 표시 스프라이트
    public float maxRadius = 2.5f;       // 최종 반경 (로컬 스케일로 반영)
    public Color warnColor = new Color();
    public Color activeColor = new Color();
    public float pulseSpeed = 6f;        // 경고 중 깜빡임 속도

    [Header("Timings")]
    public float warnTime = 1.2f;        // 경고 시간
    public float homingStopTime = 1.0f;
    public float activeTime = 1.6f;      // 실제 피해 시간
    public float fadeOutTime = 0.25f;    // 종료 페이드

    [Header("Targeting")]
    public bool trackPlayerDuringWarn = true;  // 경고 중 플레이어 추적
    public float trackLerp = 10f;              // 추적 보간 속도

    [Header("Damage")]
    public string playerTag = "Player";

    private CircleCollider2D col;
    private Transform player;

    void Awake()
    {
        col = GetComponent<CircleCollider2D>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        col.isTrigger = true;
        col.enabled = false; // 경고 동안은 비활성
        transform.localScale = Vector3.zero; // 작게 시작(확대 연출)
    }

    void OnEnable()
    {
        var p = GameObject.FindGameObjectWithTag(playerTag);
        if (p) player = p.transform;
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        // WARN: 크기 키우고, 색 펄스, 필요 시 플레이어 추적
        float t = 0f;
        while (t < warnTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / warnTime);
            // 부드러운 확대
            float scale = Mathf.Lerp(0f, maxRadius * 2f, k); // 원 스프라이트 기준 지름 스케일
            transform.localScale = new Vector3(scale, scale, 1f);

            // 색 펄스(밝기 변조)
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed);
            var c = warnColor; c.a *= Mathf.Lerp(0.4f, 0.8f, pulse);
            sr.color = c;

            // 경고 중 추적
            if (trackPlayerDuringWarn && player != null && t <= homingStopTime)
            {
                transform.position = Vector3.Lerp(transform.position, player.position, trackLerp * Time.deltaTime);
            }
            yield return null;
        }

        // ACTIVE: 충돌 켜고, 색 고정
        col.enabled = true;
        sr.color = activeColor;

        t = 0f;
        while (t < activeTime)
        {
            t += Time.deltaTime;
            yield return null;
        }

        // FADE OUT: 충돌 끄고 서서히 사라짐
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
