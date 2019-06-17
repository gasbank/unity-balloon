using UnityEngine;
using System.Collections;
using UnityEngine.Advertisements;

[DisallowMultipleComponent]
public class PlatformUnityAdsInit : MonoBehaviour {
#if BALLOON_UNITY_ADS
#if UNITY_ANDROID
    string gameID = "3173511";
#elif UNITY_IOS
    string gameID = "3173510";
#endif
    void Start() {
        SushiDebug.Log("PlatformUnityAdsInit.Start()");
        //Advertisement.debugMode = true;
        Advertisement.Initialize(gameID, false);
    }
#endif
}
