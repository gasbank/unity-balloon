using UnityEngine;
using TMProText = TMPro.TextMeshProUGUI;

public class TouchToStart : MonoBehaviour
{
    [SerializeField]
    TMProText text;

    void Update()
    {
        text.color = new Color(text.color.r, text.color.g, text.color.b, Mathf.PingPong(Time.time * 3, 1.0f));
    }
}