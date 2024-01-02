using System;
using System.Text;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class PlatformIapManager : MonoBehaviour, IDetailedStoreListener
{
    internal static PlatformIapManager instance;
    static readonly string NO_ADS_PRODUCT_ID = "top.plusalpha.balloon.noads";
    static readonly string NO_ADS_PREF_KEY = "NO_ADS_PREF_KEY";
    static readonly string FORCE_ADS_PREF_KEY = "FORCE_ADS_PREF_KEY";
    IStoreController controller;
    IExtensionProvider extensions;
    
    [SerializeField]
    string environment = "production";

    [SerializeField]
    GameObject iapGroup;

    public bool ForceAds
    {
        get => PlayerPrefs.GetInt(FORCE_ADS_PREF_KEY, 0) != 0;
        set
        {
            SetForceAds(value);
            BalloonDebug.Log($"ForceAds set to {value}");
        }
    }

    public bool NoAdsPurchased
    {
        get => ForceAds ? false : PlayerPrefs.GetInt(NO_ADS_PREF_KEY, 0) != 0;
        private set
        {
            SetNoAdsPurchased_Admin(ForceAds ? false : value);
            BalloonDebug.Log($"NoAdsPurchased set to {NoAdsPurchased}");
            SyncNoAdsStates();
        }
    }

    /// <summary>
    ///     Called when Unity IAP is ready to make purchases.
    /// </summary>
    public void OnInitialized(IStoreController inController, IExtensionProvider inExtensions)
    {
        controller = inController;
        extensions = inExtensions;
        var sb = new StringBuilder();
        sb.AppendLine("PlatformIapManager.OnInitialized");
        foreach (var product in inController.products.all)
        {
            sb.AppendLine($" - {product.definition.id}: hasReceipt={product.hasReceipt}");
            if (product.hasReceipt)
                if (product.definition.id == NO_ADS_PRODUCT_ID)
                    NoAdsPurchased = true;
        }

        BalloonDebug.Log(sb.ToString());
    }
    
    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        Debug.LogError($"PlatformIapManager.OnPurchaseFailed: Product={product}, PurchaseFailureDescription={failureDescription}");
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"PlatformIapManager.OnInitializeFailed: {error}");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message) {
        Debug.LogError($"PlatformIapManager.OnInitializeFailed: Error={error}, Message={message}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        var sb = new StringBuilder();
        sb.AppendLine("PlatformIapManager.ProcessPurchase");
        sb.AppendLine($"availableToPurchase = {e.purchasedProduct.availableToPurchase}");
        sb.AppendLine($"hasReceipt = {e.purchasedProduct.hasReceipt}");
        sb.AppendLine($"receipt = {e.purchasedProduct.receipt}");
        sb.AppendLine($"transactionID = {e.purchasedProduct.transactionID}");
        sb.AppendLine($"hasReceipt = {e.purchasedProduct.hasReceipt}");
        sb.Append($"definition.id = {e.purchasedProduct.definition.id}");
        BalloonDebug.Log(sb.ToString());

        if (e.purchasedProduct.definition.id == NO_ADS_PRODUCT_ID) NoAdsPurchased = true;
        PurchasingInProgress.instance.Show(false);
        return PurchaseProcessingResult.Complete;
    }

    /// <summary>
    ///     Called when a purchase fails.
    /// </summary>
    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
    {
        BalloonDebug.Log($"PlatformIapManager.OnPurchaseFailed: {product.definition.id}, Reason: {reason}");
        PurchasingInProgress.instance.Show(false);
        ConfirmPopup.instance.Open("\\구입을 실패했습니다.".Localized() + "\n\n" + reason);
    }

    public static void SetNoAdsPurchased_Admin(bool b)
    {
        PlayerPrefs.SetInt(NO_ADS_PREF_KEY, b ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static void SetForceAds(bool b)
    {
        PlayerPrefs.SetInt(FORCE_ADS_PREF_KEY, b ? 1 : 0);
        PlayerPrefs.Save();
    }

    void SyncNoAdsStates()
    {
        iapGroup.SetActive(NoAdsPurchased == false);
        if (NoAdsPurchased)
            PlatformAdMobAdsInit.instance.HideBanner();
        else
            PlatformAdMobAdsInit.instance.StartShowBanner();
    }

    async void Start()
    {
        try {
            var options = new InitializationOptions()
                .SetEnvironmentName(environment);
 
            await UnityServices.InitializeAsync(options);
        }
        catch (Exception exception) {
            // An error occurred during initialization.
            Debug.LogException(exception);
        }
        
        iapGroup = GameObject.Find("Canvas/IAP Group");

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(NO_ADS_PRODUCT_ID, ProductType.NonConsumable);
        UnityPurchasing.Initialize(this, builder);
        
        // 일단 로컬 저장된 상태만으로 빠르게 판단해서 배너 광고를 보여줘야 하면 보여준다.
        SyncNoAdsStates();
    }

    void Update()
    {
        if (Application.isEditor && Input.GetKeyDown(KeyCode.F8)) NoAdsPurchased = false;
    }

    public void PurchaseNoAds()
    {
        if (controller != null)
        {
            PurchasingInProgress.instance.Show(true);
            controller.InitiatePurchase(NO_ADS_PRODUCT_ID);
        }
        else
        {
            ConfirmPopup.instance.Open("\\인터넷 연결이 필요합니다.".Localized());
        }
    }

    public void RestorePurchase()
    {
#if UNITY_IOS
        if (extensions != null) {
            extensions.GetExtension<IAppleExtensions>().RestoreTransactions(result => {
                if (result) {
                    // This does not mean anything was restored,
                    // merely that the restoration process succeeded.
                } else {
                    // Restoration failed.
                }
            });
        } else {
            ConfirmPopup.instance.Open("\\다시 시도 해 주십시오.".Localized());
        }
#endif
    }
}