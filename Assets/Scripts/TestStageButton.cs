using UnityEngine;
using UnityEngine.SceneManagement;
using TMProText = TMPro.TextMeshProUGUI;

public class TestStageButton : MonoBehaviour
{
    [SerializeField]
    string sceneName = "";

    [SerializeField]
    TMProText text;

    void OnValidate()
    {
        text.text = sceneName;
    }

    public void GoToStage()
    {
        SceneManager.LoadScene(sceneName);
    }
}