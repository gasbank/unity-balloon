using UnityEngine;

public class NoAdsIapButton : MonoBehaviour
{
    public void Purchase()
    {
        PlatformIapManager.instance.PurchaseNoAds();
    }

    public void ResetSaveData()
    {
        PlayerPrefs.DeleteAll();
    }
}