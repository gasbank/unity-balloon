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

using System;
using System.Collections.Generic;
using GoogleMobileAds.Api.Mediation;

namespace GoogleMobileAds.Api
{
    public class AdRequest
    {
        public const string Version = "3.17.0";
        public const string TestDeviceSimulator = "SIMULATOR";

        AdRequest(Builder builder)
        {
            TestDevices = new List<string>(builder.TestDevices);
            Keywords = new HashSet<string>(builder.Keywords);
            Birthday = builder.Birthday;
            Gender = builder.Gender;
            TagForChildDirectedTreatment = builder.ChildDirectedTreatmentTag;
            Extras = new Dictionary<string, string>(builder.Extras);
            MediationExtras = builder.MediationExtras;
        }

        public List<string> TestDevices { get; }

        public HashSet<string> Keywords { get; }

        public DateTime? Birthday { get; }

        public Gender? Gender { get; }

        public bool? TagForChildDirectedTreatment { get; }

        public Dictionary<string, string> Extras { get; }

        public List<MediationExtras> MediationExtras { get; }

        public class Builder
        {
            public Builder()
            {
                TestDevices = new List<string>();
                Keywords = new HashSet<string>();
                Birthday = null;
                Gender = null;
                ChildDirectedTreatmentTag = null;
                Extras = new Dictionary<string, string>();
                MediationExtras = new List<MediationExtras>();
            }

            internal List<string> TestDevices { get; }

            internal HashSet<string> Keywords { get; }

            internal DateTime? Birthday { get; private set; }

            internal Gender? Gender { get; private set; }

            internal bool? ChildDirectedTreatmentTag { get; private set; }

            internal Dictionary<string, string> Extras { get; }

            internal List<MediationExtras> MediationExtras { get; }

            public Builder AddKeyword(string keyword)
            {
                Keywords.Add(keyword);
                return this;
            }

            public Builder AddTestDevice(string deviceId)
            {
                TestDevices.Add(deviceId);
                return this;
            }

            public AdRequest Build()
            {
                return new AdRequest(this);
            }

            public Builder SetBirthday(DateTime birthday)
            {
                Birthday = birthday;
                return this;
            }

            public Builder SetGender(Gender gender)
            {
                Gender = gender;
                return this;
            }

            public Builder AddMediationExtras(MediationExtras extras)
            {
                MediationExtras.Add(extras);
                return this;
            }

            public Builder TagForChildDirectedTreatment(bool tagForChildDirectedTreatment)
            {
                ChildDirectedTreatmentTag = tagForChildDirectedTreatment;
                return this;
            }

            public Builder AddExtra(string key, string value)
            {
                Extras.Add(key, value);
                return this;
            }
        }
    }
}