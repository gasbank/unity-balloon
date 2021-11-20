// Copyright (C) 2018 Google, Inc.
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
    public class RewardedAdClient : AndroidJavaProxy, IRewardedAdClient
    {
        readonly AndroidJavaObject androidRewardedAd;

        public RewardedAdClient() : base(Utils.UnityRewardedAdCallbackClassName)
        {
            var playerClass = new AndroidJavaClass(Utils.UnityActivityClassName);
            var activity =
                playerClass.GetStatic<AndroidJavaObject>("currentActivity");
            androidRewardedAd = new AndroidJavaObject(Utils.UnityRewardedAdClassName, activity, this);
        }

        #region IRewardedClient implementation

        public event EventHandler<EventArgs> OnAdLoaded;

        public event EventHandler<AdErrorEventArgs> OnAdFailedToLoad;

        public event EventHandler<AdErrorEventArgs> OnAdFailedToShow;

        public event EventHandler<EventArgs> OnAdOpening;

        public event EventHandler<Reward> OnUserEarnedReward;

        public event EventHandler<EventArgs> OnAdClosed;

        public void CreateRewardedAd(string adUnitId)
        {
            androidRewardedAd.Call("create", adUnitId);
        }

        public void LoadAd(AdRequest request)
        {
            androidRewardedAd.Call("loadAd", Utils.GetAdRequestJavaObject(request));
        }

        public bool IsLoaded()
        {
            return androidRewardedAd.Call<bool>("isLoaded");
        }

        public void Show()
        {
            androidRewardedAd.Call("show");
        }

        public void SetServerSideVerificationOptions(ServerSideVerificationOptions serverSideVerificationOptions)
        {
            androidRewardedAd.Call("setServerSideVerificationOptions",
                Utils.GetServerSideVerificationOptionsJavaObject(serverSideVerificationOptions));
        }

        public void DestroyRewardBasedVideoAd()
        {
            androidRewardedAd.Call("destroy");
        }

        // Returns the mediation adapter class name.
        public string MediationAdapterClassName()
        {
            return androidRewardedAd.Call<string>("getMediationAdapterClassName");
        }

        #endregion

        #region Callbacks from UnityRewardBasedVideoAdListener.

        void onRewardedAdLoaded()
        {
            if (OnAdLoaded != null) OnAdLoaded(this, EventArgs.Empty);
        }

        void onRewardedAdFailedToLoad(string errorReason)
        {
            if (OnAdFailedToLoad != null)
            {
                var args = new AdErrorEventArgs
                {
                    Message = errorReason
                };
                OnAdFailedToLoad(this, args);
            }
        }

        void onRewardedAdFailedToShow(string errorReason)
        {
            if (OnAdFailedToLoad != null)
            {
                var args = new AdErrorEventArgs
                {
                    Message = errorReason
                };
                OnAdFailedToShow(this, args);
            }
        }

        void onRewardedAdOpened()
        {
            if (OnAdOpening != null) OnAdOpening(this, EventArgs.Empty);
        }


        void onRewardedAdClosed()
        {
            if (OnAdClosed != null) OnAdClosed(this, EventArgs.Empty);
        }

        void onUserEarnedReward(string type, float amount)
        {
            if (OnUserEarnedReward != null)
            {
                var args = new Reward
                {
                    Type = type,
                    Amount = amount
                };
                OnUserEarnedReward(this, args);
            }
        }

        #endregion
    }
}

#endif