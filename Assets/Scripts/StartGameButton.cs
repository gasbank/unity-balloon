using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGameButton : MonoBehaviour {
    public void GoToStageSelection() {
        SceneManager.LoadScene("Stage Selection");
    }
}
