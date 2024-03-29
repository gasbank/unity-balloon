﻿using UnityEngine;
using TMProText = TMPro.TextMeshProUGUI;

public class StageButton : MonoBehaviour
{
    [SerializeField]
    TMProText text;

    int ButtonStageNumber => transform.GetSiblingIndex() + 1;

    void Awake()
    {
        text.text = "\\스테이지 {0}".Localized(ButtonStageNumber);
    }

    public void GoToStage()
    {
        //SceneManager.LoadScene(Bootstrap.GetStageSceneName(ButtonStageNumber));
        HotairBalloon.InitialPositionY = 0;
        StageCommon.awaken = false;
        Bootstrap.LoadStageScene(ButtonStageNumber);
    }
}