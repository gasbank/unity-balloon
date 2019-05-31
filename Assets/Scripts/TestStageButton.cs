using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMProText = TMPro.TextMeshProUGUI;

public class TestStageButton : MonoBehaviour {
    [SerializeField] TMProText text = null;
    [SerializeField] string sceneName = "";

    void OnValidate() {
        text.text = sceneName;
    }

    public void GoToStage() {
        SceneManager.LoadScene(sceneName);
    }
}
