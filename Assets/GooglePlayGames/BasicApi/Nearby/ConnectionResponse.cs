// <copyright file="ConnectionResponse.cs" company="Google Inc.">
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
    public struct ConnectionResponse
    {
        static readonly byte[] EmptyPayload = new byte[0];

        public enum Status
        {
            Accepted,
            Rejected,
            ErrorInternal,
            ErrorNetworkNotConnected,
            ErrorEndpointNotConnected,
            ErrorAlreadyConnected
        }

        ConnectionResponse(long localClientId, string remoteEndpointId, Status code,
            byte[] payload)
        {
            LocalClientId = localClientId;
            RemoteEndpointId = Misc.CheckNotNull(remoteEndpointId);
            ResponseStatus = code;
            Payload = Misc.CheckNotNull(payload);
        }

        public long LocalClientId { get; }

        public string RemoteEndpointId { get; }

        public Status ResponseStatus { get; }

        public byte[] Payload { get; }

        public static ConnectionResponse Rejected(long localClientId, string remoteEndpointId)
        {
            return new ConnectionResponse(localClientId, remoteEndpointId, Status.Rejected,
                EmptyPayload);
        }

        public static ConnectionResponse NetworkNotConnected(long localClientId, string remoteEndpointId)
        {
            return new ConnectionResponse(localClientId, remoteEndpointId, Status.ErrorNetworkNotConnected,
                EmptyPayload);
        }

        public static ConnectionResponse InternalError(long localClientId, string remoteEndpointId)
        {
            return new ConnectionResponse(localClientId, remoteEndpointId, Status.ErrorInternal,
                EmptyPayload);
        }

        public static ConnectionResponse EndpointNotConnected(long localClientId, string remoteEndpointId)
        {
            return new ConnectionResponse(localClientId, remoteEndpointId, Status.ErrorEndpointNotConnected,
                EmptyPayload);
        }

        public static ConnectionResponse Accepted(long localClientId, string remoteEndpointId,
            byte[] payload)
        {
            return new ConnectionResponse(localClientId, remoteEndpointId, Status.Accepted,
                payload);
        }

        public static ConnectionResponse AlreadyConnected(long localClientId,
            string remoteEndpointId)
        {
            return new ConnectionResponse(localClientId, remoteEndpointId,
                Status.ErrorAlreadyConnected,
                EmptyPayload);
        }
    }
}