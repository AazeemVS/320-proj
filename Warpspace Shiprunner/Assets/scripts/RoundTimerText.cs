using UnityEngine;
using TMPro;

public class RoundTimerText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI label;
    [SerializeField] GameTimer gameTimer;

    void Reset() => label = GetComponent<TextMeshProUGUI>();

    void Update()
    {
        float t = Mathf.Max(0f, gameTimer.Remaining);
        int secs = Mathf.CeilToInt(t);
        label.text = $"{secs / 60:0}:{secs % 60:00}";
    }
}
