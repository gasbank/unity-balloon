using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButton : MonoBehaviour
{
    [SerializeField]
    float lastTouchTime;

    [SerializeField]
    int repeatedTouchCount;

    public void GoToStageSelection()
    {
#if BALLOON_ADMIN
        if (Application.isEditor)
        {
            SceneManager.LoadScene("Stage Selection");
        }
        else
        {
            if (Time.time - lastTouchTime < 0.3f)
            {
                repeatedTouchCount++;
                if (repeatedTouchCount >= 10) SceneManager.LoadScene("Stage Selection");
            }
            else
            {
                repeatedTouchCount = 0;
            }

            lastTouchTime = Time.time;
        }
#endif
    }
}