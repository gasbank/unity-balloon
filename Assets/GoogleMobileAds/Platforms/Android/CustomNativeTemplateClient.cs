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


using System.Collections.Generic;
#if UNITY_ANDROID
using GoogleMobileAds.Common;
using UnityEngine;

namespace GoogleMobileAds.Android
{
    internal class CustomNativeTemplateClient : ICustomNativeTemplateClient
    {
        readonly AndroidJavaObject customNativeAd;

        public CustomNativeTemplateClient(AndroidJavaObject customNativeAd)
        {
            this.customNativeAd = customNativeAd;
        }

        public List<string> GetAvailableAssetNames()
        {
            return new List<string>(customNativeAd.Call<string[]>("getAvailableAssetNames"));
        }

        public string GetTemplateId()
        {
            return customNativeAd.Call<string>("getTemplateId");
        }

        public byte[] GetImageByteArray(string key)
        {
            var imageAssetAsByteArray = customNativeAd.Call<byte[]>("getImage", key);
            if (imageAssetAsByteArray.Length == 0) return null;

            return imageAssetAsByteArray;
        }

        public string GetText(string key)
        {
            var assetText = customNativeAd.Call<string>("getText", key);
            if (assetText.Equals(string.Empty)) return null;

            return assetText;
        }

        public void PerformClick(string assetName)
        {
            customNativeAd.Call("performClick", assetName);
        }

        public void RecordImpression()
        {
            customNativeAd.Call("recordImpression");
        }
    }
}

#endif