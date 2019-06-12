using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class NoAdsIapButton : MonoBehaviour {
    public void OnPurchaseComplete(Product product) {
        SushiDebug.Log($"OnPurchaseComplete: {product.definition.id}");
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason) {
        SushiDebug.Log($"OnPurchaseFailed: {product.definition.id}, Reason: ${reason}");
    }
}
