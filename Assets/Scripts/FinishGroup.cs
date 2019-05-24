using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishGroup : MonoBehaviour {
    public void ReloadMainScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SelectStage() {
        SceneManager.LoadScene("Stage Selection");
    }

    public void NextStage() {
        var nextStageNumber = int.Parse(SceneManager.GetActiveScene().name.Substring("Stage ".Length, 2)) + 1;
        if (nextStageNumber >= 16) {
            SceneManager.LoadScene("Ending");
        } else {
            SceneManager.LoadScene($"Stage {nextStageNumber:d2}");
        }
    }
}
