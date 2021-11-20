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
    public class BannerClient : AndroidJavaProxy, IBannerClient
    {
        readonly AndroidJavaObject bannerView;

        public BannerClient() : base(Utils.UnityAdListenerClassName)
        {
            var playerClass = new AndroidJavaClass(Utils.UnityActivityClassName);
            var activity =
                playerClass.GetStatic<AndroidJavaObject>("currentActivity");
            bannerView = new AndroidJavaObject(
                Utils.BannerViewClassName, activity, this);
        }

        public event EventHandler<EventArgs> OnAdLoaded;

        public event EventHandler<AdFailedToLoadEventArgs> OnAdFailedToLoad;

        public event EventHandler<EventArgs> OnAdOpening;

        public event EventHandler<EventArgs> OnAdClosed;

        public event EventHandler<EventArgs> OnAdLeavingApplication;

        // Creates a banner view.
        public void CreateBannerView(string adUnitId, AdSize adSize, AdPosition position)
        {
            bannerView.Call(
                "create", adUnitId, Utils.GetAdSizeJavaObject(adSize), (int) position);
        }

        // Creates a banner view with a custom position.
        public void CreateBannerView(string adUnitId, AdSize adSize, int x, int y)
        {
            bannerView.Call(
                "create", adUnitId, Utils.GetAdSizeJavaObject(adSize), x, y);
        }

        // Loads an ad.
        public void LoadAd(AdRequest request)
        {
            bannerView.Call("loadAd", Utils.GetAdRequestJavaObject(request));
        }

        // Displays the banner view on the screen.
        public void ShowBannerView()
        {
            bannerView.Call("show");
        }

        // Hides the banner view from the screen.
        public void HideBannerView()
        {
            bannerView.Call("hide");
        }

        // Destroys the banner view.
        public void DestroyBannerView()
        {
            bannerView.Call("destroy");
        }

        // Returns the height of the BannerView in pixels.
        public float GetHeightInPixels()
        {
            return bannerView.Call<float>("getHeightInPixels");
        }

        // Returns the width of the BannerView in pixels.
        public float GetWidthInPixels()
        {
            return bannerView.Call<float>("getWidthInPixels");
        }

        // Set the position of the banner view using standard position.
        public void SetPosition(AdPosition adPosition)
        {
            bannerView.Call("setPosition", (int) adPosition);
        }

        // Set the position of the banner view using custom position.
        public void SetPosition(int x, int y)
        {
            bannerView.Call("setPosition", x, y);
        }

        // Returns the mediation adapter class name.
        public string MediationAdapterClassName()
        {
            return bannerView.Call<string>("getMediationAdapterClassName");
        }

        #region Callbacks from UnityBannerAdListener.

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