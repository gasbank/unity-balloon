using UnityEngine;
using UnityEngine.Purchasing;

[DisallowMultipleComponent]
public class PlatformShop : MonoBehaviour
{
    public void OnPurchaseComplete(Product product)
    {
        SushiDebug.LogFormat("OnPurchaseComplete: {0}", product);
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason p)
    {
        SushiDebug.LogFormat("OnPurchaseFailed: {0} / Reason: {1}", product, p);
    }
}