using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverGroup : MonoBehaviour {
    public void ReloadMainScene() {
        SceneManager.LoadScene("Main");
    }
}
