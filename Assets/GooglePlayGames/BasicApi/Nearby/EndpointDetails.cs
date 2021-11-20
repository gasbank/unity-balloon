// <copyright file="EndpointDetails.cs" company="Google Inc.">
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

using GooglePlayGames.OurUtils;

namespace GooglePlayGames.BasicApi.Nearby
{
    public struct EndpointDetails
    {
        public EndpointDetails(string endpointId, string name, string serviceId)
        {
            EndpointId = Misc.CheckNotNull(endpointId);
            Name = Misc.CheckNotNull(name);
            ServiceId = Misc.CheckNotNull(serviceId);
        }

        public string EndpointId { get; }

        public string Name { get; }

        public string ServiceId { get; }
    }
}