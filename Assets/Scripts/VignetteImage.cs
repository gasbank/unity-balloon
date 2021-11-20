using UnityEngine;
using UnityEngine.UI;

public class VignetteImage : MonoBehaviour
{
    [SerializeField]
    bool continued;

    [SerializeField]
    Image image;

    [SerializeField]
    Button showContinuePopupImmediatelyButton;

    public void SetVignetteColor(Color c)
    {
        image.color = c;
        if (continued == false) showContinuePopupImmediatelyButton.gameObject.SetActive(c.a != 0);
    }

    public void ShowContinuePopupImmediately()
    {
        var hotairBalloon = FindObjectOfType<HotairBalloon>();
        if (hotairBalloon != null) hotairBalloon.DieImmediately();
        HideShowContinuePopupImmediatelyButton();
    }

    public void HideShowContinuePopupImmediatelyButton()
    {
        SushiDebug.Log("HideShowContinuePopupImmediatelyButton");
        continued = true;
        showContinuePopupImmediatelyButton.gameObject.SetActive(false);
    }

    internal void ResetShowContinuePopupImmediatelyButton()
    {
        SushiDebug.Log("ResetShowContinuePopupImmediatelyButton");
        continued = false;
    }
}