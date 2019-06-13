using UnityEngine;

[DisallowMultipleComponent]
public class SingletonManager : MonoBehaviour {
    [SerializeField] private ConfirmPopup confirmPopup = null;
    [SerializeField] private ProgressMessage progressMessage = null;
    [SerializeField] private FontManager fontManager = null;
    [SerializeField] private PlatformIapManager platformIapManager = null;
    [SerializeField] private PlatformAdMobAdsInit platformAdMobAdsInit = null;
    [SerializeField] private Data data = null;

    void Awake() {
        confirmPopup = GameObject.FindObjectOfType<ConfirmPopup>();
        progressMessage = GameObject.FindObjectOfType<ProgressMessage>();
        
        ConfirmPopup.instance = confirmPopup;
        ProgressMessage.instance = progressMessage;
        FontManager.instance = fontManager;
        PlatformIapManager.instance = platformIapManager;
        PlatformAdMobAdsInit.instance = platformAdMobAdsInit;
        Data.instance = data;
    }
}
