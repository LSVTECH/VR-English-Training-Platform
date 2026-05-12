using UnityEngine;
using TMPro;

public class SubtitleDisplay : MonoBehaviour
{
    public TextMeshProUGUI subtitleText;

    public void ShowSubtitle(string text)
    {
        if (subtitleText != null) subtitleText.text = text;
        Debug.Log($"Subtitle: {text}");
    }
}
