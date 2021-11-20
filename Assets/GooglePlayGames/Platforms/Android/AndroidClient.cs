// <copyright file="NativeClient.cs" company="Google Inc.">
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
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.Events;
using GooglePlayGames.BasicApi.SavedGame;
using GooglePlayGames.BasicApi.Video;
using GooglePlayGames.OurUtils;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Logger = GooglePlayGames.OurUtils.Logger;

#if UNITY_ANDROID
#pragma warning disable 0642 // Possible mistaken empty statement

namespace GooglePlayGames.Android
{
    public class AndroidClient : IPlayGamesClient
    {
        static readonly string TasksClassName = "com.google.android.gms.tasks.Tasks";
        readonly object AuthStateLock = new object();

        readonly object GameServicesLock = new object();

        readonly PlayGamesClientConfiguration mConfiguration;

        readonly int mFriendsMaxResults = 200; // the maximum load friends page size

        readonly int mLeaderboardMaxResults = 25; // can be from 1 to 25
        volatile AuthState mAuthState = AuthState.Unauthenticated;
        volatile IEventsClient mEventsClient;
        IUserProfile[] mFriends = new IUserProfile[0];

        AndroidJavaObject mFriendsResolutionException;

        readonly AndroidJavaClass mGamesClass = new AndroidJavaClass("com.google.android.gms.games.Games");
        LoadFriendsStatus mLastLoadFriendsStatus = LoadFriendsStatus.Unknown;
        volatile ISavedGameClient mSavedGameClient;
        volatile AndroidTokenClient mTokenClient;
        volatile Player mUser;
        volatile IVideoClient mVideoClient;

        internal AndroidClient(PlayGamesClientConfiguration configuration)
        {
            PlayGamesHelperObject.CreateObject();
            mConfiguration = Misc.CheckNotNull(configuration);
        }

        /// <summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.Authenticate" />
        public void Authenticate(bool silent, Action<SignInStatus> callback)
        {
            lock (AuthStateLock)
            {
                // If the user is already authenticated, just fire the callback, we don't need
                // any additional work.
                if (mAuthState == AuthState.Authenticated)
                {
                    Debug.Log("Already authenticated.");
                    InvokeCallbackOnGameThread(callback, SignInStatus.Success);
                    return;
                }
            }

            InitializeTokenClient();

            Debug.Log("Starting Auth with token client.");
            mTokenClient.FetchTokens(silent, result =>
            {
                var succeed = result == 0 /* CommonStatusCodes.SUCCEED */;
                InitializeGameServices();
                if (succeed)
                    using (var signInTasks = new AndroidJavaObject("java.util.ArrayList"))
                    {
                        var taskGetPlayer =
                            getPlayersClient().Call<AndroidJavaObject>("getCurrentPlayer");
                        var taskGetActivationHint =
                            getGamesClient().Call<AndroidJavaObject>("getActivationHint");
                        var taskIsCaptureSupported =
                            getVideosClient().Call<AndroidJavaObject>("isCaptureSupported");

                        if (!mConfiguration.IsHidingPopups)
                        {
                            AndroidJavaObject taskSetViewForPopups;
                            using (var popupView = AndroidHelperFragment.GetDefaultPopupView())
                            {
                                taskSetViewForPopups =
                                    getGamesClient().Call<AndroidJavaObject>("setViewForPopups", popupView);
                            }

                            signInTasks.Call<bool>("add", taskSetViewForPopups);
                        }

                        signInTasks.Call<bool>("add", taskGetPlayer);
                        signInTasks.Call<bool>("add", taskGetActivationHint);
                        signInTasks.Call<bool>("add", taskIsCaptureSupported);

                        using (var tasks = new AndroidJavaClass(TasksClassName))
                        using (var allTask = tasks.CallStatic<AndroidJavaObject>("whenAll", signInTasks))
                        {
                            AndroidTaskUtils.AddOnCompleteListener<AndroidJavaObject>(
                                allTask,
                                completeTask =>
                                {
                                    if (completeTask.Call<bool>("isSuccessful"))
                                    {
                                        using (var resultObject = taskGetPlayer.Call<AndroidJavaObject>("getResult"))
                                        {
                                            mUser = AndroidJavaConverter.ToPlayer(resultObject);
                                        }

                                        var account = mTokenClient.GetAccount();
                                        lock (GameServicesLock)
                                        {
                                            mSavedGameClient = new AndroidSavedGameClient(this, account);
                                            mEventsClient = new AndroidEventsClient(account);
                                            bool isCaptureSupported;
                                            using (var resultObject =
                                                taskIsCaptureSupported.Call<AndroidJavaObject>("getResult"))
                                            {
                                                isCaptureSupported = resultObject.Call<bool>("booleanValue");
                                            }

                                            mVideoClient = new AndroidVideoClient(isCaptureSupported, account);
                                        }

                                        mAuthState = AuthState.Authenticated;
                                        InvokeCallbackOnGameThread(callback, SignInStatus.Success);
                                        Logger.d("Authentication succeeded");
                                        LoadAchievements(ignore => { });
                                    }
                                    else
                                    {
                                        SignOut();
                                        if (completeTask.Call<bool>("isCanceled"))
                                        {
                                            InvokeCallbackOnGameThread(callback, SignInStatus.Canceled);
                                            return;
                                        }

                                        using (var exception = completeTask.Call<AndroidJavaObject>("getException"))
                                        {
                                            Logger.e(
                                                "Authentication failed - " + exception.Call<string>("toString"));
                                            InvokeCallbackOnGameThread(callback, SignInStatus.InternalError);
                                        }
                                    }
                                }
                            );
                        }
                    }
                else
                    lock (AuthStateLock)
                    {
                        Debug.Log("Returning an error code.");
                        InvokeCallbackOnGameThread(callback, SignInHelper.ToSignInStatus(result));
                    }
            });
        }

        /// <summary>
        ///     Gets the user's email.
        /// </summary>
        /// <remarks>
        ///     The email address returned is selected by the user from the accounts present
        ///     on the device. There is no guarantee this uniquely identifies the player.
        ///     For unique identification use the id property of the local player.
        ///     The user can also choose to not select any email address, meaning it is not
        ///     available.
        /// </remarks>
        /// <returns>
        ///     The user email or null if not authenticated or the permission is
        ///     not available.
        /// </returns>
        public string GetUserEmail()
        {
            if (!IsAuthenticated())
            {
                Debug.Log("Cannot get API client - not authenticated");
                return null;
            }

            return mTokenClient.GetEmail();
        }

        /// <summary>
        ///     Returns an id token, which can be verified server side, if they are logged in.
        /// </summary>
        /// <param name="idTokenCallback">
        ///     A callback to be invoked after token is retrieved. Will be passed null value
        ///     on failure.
        /// </param>
        /// <returns>The identifier token.</returns>
        public string GetIdToken()
        {
            if (!IsAuthenticated())
            {
                Debug.Log("Cannot get API client - not authenticated");
                return null;
            }

            return mTokenClient.GetIdToken();
        }

        /// <summary>
        ///     Asynchronously retrieves the server auth code for this client.
        /// </summary>
        /// <remarks>Note: This function is currently only implemented for Android.</remarks>
        /// <param name="serverClientId">The Client ID.</param>
        /// <param name="callback">Callback for response.</param>
        public string GetServerAuthCode()
        {
            if (!IsAuthenticated())
            {
                Debug.Log("Cannot get API client - not authenticated");
                return null;
            }

            return mTokenClient.GetAuthCode();
        }

        public void GetAnotherServerAuthCode(bool reAuthenticateIfNeeded,
            Action<string> callback)
        {
            mTokenClient.GetAnotherServerAuthCode(reAuthenticateIfNeeded, AsOnGameThreadCallback(callback));
        }

        /// <summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.IsAuthenticated" />
        public bool IsAuthenticated()
        {
            lock (AuthStateLock)
            {
                return mAuthState == AuthState.Authenticated;
            }
        }

        public void LoadFriends(Action<bool> callback)
        {
            LoadAllFriends(mFriendsMaxResults, /* forceReload= */ false, /* loadMore= */ false, callback);
        }

        public void LoadFriends(int pageSize, bool forceReload,
            Action<LoadFriendsStatus> callback)
        {
            LoadFriendsPaginated(pageSize, /* isLoadMore= */ false, /* forceReload= */ forceReload,
                callback);
        }

        public void LoadMoreFriends(int pageSize, Action<LoadFriendsStatus> callback)
        {
            LoadFriendsPaginated(pageSize, /* isLoadMore= */ true, /* forceReload= */ false,
                callback);
        }

        public LoadFriendsStatus GetLastLoadFriendsStatus()
        {
            return mLastLoadFriendsStatus;
        }

        public void AskForLoadFriendsResolution(Action<UIStatus> callback)
        {
            if (mFriendsResolutionException == null)
            {
                Logger.d("The developer asked for access to the friends " +
                         "list but there is no intent to trigger the UI. This may be because the user " +
                         "has granted access already or the game has not called loadFriends() before.");
                using (var playersClient = getPlayersClient())
                using (
                    var task = playersClient.Call<AndroidJavaObject>("loadFriends", /* pageSize= */ 1,
                        /* forceReload= */ false))
                {
                    AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                        task, annotatedData => { InvokeCallbackOnGameThread(callback, UIStatus.Valid); });
                    AndroidTaskUtils.AddOnFailureListener(task, exception =>
                    {
                        AndroidHelperFragment.IsResolutionRequired(exception, resolutionRequired =>
                        {
                            if (resolutionRequired)
                            {
                                mFriendsResolutionException =
                                    exception.Call<AndroidJavaObject>("getResolution");
                                AndroidHelperFragment.AskForLoadFriendsResolution(
                                    mFriendsResolutionException, AsOnGameThreadCallback(callback));
                            }
                            else
                            {
                                var statusCode = exception.Call<int>("getStatusCode");
                                if (statusCode == /* GamesClientStatusCodes.NETWORK_ERROR_NO_DATA */ 26504)
                                {
                                    InvokeCallbackOnGameThread(callback, UIStatus.NetworkError);
                                    return;
                                }

                                Debug.Log("LoadFriends failed with status code: " + statusCode);
                                InvokeCallbackOnGameThread(callback, UIStatus.InternalError);
                            }
                        });
                    });
                }
            }
            else
            {
                AndroidHelperFragment.AskForLoadFriendsResolution(mFriendsResolutionException,
                    AsOnGameThreadCallback(callback));
            }
        }

        public void ShowCompareProfileWithAlternativeNameHintsUI(string playerId,
            string otherPlayerInGameName,
            string currentPlayerInGameName,
            Action<UIStatus> callback)
        {
            AndroidHelperFragment.ShowCompareProfileWithAlternativeNameHintsUI(
                playerId, otherPlayerInGameName, currentPlayerInGameName,
                GetUiSignOutCallbackOnGameThread(callback));
        }

        public void GetFriendsListVisibility(bool forceReload,
            Action<FriendsListVisibilityStatus> callback)
        {
            using (var playersClient = getPlayersClient())
            using (
                var task = playersClient.Call<AndroidJavaObject>("getCurrentPlayer", forceReload))
            {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(task, annotatedData =>
                {
                    var currentPlayerInfo =
                        annotatedData.Call<AndroidJavaObject>("get").Call<AndroidJavaObject>(
                            "getCurrentPlayerInfo");
                    var playerListVisibility =
                        currentPlayerInfo.Call<int>("getFriendsListVisibilityStatus");
                    InvokeCallbackOnGameThread(callback,
                        AndroidJavaConverter.ToFriendsListVisibilityStatus(playerListVisibility));
                });
                AndroidTaskUtils.AddOnFailureListener(task,
                    exception => { InvokeCallbackOnGameThread(callback, FriendsListVisibilityStatus.NetworkError); });
            }
        }

        public IUserProfile[] GetFriends()
        {
            return mFriends;
        }

        /// <summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.SignOut" />
        public void SignOut()
        {
            SignOut( /* uiCallback= */ null);
        }

        /// <summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.GetUserId" />
        public string GetUserId()
        {
            if (mUser == null) return null;

            return mUser.id;
        }

        /// <summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.GetUserDisplayName" />
        public string GetUserDisplayName()
        {
            if (mUser == null) return null;

            return mUser.userName;
        }

        /// <summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.GetUserImageUrl" />
        public string GetUserImageUrl()
        {
            if (mUser == null) return null;

            return mUser.AvatarURL;
        }

        public void SetGravityForPopups(Gravity gravity)
        {
            if (!IsAuthenticated()) Logger.d("Cannot call SetGravityForPopups when not authenticated");

            using (var gamesClient = getGamesClient())
            using (gamesClient.Call<AndroidJavaObject>("setGravityForPopups",
                (int) gravity | (int) Gravity.CENTER_HORIZONTAL))
            {
                ;
            }
        }

        /// <summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.GetPlayerStats" />
        public void GetPlayerStats(Action<CommonStatusCodes, PlayerStats> callback)
        {
            using (var playerStatsClient = getPlayerStatsClient())
            using (var task = playerStatsClient.Call<AndroidJavaObject>("loadPlayerStats", /* forceReload= */ false))
            {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                    task,
                    annotatedData =>
                    {
                        using (var playerStatsJava = annotatedData.Call<AndroidJavaObject>("get"))
                        {
                            var numberOfPurchases = playerStatsJava.Call<int>("getNumberOfPurchases");
                            var avgSessionLength = playerStatsJava.Call<float>("getAverageSessionLength");
                            var daysSinceLastPlayed = playerStatsJava.Call<int>("getDaysSinceLastPlayed");
                            var numberOfSessions = playerStatsJava.Call<int>("getNumberOfSessions");
                            var sessionPercentile = playerStatsJava.Call<float>("getSessionPercentile");
                            var spendPercentile = playerStatsJava.Call<float>("getSpendPercentile");
                            var spendProbability = playerStatsJava.Call<float>("getSpendProbability");
                            var churnProbability = playerStatsJava.Call<float>("getChurnProbability");
                            var highSpenderProbability = playerStatsJava.Call<float>("getHighSpenderProbability");
                            var totalSpendNext28Days = playerStatsJava.Call<float>("getTotalSpendNext28Days");

                            var result = new PlayerStats(
                                numberOfPurchases,
                                avgSessionLength,
                                daysSinceLastPlayed,
                                numberOfSessions,
                                sessionPercentile,
                                spendPercentile,
                                spendProbability,
                                churnProbability,
                                highSpenderProbability,
                                totalSpendNext28Days);

                            InvokeCallbackOnGameThread(callback, CommonStatusCodes.Success, result);
                        }
                    });

                AddOnFailureListenerWithSignOut(
                    task,
                    e =>
                    {
                        Debug.Log("GetPlayerStats failed: " + e.Call<string>("toString"));
                        var statusCode = IsAuthenticated()
                            ? CommonStatusCodes.InternalError
                            : CommonStatusCodes.SignInRequired;
                        InvokeCallbackOnGameThread(callback, statusCode, new PlayerStats());
                    });
            }
        }

        /// <summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.LoadUsers" />
        public void LoadUsers(string[] userIds, Action<IUserProfile[]> callback)
        {
            if (!IsAuthenticated())
            {
                InvokeCallbackOnGameThread(callback, new IUserProfile[0]);
                return;
            }

            using (var playersClient = getPlayersClient())
            {
                var countLock = new object();
                var count = userIds.Length;
                var resultCount = 0;
                var users = new IUserProfile[count];
                for (var i = 0; i < count; ++i)
                    using (var task = playersClient.Call<AndroidJavaObject>("loadPlayer", userIds[i]))
                    {
                        AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                            task,
                            annotatedData =>
                            {
                                using (var player = annotatedData.Call<AndroidJavaObject>("get"))
                                {
                                    var playerId = player.Call<string>("getPlayerId");
                                    for (var j = 0; j < count; ++j)
                                        if (playerId == userIds[j])
                                        {
                                            users[j] = AndroidJavaConverter.ToPlayer(player);
                                            break;
                                        }

                                    lock (countLock)
                                    {
                                        ++resultCount;
                                        if (resultCount == count) InvokeCallbackOnGameThread(callback, users);
                                    }
                                }
                            });

                        AddOnFailureListenerWithSignOut(task, exception =>
                        {
                            Debug.Log("LoadUsers failed for index " + i +
                                      " with: " + exception.Call<string>("toString"));
                            lock (countLock)
                            {
                                ++resultCount;
                                if (resultCount == count) InvokeCallbackOnGameThread(callback, users);
                            }
                        });
                    }
            }
        }

        /// <summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.LoadAchievements" />
        public void LoadAchievements(Action<Achievement[]> callback)
        {
            using (var achievementsClient = getAchievementsClient())
            using (var task = achievementsClient.Call<AndroidJavaObject>("load", /* forceReload= */ false))
            {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                    task,
                    annotatedData =>
                    {
                        using (var achievementBuffer = annotatedData.Call<AndroidJavaObject>("get"))
                        {
                            var count = achievementBuffer.Call<int>("getCount");
                            var result = new Achievement[count];
                            for (var i = 0; i < count; ++i)
                            {
                                var achievement = new Achievement();
                                using (var javaAchievement = achievementBuffer.Call<AndroidJavaObject>("get", i))
                                {
                                    achievement.Id = javaAchievement.Call<string>("getAchievementId");
                                    achievement.Description = javaAchievement.Call<string>("getDescription");
                                    achievement.Name = javaAchievement.Call<string>("getName");
                                    achievement.Points = javaAchievement.Call<ulong>("getXpValue");

                                    var timestamp = javaAchievement.Call<long>("getLastUpdatedTimestamp");
                                    achievement.LastModifiedTime = AndroidJavaConverter.ToDateTime(timestamp);

                                    achievement.RevealedImageUrl = javaAchievement.Call<string>("getRevealedImageUrl");
                                    achievement.UnlockedImageUrl = javaAchievement.Call<string>("getUnlockedImageUrl");
                                    achievement.IsIncremental =
                                        javaAchievement.Call<int>("getType") == 1 /* TYPE_INCREMENTAL */;
                                    if (achievement.IsIncremental)
                                    {
                                        achievement.CurrentSteps = javaAchievement.Call<int>("getCurrentSteps");
                                        achievement.TotalSteps = javaAchievement.Call<int>("getTotalSteps");
                                    }

                                    var state = javaAchievement.Call<int>("getState");
                                    achievement.IsUnlocked = state == 0 /* STATE_UNLOCKED */;
                                    achievement.IsRevealed = state == 1 /* STATE_REVEALED */;
                                }

                                result[i] = achievement;
                            }

                            achievementBuffer.Call("release");
                            InvokeCallbackOnGameThread(callback, result);
                        }
                    });

                AddOnFailureListenerWithSignOut(
                    task,
                    exception =>
                    {
                        Debug.Log("LoadAchievements failed: " + exception.Call<string>("toString"));
                        InvokeCallbackOnGameThread(callback, new Achievement[0]);
                    });
            }
        }

        /// <summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.UnlockAchievement" />
        public void UnlockAchievement(string achId, Action<bool> callback)
        {
            if (!IsAuthenticated())
            {
                InvokeCallbackOnGameThread(callback, false);
                return;
            }

            using (var achievementsClient = getAchievementsClient())
            {
                achievementsClient.Call("unlock", achId);
                InvokeCallbackOnGameThread(callback, true);
            }
        }

        /// <summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.RevealAchievement" />
        public void RevealAchievement(string achId, Action<bool> callback)
        {
            if (!IsAuthenticated())
            {
                InvokeCallbackOnGameThread(callback, false);
                return;
            }

            using (var achievementsClient = getAchievementsClient())
            {
                achievementsClient.Call("reveal", achId);
                InvokeCallbackOnGameThread(callback, true);
            }
        }

        /// <summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.IncrementAchievement" />
        public void IncrementAchievement(string achId, int steps, Action<bool> callback)
        {
            if (!IsAuthenticated())
            {
                InvokeCallbackOnGameThread(callback, false);
                return;
            }

            using (var achievementsClient = getAchievementsClient())
            {
                achievementsClient.Call("increment", achId, steps);
                InvokeCallbackOnGameThread(callback, true);
            }
        }

        /// <summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.SetStepsAtLeast" />
        public void SetStepsAtLeast(string achId, int steps, Action<bool> callback)
        {
            if (!IsAuthenticated())
            {
                InvokeCallbackOnGameThread(callback, false);
                return;
            }

            using (var achievementsClient = getAchievementsClient())
            {
                achievementsClient.Call("setSteps", achId, steps);
                InvokeCallbackOnGameThread(callback, true);
            }
        }

        /// <summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.ShowAchievementsUI" />
        public void ShowAchievementsUI(Action<UIStatus> callback)
        {
            if (!IsAuthenticated())
            {
                InvokeCallbackOnGameThread(callback, UIStatus.NotAuthorized);
                return;
            }

            AndroidHelperFragment.ShowAchievementsUI(GetUiSignOutCallbackOnGameThread(callback));
        }

        /// <summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.LeaderboardMaxResults" />
        public int LeaderboardMaxResults()
        {
            return mLeaderboardMaxResults;
        }

        /// <summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.ShowLeaderboardUI" />
        public void ShowLeaderboardUI(string leaderboardId, LeaderboardTimeSpan span, Action<UIStatus> callback)
        {
            if (!IsAuthenticated())
            {
                InvokeCallbackOnGameThread(callback, UIStatus.NotAuthorized);
                return;
            }

            if (leaderboardId == null)
                AndroidHelperFragment.ShowAllLeaderboardsUI(GetUiSignOutCallbackOnGameThread(callback));
            else
                AndroidHelperFragment.ShowLeaderboardUI(leaderboardId, span,
                    GetUiSignOutCallbackOnGameThread(callback));
        }

        /// <summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.LoadScores" />
        public void LoadScores(string leaderboardId, LeaderboardStart start,
            int rowCount, LeaderboardCollection collection,
            LeaderboardTimeSpan timeSpan,
            Action<LeaderboardScoreData> callback)
        {
            using (var client = getLeaderboardsClient())
            {
                var loadScoresMethod =
                    start == LeaderboardStart.TopScores ? "loadTopScores" : "loadPlayerCenteredScores";
                using (var task = client.Call<AndroidJavaObject>(
                    loadScoresMethod,
                    leaderboardId,
                    AndroidJavaConverter.ToLeaderboardVariantTimeSpan(timeSpan),
                    AndroidJavaConverter.ToLeaderboardVariantCollection(collection),
                    rowCount))
                {
                    AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                        task,
                        annotatedData =>
                        {
                            using (var leaderboardScores = annotatedData.Call<AndroidJavaObject>("get"))
                            {
                                InvokeCallbackOnGameThread(callback, CreateLeaderboardScoreData(
                                    leaderboardId,
                                    collection,
                                    timeSpan,
                                    annotatedData.Call<bool>("isStale")
                                        ? ResponseStatus.SuccessWithStale
                                        : ResponseStatus.Success,
                                    leaderboardScores));
                                leaderboardScores.Call("release");
                            }
                        });

                    AddOnFailureListenerWithSignOut(task, exception =>
                    {
                        AndroidHelperFragment.IsResolutionRequired(
                            exception, resolutionRequired =>
                            {
                                if (resolutionRequired)
                                {
                                    mFriendsResolutionException = exception.Call<AndroidJavaObject>(
                                        "getResolution");
                                    InvokeCallbackOnGameThread(
                                        callback, new LeaderboardScoreData(leaderboardId,
                                            ResponseStatus.ResolutionRequired));
                                }
                                else
                                {
                                    mFriendsResolutionException = null;
                                }
                            });
                        Debug.Log("LoadScores failed: " + exception.Call<string>("toString"));
                        InvokeCallbackOnGameThread(
                            callback, new LeaderboardScoreData(leaderboardId,
                                ResponseStatus.InternalError));
                    });
                }
            }
        }

        /// <summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.LoadMoreScores" />
        public void LoadMoreScores(ScorePageToken token, int rowCount,
            Action<LeaderboardScoreData> callback)
        {
            using (var client = getLeaderboardsClient())
            using (var task = client.Call<AndroidJavaObject>("loadMoreScores",
                token.InternalObject, rowCount, AndroidJavaConverter.ToPageDirection(token.Direction)))
            {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                    task,
                    annotatedData =>
                    {
                        using (var leaderboardScores = annotatedData.Call<AndroidJavaObject>("get"))
                        {
                            InvokeCallbackOnGameThread(callback, CreateLeaderboardScoreData(
                                token.LeaderboardId,
                                token.Collection,
                                token.TimeSpan,
                                annotatedData.Call<bool>("isStale")
                                    ? ResponseStatus.SuccessWithStale
                                    : ResponseStatus.Success,
                                leaderboardScores));
                            leaderboardScores.Call("release");
                        }
                    });

                AddOnFailureListenerWithSignOut(task, exception =>
                {
                    AndroidHelperFragment.IsResolutionRequired(exception, resolutionRequired =>
                    {
                        if (resolutionRequired)
                        {
                            mFriendsResolutionException =
                                exception.Call<AndroidJavaObject>("getResolution");
                            InvokeCallbackOnGameThread(
                                callback, new LeaderboardScoreData(token.LeaderboardId,
                                    ResponseStatus.ResolutionRequired));
                        }
                        else
                        {
                            mFriendsResolutionException = null;
                        }
                    });
                    Debug.Log("LoadMoreScores failed: " + exception.Call<string>("toString"));
                    InvokeCallbackOnGameThread(
                        callback, new LeaderboardScoreData(token.LeaderboardId,
                            ResponseStatus.InternalError));
                });
            }
        }

        /// <summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.SubmitScore" />
        public void SubmitScore(string leaderboardId, long score, Action<bool> callback)
        {
            if (!IsAuthenticated()) InvokeCallbackOnGameThread(callback, false);

            using (var client = getLeaderboardsClient())
            {
                client.Call("submitScore", leaderboardId, score);
                InvokeCallbackOnGameThread(callback, true);
            }
        }

        /// <summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.SubmitScore" />
        public void SubmitScore(string leaderboardId, long score, string metadata,
            Action<bool> callback)
        {
            if (!IsAuthenticated()) InvokeCallbackOnGameThread(callback, false);

            using (var client = getLeaderboardsClient())
            {
                client.Call("submitScore", leaderboardId, score, metadata);
                InvokeCallbackOnGameThread(callback, true);
            }
        }

        public void RequestPermissions(string[] scopes, Action<SignInStatus> callback)
        {
            callback = AsOnGameThreadCallback(callback);
            mTokenClient.RequestPermissions(scopes, code =>
            {
                UpdateClients();
                callback(code);
            });
        }

        /// <summary>Returns whether or not user has given permissions for given scopes.</summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.HasPermissions" />
        public bool HasPermissions(string[] scopes)
        {
            return mTokenClient.HasPermissions(scopes);
        }

        /// <summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.GetSavedGameClient" />
        public ISavedGameClient GetSavedGameClient()
        {
            lock (GameServicesLock)
            {
                return mSavedGameClient;
            }
        }

        /// <summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.GetEventsClient" />
        public IEventsClient GetEventsClient()
        {
            lock (GameServicesLock)
            {
                return mEventsClient;
            }
        }

        /// <summary></summary>
        /// <seealso cref="GooglePlayGames.BasicApi.IPlayGamesClient.GetVideoClient" />
        public IVideoClient GetVideoClient()
        {
            lock (GameServicesLock)
            {
                return mVideoClient;
            }
        }

        static Action<T> AsOnGameThreadCallback<T>(Action<T> callback)
        {
            if (callback == null) return delegate { };

            return result => InvokeCallbackOnGameThread(callback, result);
        }

        static void InvokeCallbackOnGameThread(Action callback)
        {
            if (callback == null) return;

            PlayGamesHelperObject.RunOnGameThread(() => { callback(); });
        }

        static void InvokeCallbackOnGameThread<T>(Action<T> callback, T data)
        {
            if (callback == null) return;

            PlayGamesHelperObject.RunOnGameThread(() => { callback(data); });
        }


        static Action<T1, T2> AsOnGameThreadCallback<T1, T2>(
            Action<T1, T2> toInvokeOnGameThread)
        {
            return (result1, result2) =>
            {
                if (toInvokeOnGameThread == null) return;

                PlayGamesHelperObject.RunOnGameThread(() => toInvokeOnGameThread(result1, result2));
            };
        }

        static void InvokeCallbackOnGameThread<T1, T2>(Action<T1, T2> callback, T1 t1, T2 t2)
        {
            if (callback == null) return;

            PlayGamesHelperObject.RunOnGameThread(() => { callback(t1, t2); });
        }

        void InitializeGameServices()
        {
            if (mTokenClient != null) return;

            InitializeTokenClient();
        }

        void InitializeTokenClient()
        {
            if (mTokenClient != null) return;

            mTokenClient = new AndroidTokenClient();

            if (!GameInfo.WebClientIdInitialized() &&
                (mConfiguration.IsRequestingIdToken || mConfiguration.IsRequestingAuthCode))
                Logger.e("Server Auth Code and ID Token require web clientId to configured.");

            var scopes = mConfiguration.Scopes;
            // Set the auth flags in the token client.
            mTokenClient.SetWebClientId(GameInfo.WebClientId);
            mTokenClient.SetRequestAuthCode(mConfiguration.IsRequestingAuthCode, mConfiguration.IsForcingRefresh);
            mTokenClient.SetRequestEmail(mConfiguration.IsRequestingEmail);
            mTokenClient.SetRequestIdToken(mConfiguration.IsRequestingIdToken);
            mTokenClient.SetHidePopups(mConfiguration.IsHidingPopups);
            mTokenClient.AddOauthScopes("https://www.googleapis.com/auth/games_lite");
            if (mConfiguration.EnableSavedGames)
                mTokenClient.AddOauthScopes("https://www.googleapis.com/auth/drive.appdata");

            mTokenClient.AddOauthScopes(scopes);
            mTokenClient.SetAccountName(mConfiguration.AccountName);
        }

        void LoadAllFriends(int pageSize, bool forceReload, bool loadMore,
            Action<bool> callback)
        {
            LoadFriendsPaginated(pageSize, loadMore, forceReload, result =>
            {
                mLastLoadFriendsStatus = result;
                switch (result)
                {
                    case LoadFriendsStatus.Completed:
                        InvokeCallbackOnGameThread(callback, true);
                        break;
                    case LoadFriendsStatus.LoadMore:
                        // There are more friends to load.
                        LoadAllFriends(pageSize, /* forceReload= */ false, /* loadMore= */ true, callback);
                        break;
                    case LoadFriendsStatus.ResolutionRequired:
                    case LoadFriendsStatus.InternalError:
                    case LoadFriendsStatus.NotAuthorized:
                        InvokeCallbackOnGameThread(callback, false);
                        break;
                    default:
                        Logger.d("There was an error when loading friends." + result);
                        InvokeCallbackOnGameThread(callback, false);
                        break;
                }
            });
        }

        void LoadFriendsPaginated(int pageSize, bool isLoadMore, bool forceReload,
            Action<LoadFriendsStatus> callback)
        {
            mFriendsResolutionException = null;
            using (var playersClient = getPlayersClient())
            using (var task = isLoadMore
                ? playersClient.Call<AndroidJavaObject>("loadMoreFriends", pageSize)
                : playersClient.Call<AndroidJavaObject>("loadFriends", pageSize,
                    forceReload))
            {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                    task, annotatedData =>
                    {
                        using (var playersBuffer = annotatedData.Call<AndroidJavaObject>("get"))
                        {
                            var metadata = playersBuffer.Call<AndroidJavaObject>("getMetadata");
                            var areMoreFriendsToLoad = metadata != null &&
                                                       metadata.Call<AndroidJavaObject>("getString",
                                                           "next_page_token") != null;
                            mFriends = AndroidJavaConverter.playersBufferToArray(playersBuffer);
                            mLastLoadFriendsStatus = areMoreFriendsToLoad
                                ? LoadFriendsStatus.LoadMore
                                : LoadFriendsStatus.Completed;
                            InvokeCallbackOnGameThread(callback, mLastLoadFriendsStatus);
                        }
                    });

                AndroidTaskUtils.AddOnFailureListener(task, exception =>
                {
                    AndroidHelperFragment.IsResolutionRequired(exception, resolutionRequired =>
                    {
                        if (resolutionRequired)
                        {
                            mFriendsResolutionException =
                                exception.Call<AndroidJavaObject>("getResolution");
                            mLastLoadFriendsStatus = LoadFriendsStatus.ResolutionRequired;
                            mFriends = new IUserProfile[0];
                            InvokeCallbackOnGameThread(callback, LoadFriendsStatus.ResolutionRequired);
                        }
                        else
                        {
                            mFriendsResolutionException = null;

                            if (Misc.IsApiException(exception))
                            {
                                var statusCode = exception.Call<int>("getStatusCode");
                                if (statusCode == /* GamesClientStatusCodes.NETWORK_ERROR_NO_DATA */ 26504)
                                {
                                    mLastLoadFriendsStatus = LoadFriendsStatus.NetworkError;
                                    InvokeCallbackOnGameThread(callback, LoadFriendsStatus.NetworkError);
                                    return;
                                }
                            }

                            mLastLoadFriendsStatus = LoadFriendsStatus.InternalError;
                            Logger.e("LoadFriends failed: " +
                                     exception.Call<string>("toString"));
                            InvokeCallbackOnGameThread(callback, LoadFriendsStatus.InternalError);
                        }
                    });
                });
            }
        }


        public void SignOut(Action uiCallback)
        {
            if (mTokenClient == null)
            {
                InvokeCallbackOnGameThread(uiCallback);
                return;
            }

            mTokenClient.Signout();
            mAuthState = AuthState.Unauthenticated;
            if (uiCallback != null) InvokeCallbackOnGameThread(uiCallback);

            PlayGamesHelperObject.RunOnGameThread(() => SignInHelper.SetPromptUiSignIn(true));
        }

        void AddOnFailureListenerWithSignOut(AndroidJavaObject task, Action<AndroidJavaObject> callback)
        {
            AndroidTaskUtils.AddOnFailureListener(
                task,
                exception =>
                {
                    var statusCode = exception.Call<int>("getStatusCode");
                    if (statusCode == /* CommonStatusCodes.SignInRequired */ 4 ||
                        statusCode == /* GamesClientStatusCodes.CLIENT_RECONNECT_REQUIRED */ 26502)
                        SignOut();

                    callback(exception);
                });
        }

        Action<UIStatus> GetUiSignOutCallbackOnGameThread(Action<UIStatus> callback)
        {
            Action<UIStatus> uiCallback = status =>
            {
                if (status == UIStatus.NotAuthorized)
                {
                    SignOut(() =>
                    {
                        if (callback != null) callback(status);
                    });
                }
                else
                {
                    if (callback != null) callback(status);
                }
            };

            return AsOnGameThreadCallback(uiCallback);
        }

        LeaderboardScoreData CreateLeaderboardScoreData(
            string leaderboardId,
            LeaderboardCollection collection,
            LeaderboardTimeSpan timespan,
            ResponseStatus status,
            AndroidJavaObject leaderboardScoresJava)
        {
            var leaderboardScoreData = new LeaderboardScoreData(leaderboardId, status);
            var scoresBuffer = leaderboardScoresJava.Call<AndroidJavaObject>("getScores");
            var count = scoresBuffer.Call<int>("getCount");
            for (var i = 0; i < count; ++i)
                using (var leaderboardScore = scoresBuffer.Call<AndroidJavaObject>("get", i))
                {
                    var timestamp = leaderboardScore.Call<long>("getTimestampMillis");
                    var date = AndroidJavaConverter.ToDateTime(timestamp);

                    var rank = (ulong) leaderboardScore.Call<long>("getRank");
                    var scoreHolderId = "";
                    using (var scoreHolder = leaderboardScore.Call<AndroidJavaObject>("getScoreHolder"))
                    {
                        scoreHolderId = scoreHolder.Call<string>("getPlayerId");
                    }

                    var score = (ulong) leaderboardScore.Call<long>("getRawScore");
                    var metadata = leaderboardScore.Call<string>("getScoreTag");

                    leaderboardScoreData.AddScore(new PlayGamesScore(date, leaderboardId,
                        rank, scoreHolderId, score, metadata));
                }

            leaderboardScoreData.NextPageToken = new ScorePageToken(scoresBuffer, leaderboardId, collection,
                timespan, ScorePageDirection.Forward);
            leaderboardScoreData.PrevPageToken = new ScorePageToken(scoresBuffer, leaderboardId, collection,
                timespan, ScorePageDirection.Backward);

            using (var leaderboard = leaderboardScoresJava.Call<AndroidJavaObject>("getLeaderboard"))
            using (var variants = leaderboard.Call<AndroidJavaObject>("getVariants"))
            using (var variant = variants.Call<AndroidJavaObject>("get", 0))
            {
                leaderboardScoreData.Title = leaderboard.Call<string>("getDisplayName");
                if (variant.Call<bool>("hasPlayerInfo"))
                {
                    var date = AndroidJavaConverter.ToDateTime(0);
                    var rank = (ulong) variant.Call<long>("getPlayerRank");
                    var score = (ulong) variant.Call<long>("getRawPlayerScore");
                    var metadata = variant.Call<string>("getPlayerScoreTag");
                    leaderboardScoreData.PlayerScore = new PlayGamesScore(date, leaderboardId,
                        rank, mUser.id, score, metadata);
                }

                leaderboardScoreData.ApproximateCount = (ulong) variant.Call<long>("getNumScores");
            }

            return leaderboardScoreData;
        }

        void UpdateClients()
        {
            lock (GameServicesLock)
            {
                var account = mTokenClient.GetAccount();
                mSavedGameClient = new AndroidSavedGameClient(this, account);
                mEventsClient = new AndroidEventsClient(account);
                mVideoClient = new AndroidVideoClient(mVideoClient.IsCaptureSupported(), account);
            }
        }

        AndroidJavaObject getAchievementsClient()
        {
            return mGamesClass.CallStatic<AndroidJavaObject>("getAchievementsClient",
                AndroidHelperFragment.GetActivity(), mTokenClient.GetAccount());
        }

        AndroidJavaObject getGamesClient()
        {
            return mGamesClass.CallStatic<AndroidJavaObject>("getGamesClient", AndroidHelperFragment.GetActivity(),
                mTokenClient.GetAccount());
        }

        AndroidJavaObject getPlayersClient()
        {
            return mGamesClass.CallStatic<AndroidJavaObject>("getPlayersClient", AndroidHelperFragment.GetActivity(),
                mTokenClient.GetAccount());
        }

        AndroidJavaObject getLeaderboardsClient()
        {
            return mGamesClass.CallStatic<AndroidJavaObject>("getLeaderboardsClient",
                AndroidHelperFragment.GetActivity(), mTokenClient.GetAccount());
        }

        AndroidJavaObject getPlayerStatsClient()
        {
            return mGamesClass.CallStatic<AndroidJavaObject>("getPlayerStatsClient",
                AndroidHelperFragment.GetActivity(), mTokenClient.GetAccount());
        }

        AndroidJavaObject getVideosClient()
        {
            return mGamesClass.CallStatic<AndroidJavaObject>("getVideosClient", AndroidHelperFragment.GetActivity(),
                mTokenClient.GetAccount());
        }

        enum AuthState
        {
            Unauthenticated,
            Authenticated
        }
    }
}
#endif