using UnityEngine;
using TMProText = TMPro.TextMeshProUGUI;

public class AdminButton : MonoBehaviour
{
    public void SetForceAds(bool b)
    {
        PlatformIapManager.SetForceAds(b);
    }
}