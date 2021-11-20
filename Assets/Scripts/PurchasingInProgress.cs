using UnityEngine;

public class PurchasingInProgress : MonoBehaviour
{
    public static PurchasingInProgress instance;

    [SerializeField]
    CanvasGroup canvasGroup;

    public void Show(bool b)
    {
        if (b)
        {
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
        }
    }
}