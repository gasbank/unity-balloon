using UnityEngine;

[DisallowMultipleComponent]
public class ShopProductEntry : MonoBehaviour
{
    //[SerializeField] private Image productImage = null;
    //[SerializeField] private Text productDesc = null;
    //[SerializeField] private Text productPrice = null;
    //[SerializeField] private Text nonpurchaseButtonText = null;
    //[SerializeField] private Text purchaseButtonText = null;
    //[SerializeField] private IAPButton iapButton = null;
    //[SerializeField] private GameObject purchaseGameObject = null;
    //[SerializeField] private GameObject nonpurchaseGameObject = null;
    //[SerializeField] private Image priceImage = null;
    //[SerializeField] private GameObject lightRaysBg = null;
    //[SerializeField] private Toggle nonpurchaseToggle = null;

    // **********************
    // 주의: 데이터시트에도 쓰이는 값이므로, 변경하게 되면 데이터시트 리로드 해야 한다.
    // **********************
    public enum ShopProductType
    {
        inAppPurchaseBegin, // 인앱 결제상품 시작
        top_plusalpha_balloon_ads = inAppPurchaseBegin,
        inAppPurchaseEnd // 인앱 결제상품 끝
    }

    //private ShopProductData shopProductData = null;
}