using UnityEngine;

public class RestorePurchaseIapButton : MonoBehaviour
{
    void Awake()
    {
        gameObject.SetActive(Application.platform == RuntimePlatform.IPhonePlayer);
    }

    public void RestorePurchase()
    {
        PlatformIapManager.instance.RestorePurchase();
    }
}