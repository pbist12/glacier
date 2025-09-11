using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayer", menuName = "Game/Player")]
public class CharacterData : ScriptableObject
{
    [Header("�⺻ ����")]
    public string playerName;         // ĳ���� �̸�
    public Sprite portrait;           // �ʻ�ȭ(����)

    [Header("�ɷ�ġ")]
    public int maxLife = 3;           // ��� ��
    public int maxBombs = 2;          // ��ź ��
    public float moveSpeed = 5f;      // �̵� �ӵ�
    public float focusSpeed = 2.5f;   // ���� ��� �ӵ�
    public float hitboxRadius = 0.1f; // �ǰ� ���� �ݰ�

    [Header("����")]
    public float damage = 1f;         // ���ݷ�
    public float fireRate = 10f;      // �ʴ� �߻� ��
    public float bulletSpeed = 8f;    // ź �ӵ�
    public float bulletLifetime = 3f; // ź ���� �ð�

    [Header("��ź/Ư��")]
    public float bombDuration = 2f;   // ��ź ���� �ð�
    public string bombDescription;    // ��ź ����

    [Header("��Ÿ")]
    public Color playerColor = Color.white;   // UI, ��ü �� ��
    public AudioClip shotSfx;         // �⺻ �� ����
    public AudioClip bombSfx;         // ��ź ����
}
