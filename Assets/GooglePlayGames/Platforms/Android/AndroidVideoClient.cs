using System;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.Video;
using GooglePlayGames.OurUtils;
using UnityEngine;

#if UNITY_ANDROID
#pragma warning disable 0642 // Possible mistaken empty statement

namespace GooglePlayGames.Android
{
    internal class AndroidVideoClient : IVideoClient
    {
        readonly bool mIsCaptureSupported;
        OnCaptureOverlayStateListenerProxy mOnCaptureOverlayStateListenerProxy;
        volatile AndroidJavaObject mVideosClient;

        public AndroidVideoClient(bool isCaptureSupported, AndroidJavaObject account)
        {
            mIsCaptureSupported = isCaptureSupported;
            using (var gamesClass = new AndroidJavaClass("com.google.android.gms.games.Games"))
            {
                mVideosClient = gamesClass.CallStatic<AndroidJavaObject>("getVideosClient",
                    AndroidHelperFragment.GetActivity(), account);
            }
        }

        public void GetCaptureCapabilities(Action<ResponseStatus, VideoCapabilities> callback)
        {
            callback = ToOnGameThread(callback);
            using (var task = mVideosClient.Call<AndroidJavaObject>("getCaptureCapabilities"))
            {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                    task,
                    videoCapabilities => callback(ResponseStatus.Success, CreateVideoCapabilities(videoCapabilities)));

                AndroidTaskUtils.AddOnFailureListener(
                    task,
                    exception => callback(ResponseStatus.InternalError, null));
            }
        }

        public void ShowCaptureOverlay()
        {
            AndroidHelperFragment.ShowCaptureOverlayUI();
        }

        public void GetCaptureState(Action<ResponseStatus, VideoCaptureState> callback)
        {
            callback = ToOnGameThread(callback);
            using (var task = mVideosClient.Call<AndroidJavaObject>("getCaptureState"))
            {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                    task,
                    captureState =>
                        callback(ResponseStatus.Success, CreateVideoCaptureState(captureState)));

                AndroidTaskUtils.AddOnFailureListener(
                    task,
                    exception => callback(ResponseStatus.InternalError, null));
            }
        }

        public void IsCaptureAvailable(VideoCaptureMode captureMode, Action<ResponseStatus, bool> callback)
        {
            callback = ToOnGameThread(callback);
            using (var task =
                mVideosClient.Call<AndroidJavaObject>("isCaptureAvailable", ToVideoCaptureMode(captureMode)))
            {
                AndroidTaskUtils.AddOnSuccessListener<bool>(
                    task,
                    isCaptureAvailable => callback(ResponseStatus.Success, isCaptureAvailable));

                AndroidTaskUtils.AddOnFailureListener(
                    task,
                    exception => callback(ResponseStatus.InternalError, false));
            }
        }

        public bool IsCaptureSupported()
        {
            return mIsCaptureSupported;
        }

        public void RegisterCaptureOverlayStateChangedListener(CaptureOverlayStateListener listener)
        {
            if (mOnCaptureOverlayStateListenerProxy != null) UnregisterCaptureOverlayStateChangedListener();

            mOnCaptureOverlayStateListenerProxy = new OnCaptureOverlayStateListenerProxy(listener);
            using (mVideosClient.Call<AndroidJavaObject>("registerOnCaptureOverlayStateChangedListener",
                mOnCaptureOverlayStateListenerProxy))
            {
                ;
            }
        }

        public void UnregisterCaptureOverlayStateChangedListener()
        {
            if (mOnCaptureOverlayStateListenerProxy != null)
            {
                using (mVideosClient.Call<AndroidJavaObject>("unregisterOnCaptureOverlayStateChangedListener",
                    mOnCaptureOverlayStateListenerProxy))
                {
                    ;
                }

                mOnCaptureOverlayStateListenerProxy = null;
            }
        }

        static Action<T1, T2> ToOnGameThread<T1, T2>(Action<T1, T2> toConvert)
        {
            return (val1, val2) => PlayGamesHelperObject.RunOnGameThread(() => toConvert(val1, val2));
        }

        static VideoQualityLevel FromVideoQualityLevel(int captureQualityJava)
        {
            switch (captureQualityJava)
            {
                case 0: // QUALITY_LEVEL_SD
                    return VideoQualityLevel.SD;
                case 1: // QUALITY_LEVEL_HD
                    return VideoQualityLevel.HD;
                case 2: // QUALITY_LEVEL_XHD
                    return VideoQualityLevel.XHD;
                case 3: // QUALITY_LEVEL_FULLHD
                    return VideoQualityLevel.FullHD;
                default:
                    return VideoQualityLevel.Unknown;
            }
        }

        static VideoCaptureMode FromVideoCaptureMode(int captureMode)
        {
            switch (captureMode)
            {
                case 0: // CAPTURE_MODE_FILE
                    return VideoCaptureMode.File;
                case 1: // CAPTURE_MODE_STREAM
                    return VideoCaptureMode.Stream;
                default:
                    return VideoCaptureMode.Unknown;
            }
        }

        static int ToVideoCaptureMode(VideoCaptureMode captureMode)
        {
            switch (captureMode)
            {
                case VideoCaptureMode.File:
                    return 0; // CAPTURE_MODE_FILE
                case VideoCaptureMode.Stream:
                    return 1; // CAPTURE_MODE_STREAM
                default:
                    return -1; // CAPTURE_MODE_UNKNOWN
            }
        }

        static VideoCaptureState CreateVideoCaptureState(AndroidJavaObject videoCaptureState)
        {
            var isCapturing = videoCaptureState.Call<bool>("isCapturing");
            var captureMode = FromVideoCaptureMode(videoCaptureState.Call<int>("getCaptureMode"));
            var qualityLevel = FromVideoQualityLevel(videoCaptureState.Call<int>("getCaptureQuality"));
            var isOverlayVisible = videoCaptureState.Call<bool>("isOverlayVisible");
            var isPaused = videoCaptureState.Call<bool>("isPaused");

            return new VideoCaptureState(isCapturing, captureMode,
                qualityLevel, isOverlayVisible, isPaused);
        }

        static VideoCapabilities CreateVideoCapabilities(AndroidJavaObject videoCapabilities)
        {
            var isCameraSupported = videoCapabilities.Call<bool>("isCameraSupported");
            var isMicSupported = videoCapabilities.Call<bool>("isMicSupported");
            var isWriteStorageSupported = videoCapabilities.Call<bool>("isWriteStorageSupported");
            var captureModesSupported = videoCapabilities.Call<bool[]>("getSupportedCaptureModes");
            var qualityLevelsSupported = videoCapabilities.Call<bool[]>("getSupportedQualityLevels");

            return new VideoCapabilities(isCameraSupported, isMicSupported, isWriteStorageSupported,
                captureModesSupported, qualityLevelsSupported);
        }

        class OnCaptureOverlayStateListenerProxy : AndroidJavaProxy
        {
            readonly CaptureOverlayStateListener mListener;

            public OnCaptureOverlayStateListenerProxy(CaptureOverlayStateListener listener)
                : base("com/google/android/gms/games/VideosClient$OnCaptureOverlayStateListener")
            {
                mListener = listener;
            }

            public void onCaptureOverlayStateChanged(int overlayState)
            {
                PlayGamesHelperObject.RunOnGameThread(() =>
                    mListener.OnCaptureOverlayStateChanged(FromVideoCaptureOverlayState(overlayState))
                );
            }

            static VideoCaptureOverlayState FromVideoCaptureOverlayState(int overlayState)
            {
                switch (overlayState)
                {
                    case 1: // CAPTURE_OVERLAY_STATE_SHOWN
                        return VideoCaptureOverlayState.Shown;
                    case 2: // CAPTURE_OVERLAY_STATE_CAPTURE_STARTED
                        return VideoCaptureOverlayState.Started;
                    case 3: // CAPTURE_OVERLAY_STATE_CAPTURE_STOPPED
                        return VideoCaptureOverlayState.Stopped;
                    case 4: // CAPTURE_OVERLAY_STATE_DISMISSED
                        return VideoCaptureOverlayState.Dismissed;
                    default:
                        return VideoCaptureOverlayState.Unknown;
                }
            }
        }
    }
}
#endif