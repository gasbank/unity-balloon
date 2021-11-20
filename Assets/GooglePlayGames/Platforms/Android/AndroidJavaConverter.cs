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
using GooglePlayGames.BasicApi;
using UnityEngine;
using UnityEngine.SocialPlatforms;

#if UNITY_ANDROID
namespace GooglePlayGames.Android
{
    internal class AndroidJavaConverter
    {
        internal static DateTime ToDateTime(long milliseconds)
        {
            var result = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return result.AddMilliseconds(milliseconds);
        }

        // Convert to LeaderboardVariant.java#TimeSpan
        internal static int ToLeaderboardVariantTimeSpan(LeaderboardTimeSpan span)
        {
            switch (span)
            {
                case LeaderboardTimeSpan.Daily:
                    return 0 /* TIME_SPAN_DAILY */;
                case LeaderboardTimeSpan.Weekly:
                    return 1 /* TIME_SPAN_WEEKLY */;
                case LeaderboardTimeSpan.AllTime:
                default:
                    return 2 /* TIME_SPAN_ALL_TIME */;
            }
        }

        // Convert to LeaderboardVariant.java#Collection
        internal static int ToLeaderboardVariantCollection(LeaderboardCollection collection)
        {
            switch (collection)
            {
                case LeaderboardCollection.Social:
                    return 3 /* COLLECTION_FRIENDS */;
                case LeaderboardCollection.Public:
                default:
                    return 0 /* COLLECTION_PUBLIC */;
            }
        }

        // Convert to PageDirection.java#Direction
        internal static int ToPageDirection(ScorePageDirection direction)
        {
            switch (direction)
            {
                case ScorePageDirection.Forward:
                    return 0 /* NEXT */;
                case ScorePageDirection.Backward:
                    return 1 /* PREV */;
                default:
                    return -1 /* NONE */;
            }
        }

        internal static Player ToPlayer(AndroidJavaObject player)
        {
            if (player == null) return null;

            var displayName = player.Call<string>("getDisplayName");
            var playerId = player.Call<string>("getPlayerId");
            var avatarUrl = player.Call<string>("getIconImageUrl");
            return new Player(displayName, playerId, avatarUrl);
        }

        internal static PlayerProfile ToPlayerProfile(AndroidJavaObject player)
        {
            if (player == null) return null;

            var displayName = player.Call<string>("getDisplayName");
            var playerId = player.Call<string>("getPlayerId");
            var avatarUrl = player.Call<string>("getIconImageUrl");
            var isFriend =
                player.Call<AndroidJavaObject>("getRelationshipInfo").Call<int>("getFriendStatus") ==
                4 /* PlayerFriendStatus.Friend*/;
            return new PlayerProfile(displayName, playerId, avatarUrl, isFriend);
        }

        internal static List<string> ToStringList(AndroidJavaObject stringList)
        {
            if (stringList == null) return new List<string>();

            var size = stringList.Call<int>("size");
            var converted = new List<string>(size);

            for (var i = 0; i < size; i++) converted.Add(stringList.Call<string>("get", i));

            return converted;
        }

        // from C#: List<string> to Java: ArrayList<String>
        internal static AndroidJavaObject ToJavaStringList(List<string> list)
        {
            var converted = new AndroidJavaObject("java.util.ArrayList");
            for (var i = 0; i < list.Count; i++) converted.Call<bool>("add", list[i]);

            return converted;
        }

        internal static FriendsListVisibilityStatus ToFriendsListVisibilityStatus(int playerListVisibility)
        {
            switch (playerListVisibility)
            {
                case /* FriendsListVisibilityStatus.UNKNOWN */ 0:
                    return FriendsListVisibilityStatus.Unknown;
                case /* FriendsListVisibilityStatus.VISIBLE */ 1:
                    return FriendsListVisibilityStatus.Visible;
                case /* FriendsListVisibilityStatus.REQUEST_REQUIRED */ 2:
                    return FriendsListVisibilityStatus.ResolutionRequired;
                case /* FriendsListVisibilityStatus.FEATURE_UNAVAILABLE */ 3:
                    return FriendsListVisibilityStatus.Unavailable;
                default:
                    return FriendsListVisibilityStatus.Unknown;
            }
        }

        internal static IUserProfile[] playersBufferToArray(AndroidJavaObject playersBuffer)
        {
            var count = playersBuffer.Call<int>("getCount");
            var users = new IUserProfile[count];
            for (var i = 0; i < count; ++i)
                using (var player = playersBuffer.Call<AndroidJavaObject>("get", i))
                {
                    users[i] = ToPlayerProfile(player);
                }

            playersBuffer.Call("release");
            return users;
        }
    }
}
#endif