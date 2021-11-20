// <copyright file="Achievement.cs" company="Google Inc.">
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

#if UNITY_ANDROID

namespace GooglePlayGames.BasicApi
{
    /// <summary>Data interface for retrieving achievement information.</summary>
    /// <remarks>
    ///     There are 3 states an achievement can be in:
    ///     <para>
    ///         Hidden - indicating the name and description of the achievement is
    ///         not visible to the player.
    ///     </para>
    ///     <para>
    ///         Revealed - indicating the name and description of the achievement is
    ///         visible to the player.
    ///         Unlocked - indicating the player has unlocked, or achieved, the achievment.
    ///     </para>
    ///     <para>
    ///         Achievements has two types, standard which is unlocked in one step,
    ///         and incremental, which require multiple steps to unlock.
    ///     </para>
    /// </remarks>
    public class Achievement
    {
        static readonly DateTime UnixEpoch =
            new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        long mLastModifiedTime;

        /// <summary>
        ///     Indicates whether this achievement is incremental.
        /// </summary>
        public bool IsIncremental { get; set; }

        /// <summary>
        ///     The number of steps the user has gone towards unlocking this achievement.
        /// </summary>
        public int CurrentSteps { get; set; }

        /// <summary>
        ///     The total number of steps needed to unlock this achievement.
        /// </summary>
        public int TotalSteps { get; set; }

        /// <summary>
        ///     Indicates whether the achievement is unlocked or not.
        /// </summary>
        public bool IsUnlocked { get; set; }

        /// <summary>
        ///     Indicates whether the achievement is revealed or not (hidden).
        /// </summary>
        public bool IsRevealed { get; set; }

        /// <summary>
        ///     The ID string of this achievement.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        ///     The description of this achievement.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        ///     The name of this achievement.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        ///     The date and time the state of the achievement was modified.
        /// </summary>
        /// <remarks>
        ///     The value is invalid (-1 long) if the achievement state has
        ///     never been updated.
        /// </remarks>
        public DateTime LastModifiedTime
        {
            get => UnixEpoch.AddMilliseconds(mLastModifiedTime);

            set
            {
                var ts = value - UnixEpoch;
                mLastModifiedTime = (long) ts.TotalMilliseconds;
            }
        }

        /// <summary>
        ///     The number of experience points earned for unlocking this Achievement.
        /// </summary>
        public ulong Points { get; set; }

        /// <summary>
        ///     The URL to the image to display when the achievement is revealed.
        /// </summary>
        public string RevealedImageUrl { get; set; }

        /// <summary>
        ///     The URL to the image to display when the achievement is unlocked.
        /// </summary>
        public string UnlockedImageUrl { get; set; }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents the current
        ///     <see cref="GooglePlayGames.BasicApi.Achievement" />.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents the current <see cref="GooglePlayGames.BasicApi.Achievement" />
        ///     .
        /// </returns>
        public override string ToString()
        {
            return string.Format(
                "[Achievement] id={0}, name={1}, desc={2}, type={3}, revealed={4}, unlocked={5}, steps={6}/{7}",
                Id, Name, Description, IsIncremental ? "INCREMENTAL" : "STANDARD",
                IsRevealed, IsUnlocked, CurrentSteps, TotalSteps);
        }
    }
}
#endif