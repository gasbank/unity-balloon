// <copyright file="PlayGamesClientConfiguration.cs" company="Google Inc.">
// Copyright (C) 2014 Google Inc.
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

using System.Collections.Generic;
using System.Linq;

#if UNITY_ANDROID

namespace GooglePlayGames.BasicApi
{
    /// <summary>
    ///     Provides configuration for <see cref="PlayGamesPlatform" />. If you wish to use either Saved
    ///     Games or Cloud Save, you must create an instance of this class with those features enabled.
    ///     Note that Cloud Save is deprecated, and is not available for new games. You should strongly
    ///     favor Saved Game.
    /// </summary>
    public struct PlayGamesClientConfiguration
    {
        /// <summary>
        ///     The default configuration.
        /// </summary>
        public static readonly PlayGamesClientConfiguration DefaultConfiguration =
            new Builder()
                .Build();

        /// <summary>
        ///     Flag indicating to enable saved games API.
        /// </summary>
        readonly bool mEnableSavedGames;

        /// <summary>
        ///     Array of scopes to be requested from user. None is considered as 'games_lite'.
        /// </summary>
        readonly string[] mScopes;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GooglePlayGames.BasicApi.PlayGamesClientConfiguration" /> struct.
        /// </summary>
        /// <param name="builder">Builder for this configuration.</param>
        PlayGamesClientConfiguration(Builder builder)
        {
            mEnableSavedGames = builder.HasEnableSaveGames();
            mScopes = builder.getScopes();
            IsHidingPopups = builder.IsHidingPopups();
            IsRequestingAuthCode = builder.IsRequestingAuthCode();
            IsForcingRefresh = builder.IsForcingRefresh();
            IsRequestingEmail = builder.IsRequestingEmail();
            IsRequestingIdToken = builder.IsRequestingIdToken();
            AccountName = builder.GetAccountName();
        }

        /// <summary>
        ///     Gets a value indicating whether this <see cref="GooglePlayGames.BasicApi.PlayGamesClientConfiguration" />
        ///     enable saved games.
        /// </summary>
        /// <value><c>true</c> if enable saved games; otherwise, <c>false</c>.</value>
        public bool EnableSavedGames => mEnableSavedGames;

        /// <summary>
        ///     The flag indicating popup UIs should be hidden.
        /// </summary>
        public bool IsHidingPopups { get; }

        /// <summary>
        ///     The flag to indicate a server auth code should be requested when authenticating.
        /// </summary>
        public bool IsRequestingAuthCode { get; }

        /// <summary>
        ///     The flag indicating the auth code should be refresh, causing re-consent and issuing a new refresh token.
        /// </summary>
        public bool IsForcingRefresh { get; }

        /// <summary>
        ///     The flag indicating the email address should returned when authenticating.
        /// </summary>
        public bool IsRequestingEmail { get; }

        /// <summary>
        ///     The flag indicating the id token should be returned when authenticating.
        /// </summary>
        public bool IsRequestingIdToken { get; }

        /// <summary>
        ///     The account name to attempt to use when signing in.  Null indicates use the default.
        /// </summary>
        public string AccountName { get; }

        /// <summary>
        ///     Gets a array of scopes to be requested from the user.
        /// </summary>
        /// <value>String array of scopes.</value>
        public string[] Scopes => mScopes;

        public static bool operator ==(PlayGamesClientConfiguration c1, PlayGamesClientConfiguration c2)
        {
            if (c1.EnableSavedGames != c2.EnableSavedGames ||
                c1.IsForcingRefresh != c2.IsForcingRefresh ||
                c1.IsHidingPopups != c2.IsHidingPopups ||
                c1.IsRequestingEmail != c2.IsRequestingEmail ||
                c1.IsRequestingAuthCode != c2.IsRequestingAuthCode ||
                !c1.Scopes.SequenceEqual(c2.Scopes) ||
                c1.AccountName != c2.AccountName)
                return false;

            return true;
        }

        public static bool operator !=(PlayGamesClientConfiguration c1, PlayGamesClientConfiguration c2)
        {
            return !(c1 == c2);
        }

        /// <summary>
        ///     Builder class for the configuration.
        /// </summary>
        public class Builder
        {
            /// <summary>
            ///     The account name to use as a default when authenticating.
            /// </summary>
            /// <remarks>
            ///     This is only used when requesting auth code or id token.
            /// </remarks>
            string mAccountName;

            /// <summary>
            ///     The flag to enable save games. Default is false.
            /// </summary>
            bool mEnableSaveGames;

            /// <summary>
            ///     The flag indicating the auth code should be refresh, causing re-consent and issuing a new refresh token.
            /// </summary>
            bool mForceRefresh;

            /// <summary>
            ///     The flag indicating that popup UI should be hidden.
            /// </summary>
            bool mHidePopups;

            /// <summary>
            ///     The flag to indicate a server auth code should be requested when authenticating.
            /// </summary>
            bool mRequestAuthCode;

            /// <summary>
            ///     The flag indicating the email address should returned when authenticating.
            /// </summary>
            bool mRequestEmail;

            /// <summary>
            ///     The flag indicating the id token should be returned when authenticating.
            /// </summary>
            bool mRequestIdToken;

            /// <summary>
            ///     The scopes to request from the user. Default is none.
            /// </summary>
            List<string> mScopes;

            /// <summary>
            ///     Enables the saved games.
            /// </summary>
            /// <returns>The builder.</returns>
            public Builder EnableSavedGames()
            {
                mEnableSaveGames = true;
                return this;
            }

            /// <summary>
            ///     Enables hiding popups.  This is recommended for VR apps.
            /// </summary>
            /// <returns>The hide popups.</returns>
            public Builder EnableHidePopups()
            {
                mHidePopups = true;
                return this;
            }

            public Builder RequestServerAuthCode(bool forceRefresh)
            {
                mRequestAuthCode = true;
                mForceRefresh = forceRefresh;
                return this;
            }

            public Builder RequestEmail()
            {
                mRequestEmail = true;
                return this;
            }

            public Builder RequestIdToken()
            {
                mRequestIdToken = true;
                return this;
            }

            public Builder SetAccountName(string accountName)
            {
                mAccountName = accountName;
                return this;
            }

            /// <summary>
            ///     Requests an Oauth scope from the user.
            /// </summary>
            /// <remarks>
            ///     Not setting one will default to 'games_lite' and will not show a consent
            ///     dialog to the user. Valid examples are 'profile' and 'email'.
            ///     Full list: https://developers.google.com/identity/protocols/googlescopes
            ///     To exchange the auth code with an id_token (or user id) on your server,
            ///     you must add at least one scope.
            /// </remarks>
            /// <returns>The builder.</returns>
            public Builder AddOauthScope(string scope)
            {
                if (mScopes == null) mScopes = new List<string>();
                mScopes.Add(scope);
                return this;
            }

            /// <summary>
            ///     Build this instance.
            /// </summary>
            /// <returns>the client configuration instance</returns>
            public PlayGamesClientConfiguration Build()
            {
                return new PlayGamesClientConfiguration(this);
            }

            /// <summary>
            ///     Determines whether this instance has enable save games.
            /// </summary>
            /// <returns><c>true</c> if this instance has enable save games; otherwise, <c>false</c>.</returns>
            internal bool HasEnableSaveGames()
            {
                return mEnableSaveGames;
            }

            internal bool IsRequestingAuthCode()
            {
                return mRequestAuthCode;
            }

            internal bool IsHidingPopups()
            {
                return mHidePopups;
            }

            internal bool IsForcingRefresh()
            {
                return mForceRefresh;
            }

            internal bool IsRequestingEmail()
            {
                return mRequestEmail;
            }

            internal bool IsRequestingIdToken()
            {
                return mRequestIdToken;
            }

            internal string GetAccountName()
            {
                return mAccountName;
            }

            /// <summary>
            ///     Gets the Oauth scopes to be requested from the user.
            /// </summary>
            /// <returns>String array of scopes.</returns>
            internal string[] getScopes()
            {
                return mScopes == null ? new string[0] : mScopes.ToArray();
            }
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 31 + EnableSavedGames.GetHashCode();
                hash = hash * 31 + IsForcingRefresh.GetHashCode();
                hash = hash * 31 + IsHidingPopups.GetHashCode();
                hash = hash * 31 + IsRequestingEmail.GetHashCode();
                hash = hash * 31 + IsRequestingAuthCode.GetHashCode();
                hash = hash * 31 + Scopes.GetHashCode();
                hash = hash * 31 + AccountName.GetHashCode();
                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            return this == (PlayGamesClientConfiguration) obj;
        }
    }
}
#endif