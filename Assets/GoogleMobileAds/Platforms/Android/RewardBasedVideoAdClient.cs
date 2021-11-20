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
    public class RewardBasedVideoAdClient : AndroidJavaProxy, IRewardBasedVideoAdClient
    {
        readonly AndroidJavaObject androidRewardBasedVideo;

        public RewardBasedVideoAdClient()
            : base(Utils.UnityRewardBasedVideoAdListenerClassName)
        {
            var playerClass = new AndroidJavaClass(Utils.UnityActivityClassName);
            var activity =
                playerClass.GetStatic<AndroidJavaObject>("currentActivity");
            androidRewardBasedVideo = new AndroidJavaObject(Utils.RewardBasedVideoClassName,
                activity, this);
        }

        public event EventHandler<EventArgs> OnAdLoaded = delegate { };
        public event EventHandler<AdFailedToLoadEventArgs> OnAdFailedToLoad = delegate { };
        public event EventHandler<EventArgs> OnAdOpening = delegate { };
        public event EventHandler<EventArgs> OnAdStarted = delegate { };
        public event EventHandler<EventArgs> OnAdClosed = delegate { };
        public event EventHandler<Reward> OnAdRewarded = delegate { };
        public event EventHandler<EventArgs> OnAdLeavingApplication = delegate { };
        public event EventHandler<EventArgs> OnAdCompleted = delegate { };

        #region IRewardBasedVideoClient implementation

        public void CreateRewardBasedVideoAd()
        {
            androidRewardBasedVideo.Call("create");
        }

        public void LoadAd(AdRequest request, string adUnitId)
        {
            androidRewardBasedVideo.Call("loadAd", Utils.GetAdRequestJavaObject(request), adUnitId);
        }

        public bool IsLoaded()
        {
            return androidRewardBasedVideo.Call<bool>("isLoaded");
        }

        public void ShowRewardBasedVideoAd()
        {
            androidRewardBasedVideo.Call("show");
        }

        public void SetUserId(string userId)
        {
            androidRewardBasedVideo.Call("setUserId", userId);
        }

        public void DestroyRewardBasedVideoAd()
        {
            androidRewardBasedVideo.Call("destroy");
        }

        // Returns the mediation adapter class name.
        public string MediationAdapterClassName()
        {
            return androidRewardBasedVideo.Call<string>("getMediationAdapterClassName");
        }

        #endregion

        #region Callbacks from UnityRewardBasedVideoAdListener.

        void onAdLoaded()
        {
            if (OnAdLoaded != null) OnAdLoaded(this, EventArgs.Empty);
        }

        void onAdFailedToLoad(string errorReason)
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

        void onAdOpened()
        {
            if (OnAdOpening != null) OnAdOpening(this, EventArgs.Empty);
        }

        void onAdStarted()
        {
            if (OnAdStarted != null) OnAdStarted(this, EventArgs.Empty);
        }

        void onAdClosed()
        {
            if (OnAdClosed != null) OnAdClosed(this, EventArgs.Empty);
        }

        void onAdRewarded(string type, float amount)
        {
            if (OnAdRewarded != null)
            {
                var args = new Reward
                {
                    Type = type,
                    Amount = amount
                };
                OnAdRewarded(this, args);
            }
        }

        void onAdLeftApplication()
        {
            if (OnAdLeavingApplication != null) OnAdLeavingApplication(this, EventArgs.Empty);
        }

        void onAdCompleted()
        {
            if (OnAdCompleted != null) OnAdCompleted(this, EventArgs.Empty);
        }

        #endregion
    }
}

#endif