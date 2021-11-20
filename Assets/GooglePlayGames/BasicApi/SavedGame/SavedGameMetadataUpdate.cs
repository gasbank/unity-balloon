// <copyright file="SavedGameMetadataUpdate.cs" company="Google Inc.">
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
using GooglePlayGames.OurUtils;

namespace GooglePlayGames.BasicApi.SavedGame
{
    /// <summary>
    ///     A struct representing the mutation of saved game metadata. Fields can either have a new value
    ///     or be untouched (in which case the corresponding field in the saved game metadata will be
    ///     untouched). Instances must be built using <see cref="SavedGameMetadataUpdate.Builder" />
    ///     and once created, these instances are immutable and threadsafe.
    /// </summary>
    public struct SavedGameMetadataUpdate
    {
        SavedGameMetadataUpdate(Builder builder)
        {
            IsDescriptionUpdated = builder.mDescriptionUpdated;
            UpdatedDescription = builder.mNewDescription;
            IsCoverImageUpdated = builder.mCoverImageUpdated;
            UpdatedPngCoverImage = builder.mNewPngCoverImage;
            UpdatedPlayedTime = builder.mNewPlayedTime;
        }

        public bool IsDescriptionUpdated { get; }

        public string UpdatedDescription { get; }

        public bool IsCoverImageUpdated { get; }

        public byte[] UpdatedPngCoverImage { get; }

        public bool IsPlayedTimeUpdated => UpdatedPlayedTime.HasValue;

        public TimeSpan? UpdatedPlayedTime { get; }

        public struct Builder
        {
            internal bool mDescriptionUpdated;
            internal string mNewDescription;
            internal bool mCoverImageUpdated;
            internal byte[] mNewPngCoverImage;
            internal TimeSpan? mNewPlayedTime;

            public Builder WithUpdatedDescription(string description)
            {
                mNewDescription = Misc.CheckNotNull(description);
                mDescriptionUpdated = true;
                return this;
            }

            public Builder WithUpdatedPngCoverImage(byte[] newPngCoverImage)
            {
                mCoverImageUpdated = true;
                mNewPngCoverImage = newPngCoverImage;
                return this;
            }

            public Builder WithUpdatedPlayedTime(TimeSpan newPlayedTime)
            {
                if (newPlayedTime.TotalMilliseconds > ulong.MaxValue)
                    throw new InvalidOperationException("Timespans longer than ulong.MaxValue " +
                                                        "milliseconds are not allowed");

                mNewPlayedTime = newPlayedTime;
                return this;
            }

            public SavedGameMetadataUpdate Build()
            {
                return new SavedGameMetadataUpdate(this);
            }
        }
    }
}