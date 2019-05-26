using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishGroup : MonoBehaviour {
    [SerializeField] Canvas canvas = null;

    public bool Visible {
        get => canvas.enabled;
        set {
            canvas.enabled = value;

            // 이게 보였다는 것은 판을 깼다는 말.
            // 마지막으로 플레이한 스테이지를 갱신한다.
            if (value && Bootstrap.LastPlayedStageNumber < NextStageNumber) {
                Bootstrap.LastPlayedStageNumber = NextStageNumber;
            }
        }
    }

    static int NextStageNumber => Bootstrap.CurrentStageNumber + 1;

    public void ReloadMainScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SelectStage() {
        SceneManager.LoadScene("Stage Selection");
    }

    public void NextStage() {
        SceneManager.LoadScene(Bootstrap.GetStageSceneName(NextStageNumber));
    }
}
