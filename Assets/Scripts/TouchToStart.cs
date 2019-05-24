using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMProText = TMPro.TextMeshProUGUI;

public class TouchToStart : MonoBehaviour {
    [SerializeField] private TMProText text = null;

    void Update() {
        text.color = new Color(text.color.r, text.color.g, text.color.b, Mathf.PingPong(Time.time * 3, 1.0f));
    }
}
