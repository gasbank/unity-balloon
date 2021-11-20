using UnityEngine;

[DisallowMultipleComponent]
public class SingletonManager : MonoBehaviour
{
    [SerializeField]
    ConfigPopup configPopup;

    [SerializeField]
    ConfirmPopup confirmPopup;

    [SerializeField]
    Data data;

    [SerializeField]
    FontManager fontManager;

    [SerializeField]
    PlatformAdMobAdsInit platformAdMobAdsInit;

    [SerializeField]
    PlatformIapManager platformIapManager;

    [SerializeField]
    ProgressMessage progressMessage;

    [SerializeField]
    PurchasingInProgress purchasingInProgress;

    void Awake()
    {
        confirmPopup = FindObjectOfType<ConfirmPopup>();
        configPopup = FindObjectOfType<ConfigPopup>();
        progressMessage = FindObjectOfType<ProgressMessage>();
        purchasingInProgress = FindObjectOfType<PurchasingInProgress>();

        ConfirmPopup.instance = confirmPopup;
        ConfigPopup.instance = configPopup;
        ProgressMessage.instance = progressMessage;
        FontManager.instance = fontManager;
        PlatformIapManager.instance = platformIapManager;
        PlatformAdMobAdsInit.instance = platformAdMobAdsInit;
        PurchasingInProgress.instance = purchasingInProgress;
        Data.instance = data;
    }
}