using UnityEngine;
using UnityEngine.Purchasing;

[DisallowMultipleComponent]
public class PlatformShop : MonoBehaviour
{
    public void OnPurchaseComplete(Product product)
    {
        BalloonDebug.LogFormat("OnPurchaseComplete: {0}", product);
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason p)
    {
        BalloonDebug.LogFormat("OnPurchaseFailed: {0} / Reason: {1}", product, p);
    }
}