using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageName : MonoBehaviour {
    [SerializeField] TextMeshProUGUI text = null;

    // void OnValidate() {
    //     if (gameObject.scene.rootCount != 0) {
    //         text.text = SceneManager.GetActiveScene().name;
    //     }
    // }

    void Awake() {
        SetStageName(SceneManager.GetActiveScene().name);
    }

    public void SetStageName(string stageName) {
        text.text = stageName;
    }
}
