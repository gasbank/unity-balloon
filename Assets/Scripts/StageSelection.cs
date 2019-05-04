using UnityEngine;
using UnityEngine.SceneManagement;

public class StageSelection : MonoBehaviour {
    public void GoToStage(string sceneName) {
        SceneManager.LoadScene(sceneName);
    }
}
