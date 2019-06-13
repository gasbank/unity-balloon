using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMProText = TMPro.TextMeshProUGUI;

public class AdminButton : MonoBehaviour {
    public void SetNoAdsPurchased(bool b) {
        PlatformIapManager.SetNoAdsPurchased_Admin(b);
    }
}
