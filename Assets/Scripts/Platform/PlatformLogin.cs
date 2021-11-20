using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class PlatformLogin : MonoBehaviour
{
    static bool initLogin;
    public CanvasGroup rootCanvasGroup;
    public Text userId;
    public Text userName;

    void Start()
    {
        // 게임 켜고 처음 한번만 자동 로그인 시도한다.
        if (initLogin == false)
            if (Bootstrap.CurrentStageNumber > 1)
            {
                StartLogin();
                initLogin = true;
            }
    }

    public void StartLogin()
    {
        //rootCanvasGroup.interactable = false;
        try
        {
            Platform.StartAuthAsync((result, reason) =>
            {
                if (rootCanvasGroup != null) rootCanvasGroup.interactable = true;
                SushiDebug.LogFormat("Social.localUser.Authenticate {0}", result);
                if (result)
                {
                    SushiDebug.LogFormat("Social.localUser userName={0}, userId={1}", Social.localUser.userName,
                        Social.localUser.id);
                    if (userName) userName.text = Social.localUser.userName;
                    if (userId) userId.text = Social.localUser.id;
                }
            });
        }
        catch
        {
            if (rootCanvasGroup != null) rootCanvasGroup.interactable = true;
        }
    }
}