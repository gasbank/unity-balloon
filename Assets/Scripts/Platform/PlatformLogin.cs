using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class PlatformLogin : MonoBehaviour {
    public Text userName;
    public Text userId;
    public CanvasGroup rootCanvasGroup;

    private void Start() {
        StartLogin();
    }

    public void StartLogin() {
        //rootCanvasGroup.interactable = false;
        try {
            Platform.StartAuthAsync((result, reason) => {
                rootCanvasGroup.interactable = true;
                SushiDebug.LogFormat("Social.localUser.Authenticate {0}", result);
                if (result) {
                    SushiDebug.LogFormat("Social.localUser userName={0}, userId={1}", Social.localUser.userName, Social.localUser.id);
                    if (userName) {
                        userName.text = Social.localUser.userName;
                    }
                    if (userId) {
                        userId.text = Social.localUser.id;
                    }
                }
            });
        } catch {
            rootCanvasGroup.interactable = true;
        }
    }

    public void ReturnToGame() {
        SceneManager.LoadScene("main");
    }
}
