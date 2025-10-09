// File: DeathRelay.cs
using System;
using UnityEngine;

public class DeathRelay : MonoBehaviour
{
    public event Action OnDied;

    void OnDestroy()
    {
        OnDied?.Invoke();
    }
}
