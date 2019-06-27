using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMProText = TMPro.TextMeshProUGUI;

public class StageButton : MonoBehaviour {
    [SerializeField] TMProText text = null;

    int ButtonStageNumber => transform.GetSiblingIndex() + 1;

    void Awake() {
        text.text = "\\스테이지 {0}".Localized(ButtonStageNumber);
    }

    public void GoToStage() {
        //SceneManager.LoadScene(Bootstrap.GetStageSceneName(ButtonStageNumber));
        HotairBalloon.InitialPositionY = 0;
        Bootstrap.LoadStageScene(ButtonStageNumber);
    }
}
