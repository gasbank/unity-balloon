using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour {
    static readonly string LAST_PLAYED_STAGE_NUMBER = "LAST_PLAYED_STAGE_NUMBER";
    static readonly int LAST_STAGE_NUMBER = 15;
    static readonly int ENDING_SCENE_NUMBER = LAST_STAGE_NUMBER + 1;
    static string ENDING_SCENE_NAME = "Ending";

    public static string GetStageSceneName(int stageNumber) {
        if (stageNumber == ENDING_SCENE_NUMBER) {
            return ENDING_SCENE_NAME;
        } else {
            return $"Stage {stageNumber:d2}";
        }
    }

    public static int LastPlayedStageNumber {
        get => PlayerPrefs.GetInt(LAST_PLAYED_STAGE_NUMBER, 1);
        set {
            PlayerPrefs.SetInt(LAST_PLAYED_STAGE_NUMBER, Mathf.Clamp(value, 1, ENDING_SCENE_NUMBER));
            PlayerPrefs.Save();
        }
    }

    public static int GetStageNumber(string sceneName) {
        if (sceneName == ENDING_SCENE_NAME) {
            return ENDING_SCENE_NUMBER;
        } else {
            return int.Parse(sceneName.Substring("Stage ".Length, 2));
        }
    }

    public static int CurrentStageNumber => GetStageNumber(SceneManager.GetActiveScene().name);

    void Awake() {
        SceneManager.LoadScene(GetStageSceneName(LastPlayedStageNumber));
    }
}
