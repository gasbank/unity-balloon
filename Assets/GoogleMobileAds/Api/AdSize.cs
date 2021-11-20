// Copyright (C) 2015 Google, Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace GoogleMobileAds.Api
{
    public class AdSize
    {
        public static readonly AdSize Banner = new AdSize(320, 50);
        public static readonly AdSize MediumRectangle = new AdSize(300, 250);
        public static readonly AdSize IABBanner = new AdSize(468, 60);
        public static readonly AdSize Leaderboard = new AdSize(728, 90);
        public static readonly AdSize SmartBanner = new AdSize(true);
        public static readonly int FullWidth = -1;

        public AdSize(int width, int height)
        {
            IsSmartBanner = false;
            Width = width;
            Height = height;
        }

        AdSize(bool isSmartBanner) : this(0, 0)
        {
            IsSmartBanner = isSmartBanner;
        }

        public int Width { get; }

        public int Height { get; }

        public bool IsSmartBanner { get; }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var other = (AdSize) obj;
            return Width == other.Width && Height == other.Height && IsSmartBanner == other.IsSmartBanner;
        }

        public static bool operator ==(AdSize a, AdSize b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(AdSize a, AdSize b)
        {
            return !a.Equals(b);
        }

        public override int GetHashCode()
        {
            var hashBase = 71;
            var hashMultiplier = 11;

            var hash = hashBase;
            hash = (hash * hashMultiplier) ^ Width.GetHashCode();
            hash = (hash * hashMultiplier) ^ Height.GetHashCode();
            hash = (hash * hashMultiplier) ^ IsSmartBanner.GetHashCode();
            return hash;
        }
    }
}