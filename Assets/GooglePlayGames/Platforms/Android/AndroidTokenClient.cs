// <copyright file="AndroidTokenClient.cs" company="Google Inc.">
// Copyright (C) 2015 Google Inc.
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
//  limitations under the License.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using GooglePlayGames.BasicApi;
using GooglePlayGames.OurUtils;
using UnityEngine;
using Logger = GooglePlayGames.OurUtils.Logger;

#if UNITY_ANDROID
namespace GooglePlayGames.Android
{
    internal class AndroidTokenClient : TokenClient
    {
        const string HelperFragmentClass = "com.google.games.bridge.HelperFragment";

        // These are the results
        AndroidJavaObject account;
        string accountName;
        string authCode;
        string email;
        bool forceRefresh;
        bool hidePopups;
        string idToken;
        List<string> oauthScopes;
        bool requestAuthCode;

        // These are the configuration values.
        bool requestEmail;
        bool requestIdToken;
        string webClientId;

        public void SetRequestAuthCode(bool flag, bool forceRefresh)
        {
            requestAuthCode = flag;
            this.forceRefresh = forceRefresh;
        }

        public void SetRequestEmail(bool flag)
        {
            requestEmail = flag;
        }

        public void SetRequestIdToken(bool flag)
        {
            requestIdToken = flag;
        }

        public void SetWebClientId(string webClientId)
        {
            this.webClientId = webClientId;
        }

        public void SetHidePopups(bool flag)
        {
            hidePopups = flag;
        }

        public void SetAccountName(string accountName)
        {
            this.accountName = accountName;
        }

        public void AddOauthScopes(params string[] scopes)
        {
            if (scopes != null)
            {
                if (oauthScopes == null) oauthScopes = new List<string>();

                oauthScopes.AddRange(scopes);
            }
        }

        public void Signout()
        {
            account = null;
            authCode = null;
            email = null;
            idToken = null;
            PlayGamesHelperObject.RunOnGameThread(() =>
            {
                Debug.Log("Calling Signout in token client");
                var cls = new AndroidJavaClass(HelperFragmentClass);
                cls.CallStatic("signOut", AndroidHelperFragment.GetActivity());
            });
        }

        /// <summary>Gets the email selected by the current player.</summary>
        /// <remarks>
        ///     This is not necessarily the email address of the player.  It
        ///     is just the account selected by the player from a list of accounts
        ///     present on the device.
        /// </remarks>
        /// <returns>A string representing the email.</returns>
        public string GetEmail()
        {
            return email;
        }

        public string GetAuthCode()
        {
            return authCode;
        }

        /// <summary>Gets the OpenID Connect ID token for authentication with a server backend.</summary>
        /// <param name="serverClientId">
        ///     Server client ID from console.developers.google.com or the Play Games
        ///     services console.
        /// </param>
        /// <param name="idTokenCallback">
        ///     A callback to be invoked after token is retrieved. Will be passed null value
        ///     on failure.
        /// </param>
        public string GetIdToken()
        {
            return idToken;
        }

        public void FetchTokens(bool silent, Action<int> callback)
        {
            PlayGamesHelperObject.RunOnGameThread(() => DoFetchToken(silent, callback));
        }

        public void RequestPermissions(string[] scopes, Action<SignInStatus> callback)
        {
            using (var bridgeClass = new AndroidJavaClass(HelperFragmentClass))
            using (var currentActivity = AndroidHelperFragment.GetActivity())
            using (var task =
                bridgeClass.CallStatic<AndroidJavaObject>("showRequestPermissionsUi", currentActivity,
                    oauthScopes.Union(scopes).ToArray()))
            {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(task, /* disposeResult= */ false,
                    accountWithNewScopes =>
                    {
                        if (accountWithNewScopes == null)
                        {
                            callback(SignInStatus.InternalError);
                            return;
                        }

                        account = accountWithNewScopes;
                        email = account.Call<string>("getEmail");
                        idToken = account.Call<string>("getIdToken");
                        authCode = account.Call<string>("getServerAuthCode");
                        oauthScopes = oauthScopes.Union(scopes).ToList();
                        callback(SignInStatus.Success);
                    });

                AndroidTaskUtils.AddOnFailureListener(task, e =>
                {
                    if (!Misc.IsApiException(e))
                    {
                        Logger.e("Exception requesting new permissions" +
                                 e.Call<string>("toString"));
                        return;
                    }

                    var failCode = SignInHelper.ToSignInStatus(e.Call<int>("getStatusCode"));
                    Logger.e("Exception requesting new permissions: " + failCode);
                    callback(failCode);
                });
            }
        }

        /// <summary>Returns whether or not user has given permissions for given scopes.</summary>
        /// <param name="scopes">array of scopes</param>
        /// <returns><c>true</c>, if given, <c>false</c> otherwise.</returns>
        public bool HasPermissions(string[] scopes)
        {
            using (var bridgeClass = new AndroidJavaClass(HelperFragmentClass))
            using (var currentActivity = AndroidHelperFragment.GetActivity())
            {
                return bridgeClass.CallStatic<bool>("hasPermissions", currentActivity, scopes);
            }
        }

        /// <summary>
        ///     Gets another server auth code.
        /// </summary>
        /// <remarks>
        ///     This method should be called after authenticating, and exchanging
        ///     the initial server auth code for a token.  This is implemented by signing in
        ///     silently, which if successful returns almost immediately and with a new
        ///     server auth code.
        /// </remarks>
        /// <param name="reAuthenticateIfNeeded">
        ///     Calls Authenticate if needed when
        ///     retrieving another auth code.
        /// </param>
        /// <param name="callback">Callback.</param>
        public void GetAnotherServerAuthCode(bool reAuthenticateIfNeeded, Action<string> callback)
        {
            PlayGamesHelperObject.RunOnGameThread(() => DoGetAnotherServerAuthCode(reAuthenticateIfNeeded, callback));
        }

        void DoFetchToken(bool silent, Action<int> callback)
        {
            try
            {
                using (var bridgeClass = new AndroidJavaClass(HelperFragmentClass))
                using (var currentActivity = AndroidHelperFragment.GetActivity())
                using (var pendingResult = bridgeClass.CallStatic<AndroidJavaObject>(
                    "fetchToken",
                    currentActivity,
                    silent,
                    requestAuthCode,
                    requestEmail,
                    requestIdToken,
                    webClientId,
                    forceRefresh,
                    oauthScopes.ToArray(),
                    hidePopups,
                    accountName))
                {
                    pendingResult.Call("setResultCallback", new ResultCallbackProxy(
                        tokenResult =>
                        {
                            account = tokenResult.Call<AndroidJavaObject>("getAccount");
                            authCode = tokenResult.Call<string>("getAuthCode");
                            email = tokenResult.Call<string>("getEmail");
                            idToken = tokenResult.Call<string>("getIdToken");
                            callback(tokenResult.Call<int>("getStatusCode"));
                        }));
                }
            }
            catch (Exception e)
            {
                Logger.e("Exception launching token request: " + e.Message);
                Logger.e(e.ToString());
            }
        }

        public AndroidJavaObject GetAccount()
        {
            return account;
        }

        void DoGetAnotherServerAuthCode(bool reAuthenticateIfNeeded, Action<string> callback)
        {
            try
            {
                using (var bridgeClass = new AndroidJavaClass(HelperFragmentClass))
                using (var currentActivity = AndroidHelperFragment.GetActivity())
                using (var pendingResult = bridgeClass.CallStatic<AndroidJavaObject>(
                    "fetchToken",
                    currentActivity,
                    /* silent= */ reAuthenticateIfNeeded,
                    /* requestAuthCode= */ true,
                    /* requestEmail= */ false,
                    /* requestIdToken= */ false,
                    webClientId,
                    /* forceRefresh= */ false,
                    oauthScopes.ToArray(),
                    /* hidePopups= */ true,
                    /* accountName= */ ""))
                {
                    pendingResult.Call("setResultCallback", new ResultCallbackProxy(
                        tokenResult => { callback(tokenResult.Call<string>("getAuthCode")); }));
                }
            }
            catch (Exception e)
            {
                Logger.e("Exception launching token request: " + e.Message);
                Logger.e(e.ToString());
            }
        }

        class ResultCallbackProxy : AndroidJavaProxy
        {
            readonly Action<AndroidJavaObject> mCallback;

            public ResultCallbackProxy(Action<AndroidJavaObject> callback)
                : base("com/google/android/gms/common/api/ResultCallback")
            {
                mCallback = callback;
            }

            public void onResult(AndroidJavaObject tokenResult)
            {
                mCallback(tokenResult);
            }
        }
    }
}
#endif