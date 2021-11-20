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
using System.Collections.Generic;
using System.Reflection;
using GoogleMobileAds.Common;

namespace GoogleMobileAds.Api
{
    public enum NativeAdType
    {
        CustomTemplate = 0
    }

    public class AdLoader
    {
        readonly IAdLoaderClient adLoaderClient;

        AdLoader(Builder builder)
        {
            AdUnitId = string.Copy(builder.AdUnitId);
            CustomNativeTemplateClickHandlers =
                new Dictionary<string, Action<CustomNativeTemplateAd, string>>(
                    builder.CustomNativeTemplateClickHandlers);
            TemplateIds = new HashSet<string>(builder.TemplateIds);
            AdTypes = new HashSet<NativeAdType>(builder.AdTypes);

            var googleMobileAdsClientFactory = Type.GetType(
                "GoogleMobileAds.GoogleMobileAdsClientFactory,Assembly-CSharp");
            var method = googleMobileAdsClientFactory.GetMethod(
                "BuildAdLoaderClient",
                BindingFlags.Static | BindingFlags.Public);
            adLoaderClient = (IAdLoaderClient) method.Invoke(null, new object[] {this});

            Utils.CheckInitialization();

            adLoaderClient.OnCustomNativeTemplateAdLoaded +=
                delegate(object sender, CustomNativeEventArgs args) { OnCustomNativeTemplateAdLoaded(this, args); };
            adLoaderClient.OnAdFailedToLoad += delegate(
                object sender, AdFailedToLoadEventArgs args)
            {
                if (OnAdFailedToLoad != null) OnAdFailedToLoad(this, args);
            };
        }

        public Dictionary<string, Action<CustomNativeTemplateAd, string>>
            CustomNativeTemplateClickHandlers { get; }

        public string AdUnitId { get; }

        public HashSet<NativeAdType> AdTypes { get; }

        public HashSet<string> TemplateIds { get; }

        public event EventHandler<AdFailedToLoadEventArgs> OnAdFailedToLoad;

        public event EventHandler<CustomNativeEventArgs> OnCustomNativeTemplateAdLoaded;

        public void LoadAd(AdRequest request)
        {
            adLoaderClient.LoadAd(request);
        }

        public class Builder
        {
            public Builder(string adUnitId)
            {
                AdUnitId = adUnitId;
                AdTypes = new HashSet<NativeAdType>();
                TemplateIds = new HashSet<string>();
                CustomNativeTemplateClickHandlers =
                    new Dictionary<string, Action<CustomNativeTemplateAd, string>>();
            }

            internal string AdUnitId { get; }

            internal HashSet<NativeAdType> AdTypes { get; }

            internal HashSet<string> TemplateIds { get; }

            internal Dictionary<string, Action<CustomNativeTemplateAd, string>>
                CustomNativeTemplateClickHandlers { get; }

            public Builder ForCustomNativeAd(string templateId)
            {
                TemplateIds.Add(templateId);
                AdTypes.Add(NativeAdType.CustomTemplate);
                return this;
            }

            public Builder ForCustomNativeAd(
                string templateId,
                Action<CustomNativeTemplateAd, string> callback)
            {
                TemplateIds.Add(templateId);
                CustomNativeTemplateClickHandlers[templateId] = callback;
                AdTypes.Add(NativeAdType.CustomTemplate);
                return this;
            }

            public AdLoader Build()
            {
                return new AdLoader(this);
            }
        }
    }
}