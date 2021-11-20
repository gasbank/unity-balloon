// <copyright file="DummyNearbyConnectionClient.cs" company="Google Inc.">
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
using System.Collections.Generic;
using GooglePlayGames.OurUtils;

#if UNITY_ANDROID

namespace GooglePlayGames.BasicApi.Nearby
{
    public class DummyNearbyConnectionClient : INearbyConnectionClient
    {
        public int MaxUnreliableMessagePayloadLength()
        {
            return NearbyConnectionConfiguration.MaxUnreliableMessagePayloadLength;
        }

        public int MaxReliableMessagePayloadLength()
        {
            return NearbyConnectionConfiguration.MaxReliableMessagePayloadLength;
        }

        public void SendReliable(List<string> recipientEndpointIds, byte[] payload)
        {
            Logger.d("SendReliable called from dummy implementation");
        }

        public void SendUnreliable(List<string> recipientEndpointIds, byte[] payload)
        {
            Logger.d("SendUnreliable called from dummy implementation");
        }

        public void StartAdvertising(string name, List<string> appIdentifiers,
            TimeSpan? advertisingDuration, Action<AdvertisingResult> resultCallback,
            Action<ConnectionRequest> connectionRequestCallback)
        {
            var obj = new AdvertisingResult(ResponseStatus.LicenseCheckFailed, string.Empty);
            resultCallback.Invoke(obj);
        }

        public void StopAdvertising()
        {
            Logger.d("StopAvertising in dummy implementation called");
        }

        public void SendConnectionRequest(string name, string remoteEndpointId, byte[] payload,
            Action<ConnectionResponse> responseCallback, IMessageListener listener)
        {
            Logger.d("SendConnectionRequest called from dummy implementation");

            if (responseCallback != null)
            {
                var obj = ConnectionResponse.Rejected(0, string.Empty);
                responseCallback.Invoke(obj);
            }
        }

        public void AcceptConnectionRequest(string remoteEndpointId, byte[] payload, IMessageListener listener)
        {
            Logger.d("AcceptConnectionRequest in dummy implementation called");
        }

        public void StartDiscovery(string serviceId, TimeSpan? advertisingTimeout, IDiscoveryListener listener)
        {
            Logger.d("StartDiscovery in dummy implementation called");
        }

        public void StopDiscovery(string serviceId)
        {
            Logger.d("StopDiscovery in dummy implementation called");
        }

        public void RejectConnectionRequest(string requestingEndpointId)
        {
            Logger.d("RejectConnectionRequest in dummy implementation called");
        }

        public void DisconnectFromEndpoint(string remoteEndpointId)
        {
            Logger.d("DisconnectFromEndpoint in dummy implementation called");
        }

        public void StopAllConnections()
        {
            Logger.d("StopAllConnections in dummy implementation called");
        }

        public string GetAppBundleId()
        {
            return "dummy.bundle.id";
        }

        public string GetServiceId()
        {
            return "dummy.service.id";
        }

        public string LocalEndpointId()
        {
            return string.Empty;
        }

        public string LocalDeviceId()
        {
            return "DummyDevice";
        }
    }
}
#endif