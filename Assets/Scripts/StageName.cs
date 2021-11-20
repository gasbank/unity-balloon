using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageName : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI text;

    // void OnValidate() {
    //     if (gameObject.scene.rootCount != 0) {
    //         text.text = SceneManager.GetActiveScene().name;
    //     }
    // }

    void Awake()
    {
        SetStageName(SceneManager.GetActiveScene().name);
    }

    public void SetStageName(string stageName)
    {
        if (stageName.StartsWith("Levels/")) stageName = stageName.Substring("Levels/".Length);
        text.text = stageName;
    }
}