// <copyright file="PlayGamesUserProfile.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc.  All Rights Reserved.
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
using System.Collections;
using GooglePlayGames.OurUtils;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Logger = GooglePlayGames.OurUtils.Logger;

#if UNITY_ANDROID

namespace GooglePlayGames
{
#if UNITY_2017_2_OR_NEWER
    using UnityEngine.Networking;
#endif

    /// <summary>
    ///     Represents a Google Play Games user profile. In the current implementation,
    ///     this is only used as a base class of <see cref="PlayGamesLocalUser" />
    ///     and should not be used directly.
    /// </summary>
    public class PlayGamesUserProfile : IUserProfile
    {
        Texture2D mImage;

        volatile bool mImageLoading;

        internal PlayGamesUserProfile(string displayName, string playerId,
            string avatarUrl)
        {
            userName = displayName;
            id = playerId;
            setAvatarUrl(avatarUrl);
            mImageLoading = false;
            isFriend = false;
        }

        internal PlayGamesUserProfile(string displayName, string playerId, string avatarUrl,
            bool isFriend)
        {
            userName = displayName;
            id = playerId;
            AvatarURL = avatarUrl;
            mImageLoading = false;
            this.isFriend = isFriend;
        }

        public string AvatarURL { get; set; }

        protected void ResetIdentity(string displayName, string playerId,
            string avatarUrl)
        {
            userName = displayName;
            id = playerId;
            isFriend = false;
            if (AvatarURL != avatarUrl)
            {
                mImage = null;
                setAvatarUrl(avatarUrl);
            }

            mImageLoading = false;
        }

        /// <summary>
        ///     Loads the local user's image from the url.  Loading urls
        ///     is asynchronous so the return from this call is fast,
        ///     the image is returned once it is loaded.  null is returned
        ///     up to that point.
        /// </summary>
        internal IEnumerator LoadImage()
        {
            // the url can be null if the user does not have an
            // avatar configured.
            if (!string.IsNullOrEmpty(AvatarURL))
            {
#if UNITY_2017_2_OR_NEWER
                var www = UnityWebRequestTexture.GetTexture(AvatarURL);
                www.SendWebRequest();
#else
                WWW www = new WWW(AvatarURL);
#endif
                while (!www.isDone) yield return null;

                if (www.error == null)
                {
#if UNITY_2017_2_OR_NEWER
                    mImage = DownloadHandlerTexture.GetContent(www);
#else
                    this.mImage = www.texture;
#endif
                }
                else
                {
                    mImage = Texture2D.blackTexture;
                    Logger.e("Error downloading image: " + www.error);
                }

                mImageLoading = false;
            }
            else
            {
                Logger.e("No URL found.");
                mImage = Texture2D.blackTexture;
                mImageLoading = false;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (ReferenceEquals(this, obj)) return true;

            var other = obj as PlayGamesUserProfile;
            if (other == null) return false;

            return StringComparer.Ordinal.Equals(id, other.id);
        }

        public override int GetHashCode()
        {
            return typeof(PlayGamesUserProfile).GetHashCode() ^ id.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("[Player: '{0}' (id {1})]", userName, id);
        }

        void setAvatarUrl(string avatarUrl)
        {
            AvatarURL = avatarUrl;
            if (!avatarUrl.StartsWith("https") && avatarUrl.StartsWith("http")) AvatarURL = avatarUrl.Insert(4, "s");
        }

        #region IUserProfile implementation

        public string userName { get; set; }

        public string id { get; set; }

        public string gameId => id;

        public bool isFriend { get; set; }

        public UserState state => UserState.Online;

        public Texture2D image
        {
            get
            {
                if (!mImageLoading && mImage == null && !string.IsNullOrEmpty(AvatarURL))
                {
                    Logger.d("Starting to load image: " + AvatarURL);
                    mImageLoading = true;
                    PlayGamesHelperObject.RunCoroutine(LoadImage());
                }

                return mImage;
            }
        }

        #endregion
    }
}
#endif