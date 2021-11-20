using TMPro;
using UnityEngine;

public class FpsText : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI text;

    void Update()
    {
        text.text = (1.0f / Time.unscaledDeltaTime).ToString("f1");
    }
}