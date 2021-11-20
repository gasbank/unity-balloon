// <copyright file="PlayGamesLeaderboard.cs" company="Google Inc.">
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

using System;
using System.Collections.Generic;
using GooglePlayGames.BasicApi;
using GooglePlayGames.OurUtils;
using UnityEngine.SocialPlatforms;

#if UNITY_ANDROID

namespace GooglePlayGames
{
    public class PlayGamesLeaderboard : ILeaderboard
    {
        string[] mFilteredUserIds;

        Range mRange;
        readonly List<PlayGamesScore> mScoreList = new List<PlayGamesScore>();

        public PlayGamesLeaderboard(string id)
        {
            this.id = id;
        }

        public int ScoreCount => mScoreList.Count;

        internal bool SetFromData(LeaderboardScoreData data)
        {
            if (data.Valid)
            {
                Logger.d("Setting leaderboard from: " + data);
                SetMaxRange(data.ApproximateCount);
                SetTitle(data.Title);
                SetLocalUserScore((PlayGamesScore) data.PlayerScore);
                foreach (var score in data.Scores) AddScore((PlayGamesScore) score);

                loading = data.Scores.Length == 0 || HasAllScores();
            }

            return data.Valid;
        }

        internal void SetMaxRange(ulong val)
        {
            maxRange = (uint) val;
        }

        internal void SetTitle(string value)
        {
            title = value;
        }

        internal void SetLocalUserScore(PlayGamesScore score)
        {
            localUserScore = score;
        }

        internal int AddScore(PlayGamesScore score)
        {
            if (mFilteredUserIds == null || mFilteredUserIds.Length == 0)
                mScoreList.Add(score);
            else
                foreach (var fid in mFilteredUserIds)
                    if (fid.Equals(score.userID))
                    {
                        mScoreList.Add(score);
                        break;
                    }

            return mScoreList.Count;
        }

        internal bool HasAllScores()
        {
            return mScoreList.Count >= mRange.count || mScoreList.Count >= maxRange;
        }

        #region ILeaderboard implementation

        public void SetUserFilter(string[] userIDs)
        {
            mFilteredUserIds = userIDs;
        }

        public void LoadScores(Action<bool> callback)
        {
            PlayGamesPlatform.Instance.LoadScores(this, callback);
        }

        public bool loading { get; internal set; }

        public string id { get; set; }

        public UserScope userScope { get; set; }

        public Range range
        {
            get => mRange;
            set => mRange = value;
        }

        public TimeScope timeScope { get; set; }

        public IScore localUserScore { get; set; }

        public uint maxRange { get; set; }

        public IScore[] scores
        {
            get
            {
                var arr = new PlayGamesScore[mScoreList.Count];
                mScoreList.CopyTo(arr);
                return arr;
            }
        }

        public string title { get; set; }

        #endregion
    }
}
#endif