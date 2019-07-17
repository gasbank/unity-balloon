using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class NoAdsIapButton : MonoBehaviour {
    public void Purchase() {
        PlatformIapManager.instance.PurchaseNoAds();
    }

    public void ResetSaveData() {
        PlayerPrefs.DeleteAll();
    }
}
