using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowedStatus : MonoBehaviour
{
    private player_movement _pm;
    private float _baseSpeed;
    private bool _initialized;

    private readonly List<float> _activeMultipliers = new();

    void Awake()
    {
        _pm = GetComponent<player_movement>();
        if (_pm == null)
        {
            Debug.LogWarning("SlowedStatus: player_movement not found on Player.");
            enabled = false;
            return;
        }
    }

    private void InitIfNeeded()
    {
        if (_initialized) return;
        _baseSpeed = _pm.moveSpeed;
        _initialized = true;
    }

    public void ApplySlow(float percent, float duration)
    {
        InitIfNeeded();

        percent = Mathf.Clamp01(percent);
        float multiplier = 1f - percent;
        if (duration <= 0f) return;

        _activeMultipliers.Add(multiplier);
        RecomputeSpeed();

        StartCoroutine(RemoveAfter(duration, multiplier));
    }

    private IEnumerator RemoveAfter(float seconds, float multiplier)
    {
        yield return new WaitForSeconds(seconds);
        _activeMultipliers.Remove(multiplier);
        RecomputeSpeed();
    }

    private void RecomputeSpeed()
    {
        if (!_initialized) return;

        float product = 1f;
        for (int i = 0; i < _activeMultipliers.Count; i++)
            product *= _activeMultipliers[i];

        _pm.moveSpeed = _baseSpeed * product;
    }
}
