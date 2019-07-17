using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class RestorePurchaseIapButton : MonoBehaviour {
    void Awake() {
        gameObject.SetActive(Application.platform == RuntimePlatform.IPhonePlayer);
    }
    
    public void RestorePurchase() {
        PlatformIapManager.instance.RestorePurchase();
    }
}
