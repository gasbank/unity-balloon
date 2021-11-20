// <copyright file="LeaderboardScoreData.cs" company="Google Inc.">
// Copyright (C) 2015 Google Inc. All Rights Reserved.
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
using UnityEngine.SocialPlatforms;

#if UNITY_ANDROID

namespace GooglePlayGames.BasicApi
{
    /// <summary>
    ///     Leaderboard score data. This is the callback data
    ///     when loading leaderboard scores.  There are several SDK
    ///     API calls needed to be made to collect all the required data,
    ///     so this class is used to simplify the response.
    /// </summary>
    public class LeaderboardScoreData
    {
        readonly List<PlayGamesScore> mScores = new List<PlayGamesScore>();

        internal LeaderboardScoreData(string leaderboardId)
        {
            Id = leaderboardId;
        }

        internal LeaderboardScoreData(string leaderboardId, ResponseStatus status)
        {
            Id = leaderboardId;
            Status = status;
        }

        public bool Valid =>
            Status == ResponseStatus.Success ||
            Status == ResponseStatus.SuccessWithStale;

        public ResponseStatus Status { get; internal set; }

        public ulong ApproximateCount { get; internal set; }

        public string Title { get; internal set; }

        public string Id { get; internal set; }

        public IScore PlayerScore { get; internal set; }

        public IScore[] Scores => mScores.ToArray();

        public ScorePageToken PrevPageToken { get; internal set; }

        public ScorePageToken NextPageToken { get; internal set; }

        internal int AddScore(PlayGamesScore score)
        {
            mScores.Add(score);
            return mScores.Count;
        }

        public override string ToString()
        {
            return string.Format("[LeaderboardScoreData: mId={0}, " +
                                 " mStatus={1}, mApproxCount={2}, mTitle={3}]",
                Id, Status, ApproximateCount, Title);
        }
    }
}
#endif