// File: BulletPool.cs
using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    [Header("Pool Setup")]
    public Bullet bulletPrefab;
    public int initialCount = 512;
    public bool expandable = true;
    public int expandBlock = 128;

    readonly Stack<Bullet> _pool = new Stack<Bullet>(1024);
    Transform _root;

    void Awake()
    {
        _root = transform;
        Prewarm(initialCount);
    }

    void Prewarm(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var b = Instantiate(bulletPrefab, _root);
            //b.pool = this;
            b.gameObject.SetActive(false);
            _pool.Push(b);
        }
    }

    public Bullet Spawn(Vector2 position, Vector2 velocity, float lifetime, float zRotationDeg = 0f)
    {
        if (_pool.Count == 0)
        {
            if (expandable) Prewarm(expandBlock);
            else return null;
        }

        var b = _pool.Pop();
        var tf = b.transform;
        tf.position = position;
        tf.rotation = Quaternion.Euler(0, 0, zRotationDeg);

        b.velocity = velocity;
        b.lifetime = lifetime;

        b.gameObject.SetActive(true);
        return b;
    }

    public void Despawn(Bullet b)
    {
        b.gameObject.SetActive(false);
        _pool.Push(b);
    }
}
