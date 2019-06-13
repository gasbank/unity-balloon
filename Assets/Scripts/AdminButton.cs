using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMProText = TMPro.TextMeshProUGUI;

public class AdminButton : MonoBehaviour {
    public void SetForceAds(bool b) {
        PlatformIapManager.SetForceAds(b);
    }
}
