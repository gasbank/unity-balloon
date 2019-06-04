﻿using UnityEngine;
using System.Collections;
using UnityEngine.Advertisements;

[DisallowMultipleComponent]
public class PlatformUnityAdsInit : MonoBehaviour {
#if UNITY_ANDROID
    string gameID = "2864446";
#elif UNITY_IOS
    string gameID = "2864445";
#endif
    void Start() {
        SushiDebug.Log("PlatformUnityAdsInit.Start()");
        //Advertisement.debugMode = true;
        Advertisement.Initialize(gameID, false);
    }
}
