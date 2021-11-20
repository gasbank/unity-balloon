using System;
using System.Collections.Generic;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.Nearby;
using GooglePlayGames.OurUtils;
using UnityEngine;
using Logger = GooglePlayGames.OurUtils.Logger;

#if UNITY_ANDROID
#pragma warning disable 0642 // Possible mistaken empty statement

namespace GooglePlayGames.Android
{
    public class AndroidNearbyConnectionClient : INearbyConnectionClient
    {
        static readonly long NearbyClientId = 0L;
        static readonly int ApplicationInfoFlags = 0x00000080;
        static readonly string ServiceId = ReadServiceId();
        protected IMessageListener mAdvertisingMessageListener;
        volatile AndroidJavaObject mClient;

        public AndroidNearbyConnectionClient()
        {
            PlayGamesHelperObject.CreateObject();
            NearbyHelperObject.CreateObject(this);
            using (var nearbyClass = new AndroidJavaClass("com.google.android.gms.nearby.Nearby"))
            {
                mClient = nearbyClass.CallStatic<AndroidJavaObject>("getConnectionsClient",
                    AndroidHelperFragment.GetActivity());
            }
        }

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
            InternalSend(recipientEndpointIds, payload);
        }

        public void SendUnreliable(List<string> recipientEndpointIds, byte[] payload)
        {
            InternalSend(recipientEndpointIds, payload);
        }

        public void StartAdvertising(string name, List<string> appIdentifiers,
            TimeSpan? advertisingDuration, Action<AdvertisingResult> resultCallback,
            Action<ConnectionRequest> connectionRequestCallback)
        {
            Misc.CheckNotNull(resultCallback, "resultCallback");
            Misc.CheckNotNull(connectionRequestCallback, "connectionRequestCallback");

            if (advertisingDuration.HasValue && advertisingDuration.Value.Ticks < 0)
                throw new InvalidOperationException("advertisingDuration must be positive");

            connectionRequestCallback = ToOnGameThread(connectionRequestCallback);
            resultCallback = ToOnGameThread(resultCallback);

            var callbackProxy =
                new AdvertisingConnectionLifecycleCallbackProxy(resultCallback, connectionRequestCallback, this);
            using (var connectionLifecycleCallback =
                new AndroidJavaObject("com.google.games.bridge.ConnectionLifecycleCallbackProxy", callbackProxy))
            using (var advertisingOptions = CreateAdvertisingOptions())
            using (var task = mClient.Call<AndroidJavaObject>("startAdvertising", name, GetServiceId(),
                connectionLifecycleCallback, advertisingOptions))
            {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                    task,
                    v => NearbyHelperObject.StartAdvertisingTimer(advertisingDuration)
                );
            }
        }

        public void StopAdvertising()
        {
            mClient.Call("stopAdvertising");
            mAdvertisingMessageListener = null;
        }

        public void SendConnectionRequest(string name, string remoteEndpointId, byte[] payload,
            Action<ConnectionResponse> responseCallback, IMessageListener listener)
        {
            Misc.CheckNotNull(listener, "listener");
            var listenerOnGameThread = new OnGameThreadMessageListener(listener);
            var cb =
                new DiscoveringConnectionLifecycleCallback(responseCallback, listenerOnGameThread, mClient);
            using (var connectionLifecycleCallback =
                new AndroidJavaObject("com.google.games.bridge.ConnectionLifecycleCallbackProxy", cb))
            using (mClient.Call<AndroidJavaObject>("requestConnection", name, remoteEndpointId,
                connectionLifecycleCallback))
            {
                ;
            }
        }

        public void AcceptConnectionRequest(string remoteEndpointId, byte[] payload, IMessageListener listener)
        {
            Misc.CheckNotNull(listener, "listener");
            mAdvertisingMessageListener = new OnGameThreadMessageListener(listener);

            using (var payloadCallback = new AndroidJavaObject("com.google.games.bridge.PayloadCallbackProxy",
                new PayloadCallback(listener)))
            using (mClient.Call<AndroidJavaObject>("acceptConnection", remoteEndpointId, payloadCallback))
            {
                ;
            }
        }

        public void StartDiscovery(string serviceId, TimeSpan? advertisingDuration,
            IDiscoveryListener listener)
        {
            Misc.CheckNotNull(serviceId, "serviceId");
            Misc.CheckNotNull(listener, "listener");

            var listenerOnGameThread = new OnGameThreadDiscoveryListener(listener);

            if (advertisingDuration.HasValue && advertisingDuration.Value.Ticks < 0)
                throw new InvalidOperationException("advertisingDuration must be positive");

            using (var endpointDiscoveryCallback = new AndroidJavaObject(
                "com.google.games.bridge.EndpointDiscoveryCallbackProxy",
                new EndpointDiscoveryCallback(listenerOnGameThread)))
            using (var discoveryOptions = CreateDiscoveryOptions())
            using (var task = mClient.Call<AndroidJavaObject>("startDiscovery", serviceId, endpointDiscoveryCallback,
                discoveryOptions))
            {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                    task,
                    v => NearbyHelperObject.StartDiscoveryTimer(advertisingDuration)
                );
            }
        }

        public void StopDiscovery(string serviceId)
        {
            mClient.Call("stopDiscovery");
        }

        public void RejectConnectionRequest(string requestingEndpointId)
        {
            Misc.CheckNotNull(requestingEndpointId, "requestingEndpointId");
            using (var task = mClient.Call<AndroidJavaObject>("rejectConnection", requestingEndpointId))
            {
                ;
            }
        }

        public void DisconnectFromEndpoint(string remoteEndpointId)
        {
            mClient.Call("disconnectFromEndpoint", remoteEndpointId);
        }

        public void StopAllConnections()
        {
            mClient.Call("stopAllEndpoints");
            mAdvertisingMessageListener = null;
        }

        public string GetAppBundleId()
        {
            using (var activity = AndroidHelperFragment.GetActivity())
            {
                return activity.Call<string>("getPackageName");
            }
        }

        public string GetServiceId()
        {
            return ServiceId;
        }

        void InternalSend(List<string> recipientEndpointIds, byte[] payload)
        {
            Misc.CheckNotNull(recipientEndpointIds);
            Misc.CheckNotNull(payload);

            using (var payloadClass = new AndroidJavaClass("com.google.android.gms.nearby.connection.Payload"))
            using (var payloadObject = payloadClass.CallStatic<AndroidJavaObject>("fromBytes", payload))
            using (var task = mClient.Call<AndroidJavaObject>("sendPayload",
                AndroidJavaConverter.ToJavaStringList(recipientEndpointIds),
                payloadObject))
            {
                ;
            }
        }

        AndroidJavaObject CreateAdvertisingOptions()
        {
            using (var strategy = new AndroidJavaClass("com.google.android.gms.nearby.connection.Strategy")
                .GetStatic<AndroidJavaObject>("P2P_CLUSTER"))
            using (var builder =
                new AndroidJavaObject("com.google.android.gms.nearby.connection.AdvertisingOptions$Builder"))
            using (builder.Call<AndroidJavaObject>("setStrategy", strategy))
            {
                return builder.Call<AndroidJavaObject>("build");
            }
        }

        AndroidJavaObject CreateDiscoveryOptions()
        {
            using (var strategy =
                new AndroidJavaClass("com.google.android.gms.nearby.connection.Strategy").GetStatic<AndroidJavaObject>(
                    "P2P_CLUSTER"))
            using (var builder =
                new AndroidJavaObject("com.google.android.gms.nearby.connection.DiscoveryOptions$Builder"))
            using (builder.Call<AndroidJavaObject>("setStrategy", strategy))
            {
                return builder.Call<AndroidJavaObject>("build");
            }
        }

        static string ReadServiceId()
        {
            using (var activity = AndroidHelperFragment.GetActivity())
            {
                var packageName = activity.Call<string>("getPackageName");
                using (var pm = activity.Call<AndroidJavaObject>("getPackageManager"))
                using (var appInfo =
                    pm.Call<AndroidJavaObject>("getApplicationInfo", packageName, ApplicationInfoFlags))
                using (var bundle = appInfo.Get<AndroidJavaObject>("metaData"))
                {
                    var sysId = bundle.Call<string>("getString",
                        "com.google.android.gms.nearby.connection.SERVICE_ID");
                    Logger.d("SystemId from Manifest: " + sysId);
                    return sysId;
                }
            }
        }

        static Action<T> ToOnGameThread<T>(Action<T> toConvert)
        {
            return val => PlayGamesHelperObject.RunOnGameThread(() => toConvert(val));
        }

        static Action<T1, T2> ToOnGameThread<T1, T2>(Action<T1, T2> toConvert)
        {
            return (val1, val2) => PlayGamesHelperObject.RunOnGameThread(() => toConvert(val1, val2));
        }

        class AdvertisingConnectionLifecycleCallbackProxy : AndroidJavaProxy
        {
            readonly AndroidNearbyConnectionClient mClient;
            readonly Action<ConnectionRequest> mConnectionRequestCallback;
            string mLocalEndpointName;
            readonly Action<AdvertisingResult> mResultCallback;

            public AdvertisingConnectionLifecycleCallbackProxy(Action<AdvertisingResult> resultCallback,
                Action<ConnectionRequest> connectionRequestCallback, AndroidNearbyConnectionClient client) : base(
                "com/google/games/bridge/ConnectionLifecycleCallbackProxy$Callback")
            {
                mResultCallback = resultCallback;
                mConnectionRequestCallback = connectionRequestCallback;
                mClient = client;
            }

            public void onConnectionInitiated(string endpointId, AndroidJavaObject connectionInfo)
            {
                mLocalEndpointName = connectionInfo.Call<string>("getEndpointName");
                mConnectionRequestCallback(new ConnectionRequest(endpointId, mLocalEndpointName, mClient.GetServiceId(),
                    new byte[0]));
            }

            public void onConnectionResult(string endpointId, AndroidJavaObject connectionResolution)
            {
                int statusCode;
                using (var status = connectionResolution.Call<AndroidJavaObject>("getStatus"))
                {
                    statusCode = status.Call<int>("getStatusCode");
                }

                if (statusCode == 0) // STATUS_OK
                {
                    mResultCallback(new AdvertisingResult(ResponseStatus.Success, mLocalEndpointName));
                    return;
                }

                if (statusCode == 8001) // STATUS_ALREADY_ADVERTISING
                {
                    mResultCallback(new AdvertisingResult(ResponseStatus.NotAuthorized, mLocalEndpointName));
                    return;
                }

                mResultCallback(new AdvertisingResult(ResponseStatus.InternalError, mLocalEndpointName));
            }

            public void onDisconnected(string endpointId)
            {
                if (mClient.mAdvertisingMessageListener != null)
                    mClient.mAdvertisingMessageListener.OnRemoteEndpointDisconnected(endpointId);
            }
        }

        class PayloadCallback : AndroidJavaProxy
        {
            readonly IMessageListener mListener;

            public PayloadCallback(IMessageListener listener) : base(
                "com/google/games/bridge/PayloadCallbackProxy$Callback")
            {
                mListener = listener;
            }

            public void onPayloadReceived(string endpointId, AndroidJavaObject payload)
            {
                if (payload.Call<int>("getType") != 1) // 1 for BYTES
                    return;

                mListener.OnMessageReceived(endpointId, payload.Call<byte[]>("asBytes"), /* isReliableMessage */ true);
            }
        }

        class DiscoveringConnectionLifecycleCallback : AndroidJavaProxy
        {
            readonly AndroidJavaObject mClient;
            readonly IMessageListener mListener;
            readonly Action<ConnectionResponse> mResponseCallback;

            public DiscoveringConnectionLifecycleCallback(Action<ConnectionResponse> responseCallback,
                IMessageListener listener, AndroidJavaObject client) : base(
                "com/google/games/bridge/ConnectionLifecycleCallbackProxy$Callback")
            {
                mResponseCallback = responseCallback;
                mListener = listener;
                mClient = client;
            }

            public void onConnectionInitiated(string endpointId, AndroidJavaObject connectionInfo)
            {
                using (var payloadCallback = new AndroidJavaObject("com.google.games.bridge.PayloadCallbackProxy",
                    new PayloadCallback(mListener)))
                using (mClient.Call<AndroidJavaObject>("acceptConnection", endpointId, payloadCallback))
                {
                    ;
                }
            }

            public void onConnectionResult(string endpointId, AndroidJavaObject connectionResolution)
            {
                int statusCode;
                using (var status = connectionResolution.Call<AndroidJavaObject>("getStatus"))
                {
                    statusCode = status.Call<int>("getStatusCode");
                }

                if (statusCode == 0) // STATUS_OK
                {
                    mResponseCallback(ConnectionResponse.Accepted(NearbyClientId, endpointId, new byte[0]));
                    return;
                }

                if (statusCode == 8002) // STATUS_ALREADY_DISCOVERING
                {
                    mResponseCallback(ConnectionResponse.AlreadyConnected(NearbyClientId, endpointId));
                    return;
                }

                mResponseCallback(ConnectionResponse.Rejected(NearbyClientId, endpointId));
            }

            public void onDisconnected(string endpointId)
            {
                mListener.OnRemoteEndpointDisconnected(endpointId);
            }
        }

        class EndpointDiscoveryCallback : AndroidJavaProxy
        {
            readonly IDiscoveryListener mListener;

            public EndpointDiscoveryCallback(IDiscoveryListener listener) : base(
                "com/google/games/bridge/EndpointDiscoveryCallbackProxy$Callback")
            {
                mListener = listener;
            }

            public void onEndpointFound(string endpointId, AndroidJavaObject endpointInfo)
            {
                mListener.OnEndpointFound(CreateEndPointDetails(endpointId, endpointInfo));
            }

            public void onEndpointLost(string endpointId)
            {
                mListener.OnEndpointLost(endpointId);
            }

            EndpointDetails CreateEndPointDetails(string endpointId, AndroidJavaObject endpointInfo)
            {
                return new EndpointDetails(
                    endpointId,
                    endpointInfo.Call<string>("getEndpointName"),
                    endpointInfo.Call<string>("getServiceId")
                );
            }
        }

        class OnGameThreadMessageListener : IMessageListener
        {
            readonly IMessageListener mListener;

            public OnGameThreadMessageListener(IMessageListener listener)
            {
                mListener = Misc.CheckNotNull(listener);
            }

            public void OnMessageReceived(string remoteEndpointId, byte[] data,
                bool isReliableMessage)
            {
                PlayGamesHelperObject.RunOnGameThread(() => mListener.OnMessageReceived(
                    remoteEndpointId, data, isReliableMessage));
            }

            public void OnRemoteEndpointDisconnected(string remoteEndpointId)
            {
                PlayGamesHelperObject.RunOnGameThread(
                    () => mListener.OnRemoteEndpointDisconnected(remoteEndpointId));
            }
        }

        class OnGameThreadDiscoveryListener : IDiscoveryListener
        {
            readonly IDiscoveryListener mListener;

            public OnGameThreadDiscoveryListener(IDiscoveryListener listener)
            {
                mListener = listener;
            }

            public void OnEndpointFound(EndpointDetails discoveredEndpoint)
            {
                PlayGamesHelperObject.RunOnGameThread(() => mListener.OnEndpointFound(discoveredEndpoint));
            }

            public void OnEndpointLost(string lostEndpointId)
            {
                PlayGamesHelperObject.RunOnGameThread(() => mListener.OnEndpointLost(lostEndpointId));
            }
        }
    }
}
#endif