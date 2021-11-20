// <copyright file="PlayGamesAchievement.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc. All Rights Reserved.
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

using System;
using GooglePlayGames.BasicApi;
using UnityEngine;
using UnityEngine.SocialPlatforms;

#if UNITY_ANDROID

namespace GooglePlayGames
{
#if UNITY_2017_1_OR_NEWER
    using UnityEngine.Networking;
#endif

    internal delegate void ReportProgress(string id, double progress, Action<bool> callback);

    /// <summary>
    ///     Represents a Google Play Games achievement. It can be used to report an achievement
    ///     to the API, offering identical functionality as <see cref="PlayGamesPlatform.ReportProgress" />.
    /// </summary>
    internal class PlayGamesAchievement : IAchievement, IAchievementDescription
    {
        readonly ReportProgress mProgressCallback;
        readonly string mRevealedImageUrl = string.Empty;
        readonly string mUnlockedImageUrl = string.Empty;
#if UNITY_2017_1_OR_NEWER
        UnityWebRequest mImageFetcher;
#else
        private WWW mImageFetcher = null;
#endif
        Texture2D mImage;
        readonly ulong mPoints;

        internal PlayGamesAchievement()
            : this(PlayGamesPlatform.Instance.ReportProgress)
        {
        }

        internal PlayGamesAchievement(ReportProgress progressCallback)
        {
            mProgressCallback = progressCallback;
        }

        internal PlayGamesAchievement(Achievement ach) : this()
        {
            id = ach.Id;
            isIncremental = ach.IsIncremental;
            currentSteps = ach.CurrentSteps;
            totalSteps = ach.TotalSteps;
            if (ach.IsIncremental)
            {
                if (ach.TotalSteps > 0)
                    percentCompleted =
                        ach.CurrentSteps / (double) ach.TotalSteps * 100.0;
                else
                    percentCompleted = 0.0;
            }
            else
            {
                percentCompleted = ach.IsUnlocked ? 100.0 : 0.0;
            }

            completed = ach.IsUnlocked;
            hidden = !ach.IsRevealed;
            lastReportedDate = ach.LastModifiedTime;
            title = ach.Name;
            achievedDescription = ach.Description;
            mPoints = ach.Points;
            mRevealedImageUrl = ach.RevealedImageUrl;
            mUnlockedImageUrl = ach.UnlockedImageUrl;
        }

        /// <summary>
        ///     Reveals, unlocks or increment achievement.
        /// </summary>
        /// <remarks>
        ///     Call after setting <see cref="id" />, <see cref="completed" />,
        ///     as well as <see cref="currentSteps" /> and <see cref="totalSteps" />
        ///     for incremental achievements. Equivalent to calling
        ///     <see cref="PlayGamesPlatform.ReportProgress" />.
        /// </remarks>
        public void ReportProgress(Action<bool> callback)
        {
            mProgressCallback.Invoke(id, percentCompleted, callback);
        }

        /// <summary>
        ///     Loads the local user's image from the url.  Loading urls
        ///     is asynchronous so the return from this call is fast,
        ///     the image is returned once it is loaded.  null is returned
        ///     up to that point.
        /// </summary>
        Texture2D LoadImage()
        {
            if (hidden)
                // return null, we dont have images for hidden achievements.
                return null;

            var url = completed ? mUnlockedImageUrl : mRevealedImageUrl;

            // the url can be null if the image is not configured.
            if (!string.IsNullOrEmpty(url))
            {
                if (mImageFetcher == null || mImageFetcher.url != url)
                {
#if UNITY_2017_1_OR_NEWER
                    mImageFetcher = UnityWebRequestTexture.GetTexture(url);
#else
                    mImageFetcher = new WWW(url);
#endif
                    mImage = null;
                }

                // if we have the texture, just return, this avoids excessive
                // memory usage calling www.texture repeatedly.
                if (mImage != null) return mImage;

                if (mImageFetcher.isDone)
                {
#if UNITY_2017_1_OR_NEWER
                    mImage = DownloadHandlerTexture.GetContent(mImageFetcher);
#else
                    mImage = mImageFetcher.texture;
#endif
                    return mImage;
                }
            }

            // if there is no url, always return null.
            return null;
        }


        /// <summary>
        ///     Gets or sets the id of this achievement.
        /// </summary>
        /// <returns>
        ///     The identifier.
        /// </returns>
        public string id { get; set; } = string.Empty;

        /// <summary>
        ///     Gets a value indicating whether this achievement is incremental.
        /// </summary>
        /// <remarks>
        ///     This value is only set by PlayGamesPlatform.LoadAchievements
        /// </remarks>
        /// <returns><c>true</c> if incremental; otherwise, <c>false</c>.</returns>
        public bool isIncremental { get; }

        /// <summary>
        ///     Gets the current steps completed of this achievement.
        /// </summary>
        /// <remarks>
        ///     Undefined for standard (i.e. non-incremental) achievements.
        ///     This value is only set by PlayGamesPlatform.LoadAchievements, changing the
        ///     percentComplete will not affect this.
        /// </remarks>
        /// <returns>The current steps.</returns>
        public int currentSteps { get; }

        /// <summary>
        ///     Gets the total steps of this achievement.
        /// </summary>
        /// <remarks>
        ///     Undefined for standard (i.e. non-incremental) achievements.
        ///     This value is only set by PlayGamesPlatform.LoadAchievements, changing the
        ///     percentComplete will not affect this.
        /// </remarks>
        /// <returns>The total steps.</returns>
        public int totalSteps { get; }

        /// <summary>
        ///     Gets or sets the percent completed.
        /// </summary>
        /// <returns>
        ///     The percent completed.
        /// </returns>
        public double percentCompleted { get; set; }

        /// <summary>
        ///     Gets a value indicating whether this achievement is completed.
        /// </summary>
        /// <remarks>
        ///     This value is only set by PlayGamesPlatform.LoadAchievements, changing the
        ///     percentComplete will not affect this.
        /// </remarks>
        /// <returns><c>true</c> if completed; otherwise, <c>false</c>.</returns>
        public bool completed { get; }

        /// <summary>
        ///     Gets a value indicating whether this achievement is hidden.
        /// </summary>
        /// <value><c>true</c> if hidden; otherwise, <c>false</c>.</value>
        public bool hidden { get; }

        public DateTime lastReportedDate { get; } = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        public string title { get; } = string.Empty;

        public Texture2D image => LoadImage();

        public string achievedDescription { get; } = string.Empty;

        public string unachievedDescription => achievedDescription;

        public int points => (int) mPoints;
    }
}
#endif