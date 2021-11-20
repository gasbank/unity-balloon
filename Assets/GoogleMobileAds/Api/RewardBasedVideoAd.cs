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
using System.Reflection;
using GoogleMobileAds.Common;

namespace GoogleMobileAds.Api
{
    public class RewardBasedVideoAd
    {
        readonly IRewardBasedVideoAdClient client;

        // Creates a Singleton RewardBasedVideoAd.
        RewardBasedVideoAd()
        {
            var googleMobileAdsClientFactory = Type.GetType(
                "GoogleMobileAds.GoogleMobileAdsClientFactory,Assembly-CSharp");
            var method = googleMobileAdsClientFactory.GetMethod(
                "BuildRewardBasedVideoAdClient",
                BindingFlags.Static | BindingFlags.Public);
            client = (IRewardBasedVideoAdClient) method.Invoke(null, null);
            client.CreateRewardBasedVideoAd();

            client.OnAdLoaded += (sender, args) =>
            {
                if (OnAdLoaded != null) OnAdLoaded(this, args);
            };

            client.OnAdFailedToLoad += (sender, args) =>
            {
                if (OnAdFailedToLoad != null) OnAdFailedToLoad(this, args);
            };

            client.OnAdOpening += (sender, args) =>
            {
                if (OnAdOpening != null) OnAdOpening(this, args);
            };

            client.OnAdStarted += (sender, args) =>
            {
                if (OnAdStarted != null) OnAdStarted(this, args);
            };

            client.OnAdClosed += (sender, args) =>
            {
                if (OnAdClosed != null) OnAdClosed(this, args);
            };

            client.OnAdLeavingApplication += (sender, args) =>
            {
                if (OnAdLeavingApplication != null) OnAdLeavingApplication(this, args);
            };

            client.OnAdRewarded += (sender, args) =>
            {
                if (OnAdRewarded != null) OnAdRewarded(this, args);
            };

            client.OnAdCompleted += (sender, args) =>
            {
                if (OnAdCompleted != null) OnAdCompleted(this, args);
            };
        }

        public static RewardBasedVideoAd Instance { get; } = new RewardBasedVideoAd();

        // These are the ad callback events that can be hooked into.
        public event EventHandler<EventArgs> OnAdLoaded;

        public event EventHandler<AdFailedToLoadEventArgs> OnAdFailedToLoad;

        public event EventHandler<EventArgs> OnAdOpening;

        public event EventHandler<EventArgs> OnAdStarted;

        public event EventHandler<EventArgs> OnAdClosed;

        public event EventHandler<Reward> OnAdRewarded;

        public event EventHandler<EventArgs> OnAdLeavingApplication;

        public event EventHandler<EventArgs> OnAdCompleted;

        // Loads a new reward based video ad request
        public void LoadAd(AdRequest request, string adUnitId)
        {
            client.LoadAd(request, adUnitId);
        }

        // Determines whether the reward based video has loaded.
        public bool IsLoaded()
        {
            return client.IsLoaded();
        }

        // Shows the reward based video.
        public void Show()
        {
            client.ShowRewardBasedVideoAd();
        }

        // Sets the user id of current user.
        public void SetUserId(string userId)
        {
            client.SetUserId(userId);
        }

        // Returns the mediation adapter class name.
        public string MediationAdapterClassName()
        {
            return client.MediationAdapterClassName();
        }
    }
}