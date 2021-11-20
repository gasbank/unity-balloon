using System;
using GooglePlayGames.BasicApi.SavedGame;
using UnityEngine;

#if UNITY_ANDROID

namespace GooglePlayGames.Android
{
    internal class AndroidSnapshotMetadata : ISavedGameMetadata
    {
        public AndroidSnapshotMetadata(AndroidJavaObject javaSnapshot)
        {
            JavaSnapshot = javaSnapshot;
            JavaMetadata = javaSnapshot.Call<AndroidJavaObject>("getMetadata");
            JavaContents = javaSnapshot.Call<AndroidJavaObject>("getSnapshotContents");
        }

        public AndroidSnapshotMetadata(AndroidJavaObject javaMetadata, AndroidJavaObject javaContents)
        {
            JavaSnapshot = null;
            JavaMetadata = javaMetadata;
            JavaContents = javaContents;
        }

        public AndroidJavaObject JavaSnapshot { get; }

        public AndroidJavaObject JavaMetadata { get; }

        public AndroidJavaObject JavaContents { get; }

        public bool IsOpen
        {
            get
            {
                if (JavaContents == null) return false;

                return !JavaContents.Call<bool>("isClosed");
            }
        }

        public string Filename => JavaMetadata.Call<string>("getUniqueName");

        public string Description => JavaMetadata.Call<string>("getDescription");

        public string CoverImageURL => JavaMetadata.Call<string>("getCoverImageUrl");

        public TimeSpan TotalTimePlayed => TimeSpan.FromMilliseconds(JavaMetadata.Call<long>("getPlayedTime"));

        public DateTime LastModifiedTimestamp
        {
            get
            {
                var timestamp = JavaMetadata.Call<long>("getLastModifiedTimestamp");
                var lastModifiedTime = AndroidJavaConverter.ToDateTime(timestamp);
                return lastModifiedTime;
            }
        }
    }
}
#endif