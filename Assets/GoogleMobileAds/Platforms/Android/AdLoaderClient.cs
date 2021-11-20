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
using GoogleMobileAds.Api;
#if UNITY_ANDROID
using GoogleMobileAds.Common;
using UnityEngine;

namespace GoogleMobileAds.Android
{
    public class AdLoaderClient : AndroidJavaProxy, IAdLoaderClient
    {
        readonly AndroidJavaObject adLoader;

        public AdLoaderClient(AdLoader unityAdLoader) : base(Utils.UnityAdLoaderListenerClassName)
        {
            var playerClass = new AndroidJavaClass(Utils.UnityActivityClassName);
            var activity =
                playerClass.GetStatic<AndroidJavaObject>("currentActivity");
            adLoader = new AndroidJavaObject(Utils.NativeAdLoaderClassName, activity,
                unityAdLoader.AdUnitId, this);

            CustomNativeTemplateCallbacks = unityAdLoader.CustomNativeTemplateClickHandlers;

            if (unityAdLoader.AdTypes.Contains(NativeAdType.CustomTemplate))
                foreach (var templateId in unityAdLoader.TemplateIds)
                    adLoader.Call("configureCustomNativeTemplateAd", templateId,
                        CustomNativeTemplateCallbacks.ContainsKey(templateId));
            adLoader.Call("create");
        }

        Dictionary<string, Action<CustomNativeTemplateAd, string>> CustomNativeTemplateCallbacks { get; }

        public event EventHandler<AdFailedToLoadEventArgs> OnAdFailedToLoad;
        public event EventHandler<CustomNativeEventArgs> OnCustomNativeTemplateAdLoaded;

        public void LoadAd(AdRequest request)
        {
            adLoader.Call("loadAd", Utils.GetAdRequestJavaObject(request));
        }

        public void onCustomTemplateAdLoaded(AndroidJavaObject ad)
        {
            if (OnCustomNativeTemplateAdLoaded != null)
            {
                var args = new CustomNativeEventArgs
                {
                    nativeAd = new CustomNativeTemplateAd(new CustomNativeTemplateClient(ad))
                };
                OnCustomNativeTemplateAdLoaded(this, args);
            }
        }

        void onAdFailedToLoad(string errorReason)
        {
            var args = new AdFailedToLoadEventArgs
            {
                Message = errorReason
            };
            OnAdFailedToLoad(this, args);
        }

        public void onCustomClick(AndroidJavaObject ad, string assetName)
        {
            var nativeAd = new CustomNativeTemplateAd(
                new CustomNativeTemplateClient(ad));
            CustomNativeTemplateCallbacks[nativeAd.GetCustomTemplateId()](nativeAd, assetName);
        }
    }
}

#endif