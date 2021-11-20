// <copyright file="VideoCapabilities.cs" company="Google Inc.">
// Copyright (C) 2016 Google Inc.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//    limitations under the License.
// </copyright>

using System.Linq;
using GooglePlayGames.OurUtils;

namespace GooglePlayGames.BasicApi.Video
{
    /// <summary>
    ///     Represents the video recording capabilities.
    /// </summary>
    public class VideoCapabilities
    {
        readonly bool[] mCaptureModesSupported;
        readonly bool[] mQualityLevelsSupported;

        internal VideoCapabilities(bool isCameraSupported, bool isMicSupported, bool isWriteStorageSupported,
            bool[] captureModesSupported, bool[] qualityLevelsSupported)
        {
            IsCameraSupported = isCameraSupported;
            IsMicSupported = isMicSupported;
            IsWriteStorageSupported = isWriteStorageSupported;
            mCaptureModesSupported = captureModesSupported;
            mQualityLevelsSupported = qualityLevelsSupported;
        }

        /// <summary>Returns whether the device has a front-facing camera and we can use it.</summary>
        public bool IsCameraSupported { get; }

        /// <summary>Returns whether the device has a microphone and we can use it.</summary>
        public bool IsMicSupported { get; }

        /// <summary>Returns whether the device has an external storage device and we can use it.</summary>
        public bool IsWriteStorageSupported { get; }

        /// <summary>Returns whether the device supports the given capture mode.</summary>
        public bool SupportsCaptureMode(VideoCaptureMode captureMode)
        {
            if (captureMode != VideoCaptureMode.Unknown) return mCaptureModesSupported[(int) captureMode];

            Logger.w("SupportsCaptureMode called with an unknown captureMode.");
            return false;
        }

        /// <summary>Returns whether the device supports the given quality level.</summary>
        public bool SupportsQualityLevel(VideoQualityLevel qualityLevel)
        {
            if (qualityLevel != VideoQualityLevel.Unknown)
            {
                return mQualityLevelsSupported[(int) qualityLevel];
            }

            Logger.w("SupportsCaptureMode called with an unknown qualityLevel.");
            return false;
        }

        public override string ToString()
        {
            return string.Format(
                "[VideoCapabilities: mIsCameraSupported={0}, mIsMicSupported={1}, mIsWriteStorageSupported={2}, " +
                "mCaptureModesSupported={3}, mQualityLevelsSupported={4}]",
                IsCameraSupported,
                IsMicSupported,
                IsWriteStorageSupported,
                string.Join(",", mCaptureModesSupported.Select(p => p.ToString()).ToArray()),
                string.Join(",", mQualityLevelsSupported.Select(p => p.ToString()).ToArray()));
        }
    }
}