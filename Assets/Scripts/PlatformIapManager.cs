using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Purchasing;

public class PlatformIapManager : MonoBehaviour, IStoreListener {
    static readonly string NO_ADS_PRODUCT_ID = "top.plusalpha.balloon.noads";
    static readonly string NO_ADS_PREF_KEY = "NO_ADS_PREF_KEY";
    private IStoreController controller;
    private IExtensionProvider extensions;
    internal static PlatformIapManager instance;

    public static bool NoAdsPurchased => PlayerPrefs.GetInt(NO_ADS_PREF_KEY, 0) != 0;

    void Awake() {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(NO_ADS_PRODUCT_ID, ProductType.NonConsumable);
        UnityPurchasing.Initialize(this, builder);
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
                // var receipt = MiniJson.JsonDecode(product.receipt) as Hashtable;
                // foreach (var key in receipt.Keys) {
                //     var value = receipt[key];
                //     sb.AppendLine($" --- {key} = {value}");
                // }

                if (product.definition.id == NO_ADS_PRODUCT_ID) {
                    PlayerPrefs.SetInt(NO_ADS_PREF_KEY, 1);
                    PlayerPrefs.Save();
                }
            }
        }
        SushiDebug.Log(sb.ToString());
        if (NoAdsPurchased == false) {
            PlatformAdMobAdsInit.instance.StartShowBanner();
        }
    }

    /// <summary>
    /// Called when Unity IAP encounters an unrecoverable initialization error.
    ///
    /// Note that this will not be called if Internet is unavailable; Unity IAP
    /// will attempt initialization until it becomes available.
    /// </summary>
    public void OnInitializeFailed(InitializationFailureReason error) {
        Debug.LogError($"PlatformIapManager.OnInitializeFailed: {error}");
        if (NoAdsPurchased == false) {
            PlatformAdMobAdsInit.instance.StartShowBanner();
        }
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
            PlayerPrefs.SetInt(NO_ADS_PREF_KEY, 1);
            PlayerPrefs.Save();
        }

        if (NoAdsPurchased) {
            PlatformAdMobAdsInit.instance.HideBanner();
        }

        return PurchaseProcessingResult.Complete;
    }

    /// <summary>
    /// Called when a purchase fails.
    /// </summary>
    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason) {
        SushiDebug.Log($"PlatformIapManager.OnPurchaseFailed: {product.definition.id}, Reason: {reason}");
    }

    public void PurchaseNoAds() {
        controller.InitiatePurchase(NO_ADS_PRODUCT_ID);
    }
}