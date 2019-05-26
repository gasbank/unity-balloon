using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToStage1Button : MonoBehaviour {
    public void ReturnToStage1() {
        StageCommon.awaken = false;
        Bootstrap.LastPlayedStageNumber = 1;
        SceneManager.LoadScene("Bootstrap");
    }
}
