using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButton : MonoBehaviour {
    public void GoToStageSelection() => SceneManager.LoadScene("Stage Selection");
}
