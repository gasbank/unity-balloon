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
using System.Reflection;
using GoogleMobileAds.Common;

namespace GoogleMobileAds.Api
{
    public class RewardedAd
    {
        readonly IRewardedAdClient client;

        public RewardedAd(string adUnitId)
        {
            // GoogleMobileAdsClientFactory is not included in the compiled DLL due to
            // needing platform directives, so reflection is needed to call this method.
            var googleMobileAdsClientFactory = Type.GetType(
                "GoogleMobileAds.GoogleMobileAdsClientFactory,Assembly-CSharp");
            var method = googleMobileAdsClientFactory.GetMethod(
                "BuildRewardedAdClient",
                BindingFlags.Static | BindingFlags.Public);
            client = (IRewardedAdClient) method.Invoke(null, null);
            client.CreateRewardedAd(adUnitId);

            client.OnAdLoaded += (sender, args) =>
            {
                if (OnAdLoaded != null) OnAdLoaded(this, args);
            };

            client.OnAdFailedToLoad += (sender, args) =>
            {
                if (OnAdFailedToLoad != null) OnAdFailedToLoad(this, args);
            };

            client.OnAdFailedToShow += (sender, args) =>
            {
                if (OnAdFailedToShow != null) OnAdFailedToShow(this, args);
            };

            client.OnAdOpening += (sender, args) =>
            {
                if (OnAdOpening != null) OnAdOpening(this, args);
            };

            client.OnAdClosed += (sender, args) =>
            {
                if (OnAdClosed != null) OnAdClosed(this, args);
            };

            client.OnUserEarnedReward += (sender, args) =>
            {
                if (OnUserEarnedReward != null) OnUserEarnedReward(this, args);
            };
        }

        // These are the ad callback events that can be hooked into.
        public event EventHandler<EventArgs> OnAdLoaded;

        public event EventHandler<AdErrorEventArgs> OnAdFailedToLoad;

        public event EventHandler<AdErrorEventArgs> OnAdFailedToShow;

        public event EventHandler<EventArgs> OnAdOpening;

        public event EventHandler<EventArgs> OnAdClosed;

        public event EventHandler<Reward> OnUserEarnedReward;

        // Loads a new rewarded ad.
        public void LoadAd(AdRequest request)
        {
            client.LoadAd(request);
        }

        // Determines whether the rewarded ad has loaded.
        public bool IsLoaded()
        {
            return client.IsLoaded();
        }

        // Shows the rewarded ad.
        public void Show()
        {
            client.Show();
        }

        // Sets the server side verification options
        public void SetServerSideVerificationOptions(ServerSideVerificationOptions serverSideVerificationOptions)
        {
            client.SetServerSideVerificationOptions(serverSideVerificationOptions);
        }

        // Returns the mediation adapter class name.
        public string MediationAdapterClassName()
        {
            return client.MediationAdapterClassName();
        }
    }
}