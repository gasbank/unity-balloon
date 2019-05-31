using UnityEngine;
using TMPro;

public class FpsText : MonoBehaviour {
    [SerializeField] TextMeshProUGUI text = null;

    void Update() {
        text.text = (1.0f / Time.unscaledDeltaTime).ToString("f1");
    }
}
