// <copyright file="PlayGamesScore.cs" company="Google Inc.">
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

using System;
using UnityEngine.SocialPlatforms;

#if UNITY_ANDROID

namespace GooglePlayGames
{
    /// <summary>
    ///     Represents a Google Play Games score that can be sent to a leaderboard.
    /// </summary>
    public class PlayGamesScore : IScore
    {
        readonly ulong mRank;

        internal PlayGamesScore(DateTime date, string leaderboardId,
            ulong rank, string playerId, ulong value, string metadata)
        {
            this.date = date;
            leaderboardID = leaderboardID;
            mRank = rank;
            userID = playerId;
            this.value = (long) value;
            metaData = metadata;
        }

        /// <summary>
        ///     Gets the metaData (scoreTag).
        /// </summary>
        /// <returns>
        ///     The metaData.
        /// </returns>
        public string metaData { get; } = string.Empty;

        /// <summary>
        ///     Reports the score. Equivalent to <see cref="PlayGamesPlatform.ReportScore" />.
        /// </summary>
        public void ReportScore(Action<bool> callback)
        {
            PlayGamesPlatform.Instance.ReportScore(value, leaderboardID, metaData, callback);
        }

        /// <summary>
        ///     Gets or sets the leaderboard id.
        /// </summary>
        /// <returns>
        ///     The leaderboard id.
        /// </returns>
        public string leaderboardID { get; set; }

        /// <summary>
        ///     Gets or sets the score value.
        /// </summary>
        /// <returns>
        ///     The value.
        /// </returns>
        public long value { get; set; }

        /// <summary>
        ///     Not implemented. Returns Jan 01, 1970, 00:00:00
        /// </summary>
        public DateTime date { get; } = new DateTime(1970, 1, 1, 0, 0, 0);

        /// <summary>
        ///     Not implemented. Returns the value converted to a string, unformatted.
        /// </summary>
        public string formattedValue => value.ToString();

        /// <summary>
        ///     Not implemented. Returns the empty string.
        /// </summary>
        public string userID { get; } = string.Empty;

        /// <summary>
        ///     Not implemented. Returns 1.
        /// </summary>
        public int rank => (int) mRank;
    }
}
#endif