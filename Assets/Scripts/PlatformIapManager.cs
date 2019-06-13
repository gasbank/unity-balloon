using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Purchasing;

public class PlatformIapManager : MonoBehaviour, IStoreListener {
    internal static PlatformIapManager instance;
    static readonly string NO_ADS_PRODUCT_ID = "top.plusalpha.balloon.noads";
    static readonly string NO_ADS_PREF_KEY = "NO_ADS_PREF_KEY";
    static readonly string FORCE_ADS_PREF_KEY = "FORCE_ADS_PREF_KEY";
    private IStoreController controller;
    private IExtensionProvider extensions;
    [SerializeField] GameObject iapGroup = null;

    public bool ForceAds {
        get => PlayerPrefs.GetInt(FORCE_ADS_PREF_KEY, 0) != 0;
        set {
            SetForceAds(value);
            SushiDebug.Log($"ForceAds set to {value}");
        }
    }

    public bool NoAdsPurchased {
        get => ForceAds ? false : PlayerPrefs.GetInt(NO_ADS_PREF_KEY, 0) != 0;
        private set {
            SetNoAdsPurchased_Admin(ForceAds ? false : value);
            SushiDebug.Log($"NoAdsPurchased set to {NoAdsPurchased}");
            SyncNoAdsStates();
        }
    }

    static public void SetNoAdsPurchased_Admin(bool b) {
        PlayerPrefs.SetInt(NO_ADS_PREF_KEY, b ? 1 : 0);
        PlayerPrefs.Save();
    }

    static public void SetForceAds(bool b) {
        PlayerPrefs.SetInt(FORCE_ADS_PREF_KEY, b ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void SyncNoAdsStates() {
        iapGroup.SetActive(NoAdsPurchased == false);
        if (NoAdsPurchased) {
            PlatformAdMobAdsInit.instance.HideBanner();
        } else {
            PlatformAdMobAdsInit.instance.StartShowBanner();
        }
    }

    void Awake() {
        iapGroup = GameObject.Find("Canvas/IAP Group");

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(NO_ADS_PRODUCT_ID, ProductType.NonConsumable);
        UnityPurchasing.Initialize(this, builder);

        // 일단 로컬 저장된 상태만으로 빠르게 판단해서 배너 광고를 보여줘야 하면 보여준다.
        SyncNoAdsStates();
    }

    void Update() {
        if (Application.isEditor && Input.GetKeyDown(KeyCode.F8)) {
            NoAdsPurchased = false;
        }
    }

    /// <summary>
    /// Called when Unity IAP is ready to make purchases.
    /// </summary>
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions) {
        this.controller = controller;
        this.extensions = extensions;
        var sb = new StringBuilder();
        sb.AppendLine($"PlatformIapManager.OnInitialized");
        foreach (var product in controller.products.all) {
            sb.AppendLine($" - {product.definition.id}: hasReceipt={product.hasReceipt}");
            if (product.hasReceipt) {
                if (product.definition.id == NO_ADS_PRODUCT_ID) {
                    NoAdsPurchased = true;
                }
            }
        }
        SushiDebug.Log(sb.ToString());
    }

    /// <summary>
    /// Called when Unity IAP encounters an unrecoverable initialization error.
    ///
    /// Note that this will not be called if Internet is unavailable; Unity IAP
    /// will attempt initialization until it becomes available.
    /// </summary>
    public void OnInitializeFailed(InitializationFailureReason error) {
        Debug.LogError($"PlatformIapManager.OnInitializeFailed: {error}");
    }

    /// <summary>
    /// Called when a purchase completes.
    ///
    /// May be called at any time after OnInitialized().
    /// </summary>
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e) {
        var sb = new StringBuilder();
        sb.AppendLine($"PlatformIapManager.ProcessPurchase");
        sb.AppendLine($"availableToPurchase = {e.purchasedProduct.availableToPurchase}");
        sb.AppendLine($"hasReceipt = {e.purchasedProduct.hasReceipt}");
        sb.AppendLine($"receipt = {e.purchasedProduct.receipt}");
        sb.AppendLine($"transactionID = {e.purchasedProduct.transactionID}");
        sb.AppendLine($"hasReceipt = {e.purchasedProduct.hasReceipt}");
        sb.Append($"definition.id = {e.purchasedProduct.definition.id}");
        SushiDebug.Log(sb.ToString());

        if (e.purchasedProduct.definition.id == NO_ADS_PRODUCT_ID) {
            NoAdsPurchased = true;
        }
        PurchasingInProgress.instance.Show(false);
        return PurchaseProcessingResult.Complete;
    }

    /// <summary>
    /// Called when a purchase fails.
    /// </summary>
    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason) {
        SushiDebug.Log($"PlatformIapManager.OnPurchaseFailed: {product.definition.id}, Reason: {reason}");
        PurchasingInProgress.instance.Show(false);
        ConfirmPopup.instance.Open("\\구입을 실패했습니다.".Localized() + "\n\n" + reason);
    }

    public void PurchaseNoAds() {
        if (controller != null) {
            PurchasingInProgress.instance.Show(true);
            controller.InitiatePurchase(NO_ADS_PRODUCT_ID);
        } else {
            ConfirmPopup.instance.Open("\\인터넷 연결이 필요합니다.".Localized());
        }
    }
}
