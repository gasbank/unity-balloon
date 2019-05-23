using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMProText = TMPro.TextMeshProUGUI;

public class StageButton : MonoBehaviour {
    [SerializeField] TMProText text = null;

    int StageNumber => transform.GetSiblingIndex() + 1;

    void Awake() {
        text.text = "\\스테이지 {0}".Localized(StageNumber);
    }

    public void GoToStage() {
        SceneManager.LoadScene($"Stage {StageNumber.ToString("D2")}");
    }
}
