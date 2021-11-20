using UnityEngine;
using UnityEngine.UI;

public class StageCommon : MonoBehaviour
{
    public static bool awaken;
    bool bfadeout;

    [SerializeField]
    Image image;

    [SerializeField]
    GameObject titleGroup;

    [SerializeField]
    GameObject titleImage;

    public bool IsTitleVisible => titleGroup.activeSelf;

    void Awake()
    {
        // 게임 첫 실행했을 때만 타이틀 이미지 보인다.
        titleImage.SetActive(awaken == false);
        awaken = true;
    }

    public void DeactivateTitleGroup()
    {
        if (image.color != null)
        {
            image.color -= new Color(0, 0, 0, Time.deltaTime);
            // image.color.a -= Time.deltaTime;
            // titleImage.a = color.a ;
            bfadeout = true;
            if (image.color.a <= 0)
                titleGroup.SetActive(false);
        }
    }

    public void Update()
    {
        if (bfadeout)
            DeactivateTitleGroup();
    }
}