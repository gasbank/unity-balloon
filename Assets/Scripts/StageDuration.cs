using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StageDuration : MonoBehaviour {
    [SerializeField] TextMeshProUGUI text = null;

    void Update() {
        text.text = (DateTime.MinValue + TimeSpan.FromSeconds(Time.timeSinceLevelLoad)).ToString("mm:ss.fff");
    }
}
