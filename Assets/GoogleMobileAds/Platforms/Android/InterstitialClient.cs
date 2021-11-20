// Copyright (C) 2015 Google, Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.


using System;
using GoogleMobileAds.Api;
#if UNITY_ANDROID
using GoogleMobileAds.Common;
using UnityEngine;

namespace GoogleMobileAds.Android
{
    public class InterstitialClient : AndroidJavaProxy, IInterstitialClient
    {
        readonly AndroidJavaObject interstitial;

        public InterstitialClient() : base(Utils.UnityAdListenerClassName)
        {
            var playerClass = new AndroidJavaClass(Utils.UnityActivityClassName);
            var activity =
                playerClass.GetStatic<AndroidJavaObject>("currentActivity");
            interstitial = new AndroidJavaObject(
                Utils.InterstitialClassName, activity, this);
        }

        public event EventHandler<EventArgs> OnAdLoaded;

        public event EventHandler<AdFailedToLoadEventArgs> OnAdFailedToLoad;

        public event EventHandler<EventArgs> OnAdOpening;

        public event EventHandler<EventArgs> OnAdClosed;

        public event EventHandler<EventArgs> OnAdLeavingApplication;

        #region IGoogleMobileAdsInterstitialClient implementation

        // Creates an interstitial ad.
        public void CreateInterstitialAd(string adUnitId)
        {
            interstitial.Call("create", adUnitId);
        }

        // Loads an ad.
        public void LoadAd(AdRequest request)
        {
            interstitial.Call("loadAd", Utils.GetAdRequestJavaObject(request));
        }

        // Checks if interstitial has loaded.
        public bool IsLoaded()
        {
            return interstitial.Call<bool>("isLoaded");
        }

        // Presents the interstitial ad on the screen.
        public void ShowInterstitial()
        {
            interstitial.Call("show");
        }

        // Destroys the interstitial ad.
        public void DestroyInterstitial()
        {
            interstitial.Call("destroy");
        }

        // Returns the mediation adapter class name.
        public string MediationAdapterClassName()
        {
            return interstitial.Call<string>("getMediationAdapterClassName");
        }

        #endregion

        #region Callbacks from UnityInterstitialAdListener.

        public void onAdLoaded()
        {
            if (OnAdLoaded != null) OnAdLoaded(this, EventArgs.Empty);
        }

        public void onAdFailedToLoad(string errorReason)
        {
            if (OnAdFailedToLoad != null)
            {
                var args = new AdFailedToLoadEventArgs
                {
                    Message = errorReason
                };
                OnAdFailedToLoad(this, args);
            }
        }

        public void onAdOpened()
        {
            if (OnAdOpening != null) OnAdOpening(this, EventArgs.Empty);
        }

        public void onAdClosed()
        {
            if (OnAdClosed != null) OnAdClosed(this, EventArgs.Empty);
        }

        public void onAdLeftApplication()
        {
            if (OnAdLeavingApplication != null) OnAdLeavingApplication(this, EventArgs.Empty);
        }

        #endregion
    }
}

#endif