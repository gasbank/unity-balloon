using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverGroup : MonoBehaviour {
    [SerializeField] Canvas canvas = null;

    public bool Visible {
        get => canvas.enabled;
        set => canvas.enabled = value;
    }

    public void ReloadMainScene() {
        Bootstrap.ReloadCurrentScene();
    }

    public void SelectStage() {
        SceneManager.LoadScene("Stage Selection");
    }
}
