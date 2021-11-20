using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using GooglePlayGames.OurUtils;
using UnityEngine;
using Logger = GooglePlayGames.OurUtils.Logger;

#if UNITY_ANDROID
#pragma warning disable 0642 // Possible mistaken empty statement

namespace GooglePlayGames.Android
{
    internal class AndroidSavedGameClient : ISavedGameClient
    {
        // Regex for a valid filename. Valid file names are between 1 and 100 characters (inclusive)
        // and only include URL-safe characters: a-z, A-Z, 0-9, or the symbols "-", ".", "_", or "~".
        // This regex is guarded by \A and \Z which guarantee that the entire string matches this
        // regex. If these were omitted, then illegal strings containing legal subsequences would be
        // allowed (since the regex would match those subsequences).
        static readonly Regex ValidFilenameRegex = new Regex(@"\A[a-zA-Z0-9-._~]{1,100}\Z");
        volatile AndroidClient mAndroidClient;

        volatile AndroidJavaObject mSnapshotsClient;

        public AndroidSavedGameClient(AndroidClient androidClient, AndroidJavaObject account)
        {
            mAndroidClient = androidClient;
            using (var gamesClass = new AndroidJavaClass("com.google.android.gms.games.Games"))
            {
                mSnapshotsClient = gamesClass.CallStatic<AndroidJavaObject>("getSnapshotsClient",
                    AndroidHelperFragment.GetActivity(), account);
            }
        }

        public void OpenWithAutomaticConflictResolution(string filename, DataSource source,
            ConflictResolutionStrategy resolutionStrategy,
            Action<SavedGameRequestStatus, ISavedGameMetadata> completedCallback)
        {
            Misc.CheckNotNull(filename);
            Misc.CheckNotNull(completedCallback);
            var prefetchDataOnConflict = false;
            ConflictCallback conflictCallback = null;
            completedCallback = ToOnGameThread(completedCallback);

            if (conflictCallback == null)
                conflictCallback = (resolver, original, originalData, unmerged, unmergedData) =>
                {
                    switch (resolutionStrategy)
                    {
                        case ConflictResolutionStrategy.UseOriginal:
                            resolver.ChooseMetadata(original);
                            return;
                        case ConflictResolutionStrategy.UseUnmerged:
                            resolver.ChooseMetadata(unmerged);
                            return;
                        case ConflictResolutionStrategy.UseLongestPlaytime:
                            if (original.TotalTimePlayed >= unmerged.TotalTimePlayed)
                                resolver.ChooseMetadata(original);
                            else
                                resolver.ChooseMetadata(unmerged);

                            return;
                        default:
                            Logger.e("Unhandled strategy " + resolutionStrategy);
                            completedCallback(SavedGameRequestStatus.InternalError, null);
                            return;
                    }
                };

            conflictCallback = ToOnGameThread(conflictCallback);

            if (!IsValidFilename(filename))
            {
                Logger.e("Received invalid filename: " + filename);
                completedCallback(SavedGameRequestStatus.BadInputError, null);
                return;
            }

            InternalOpen(filename, source, resolutionStrategy, prefetchDataOnConflict, conflictCallback,
                completedCallback);
        }

        public void OpenWithManualConflictResolution(string filename, DataSource source, bool prefetchDataOnConflict,
            ConflictCallback conflictCallback, Action<SavedGameRequestStatus, ISavedGameMetadata> completedCallback)
        {
            Misc.CheckNotNull(filename);
            Misc.CheckNotNull(conflictCallback);
            Misc.CheckNotNull(completedCallback);

            conflictCallback = ToOnGameThread(conflictCallback);
            completedCallback = ToOnGameThread(completedCallback);

            if (!IsValidFilename(filename))
            {
                Logger.e("Received invalid filename: " + filename);
                completedCallback(SavedGameRequestStatus.BadInputError, null);
                return;
            }

            InternalOpen(filename, source, ConflictResolutionStrategy.UseManual, prefetchDataOnConflict,
                conflictCallback, completedCallback);
        }

        public void ReadBinaryData(ISavedGameMetadata metadata,
            Action<SavedGameRequestStatus, byte[]> completedCallback)
        {
            Misc.CheckNotNull(metadata);
            Misc.CheckNotNull(completedCallback);
            completedCallback = ToOnGameThread(completedCallback);

            var convertedMetadata = metadata as AndroidSnapshotMetadata;

            if (convertedMetadata == null)
            {
                Logger.e("Encountered metadata that was not generated by this ISavedGameClient");
                completedCallback(SavedGameRequestStatus.BadInputError, null);
                return;
            }

            if (!convertedMetadata.IsOpen)
            {
                Logger.e("This method requires an open ISavedGameMetadata.");
                completedCallback(SavedGameRequestStatus.BadInputError, null);
                return;
            }

            var data = convertedMetadata.JavaContents.Call<byte[]>("readFully");
            if (data == null)
                completedCallback(SavedGameRequestStatus.BadInputError, null);
            else
                completedCallback(SavedGameRequestStatus.Success, data);
        }

        public void ShowSelectSavedGameUI(string uiTitle, uint maxDisplayedSavedGames, bool showCreateSaveUI,
            bool showDeleteSaveUI, Action<SelectUIStatus, ISavedGameMetadata> callback)
        {
            Misc.CheckNotNull(uiTitle);
            Misc.CheckNotNull(callback);

            callback = ToOnGameThread(callback);

            if (!(maxDisplayedSavedGames > 0))
            {
                Logger.e("maxDisplayedSavedGames must be greater than 0");
                callback(SelectUIStatus.BadInputError, null);
                return;
            }

            AndroidHelperFragment.ShowSelectSnapshotUI(
                showCreateSaveUI, showDeleteSaveUI, (int) maxDisplayedSavedGames, uiTitle, callback);
        }

        public void CommitUpdate(ISavedGameMetadata metadata, SavedGameMetadataUpdate updateForMetadata,
            byte[] updatedBinaryData, Action<SavedGameRequestStatus, ISavedGameMetadata> callback)
        {
            Misc.CheckNotNull(metadata);
            Misc.CheckNotNull(updatedBinaryData);
            Misc.CheckNotNull(callback);

            callback = ToOnGameThread(callback);

            var convertedMetadata = metadata as AndroidSnapshotMetadata;

            if (convertedMetadata == null)
            {
                Logger.e("Encountered metadata that was not generated by this ISavedGameClient");
                callback(SavedGameRequestStatus.BadInputError, null);
                return;
            }

            if (!convertedMetadata.IsOpen)
            {
                Logger.e("This method requires an open ISavedGameMetadata.");
                callback(SavedGameRequestStatus.BadInputError, null);
                return;
            }

            if (!convertedMetadata.JavaContents.Call<bool>("writeBytes", updatedBinaryData))
            {
                Logger.e("This method requires an open ISavedGameMetadata.");
                callback(SavedGameRequestStatus.BadInputError, null);
            }

            using (var convertedMetadataChange = AsMetadataChange(updateForMetadata))
            using (var task = mSnapshotsClient.Call<AndroidJavaObject>("commitAndClose", convertedMetadata.JavaSnapshot,
                convertedMetadataChange))
            {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                    task,
                    /* disposeResult= */ false,
                    snapshotMetadata =>
                    {
                        Debug.Log("commitAndClose.succeed");
                        callback(SavedGameRequestStatus.Success,
                            new AndroidSnapshotMetadata(snapshotMetadata, /* contents= */null));
                    });

                AddOnFailureListenerWithSignOut(
                    task,
                    exception =>
                    {
                        Debug.Log("commitAndClose.failed: " + exception.Call<string>("toString"));
                        var status = mAndroidClient.IsAuthenticated()
                            ? SavedGameRequestStatus.InternalError
                            : SavedGameRequestStatus.AuthenticationError;
                        callback(status, null);
                    });
            }
        }

        public void FetchAllSavedGames(DataSource source,
            Action<SavedGameRequestStatus, List<ISavedGameMetadata>> callback)
        {
            Misc.CheckNotNull(callback);

            callback = ToOnGameThread(callback);

            using (var task =
                mSnapshotsClient.Call<AndroidJavaObject>("load", /* forecReload= */
                    source == DataSource.ReadNetworkOnly))
            {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                    task,
                    annotatedData =>
                    {
                        using (var buffer = annotatedData.Call<AndroidJavaObject>("get"))
                        {
                            var count = buffer.Call<int>("getCount");
                            var result = new List<ISavedGameMetadata>();
                            for (var i = 0; i < count; ++i)
                                using (var metadata = buffer.Call<AndroidJavaObject>("get", i))
                                {
                                    result.Add(new AndroidSnapshotMetadata(
                                        metadata.Call<AndroidJavaObject>("freeze"), /* contents= */null));
                                }

                            buffer.Call("release");
                            callback(SavedGameRequestStatus.Success, result);
                        }
                    });

                AddOnFailureListenerWithSignOut(
                    task,
                    exception =>
                    {
                        Logger.d("FetchAllSavedGames failed: " + exception.Call<string>("toString"));
                        var status = mAndroidClient.IsAuthenticated()
                            ? SavedGameRequestStatus.InternalError
                            : SavedGameRequestStatus.AuthenticationError;
                        callback(status, new List<ISavedGameMetadata>());
                    }
                );
            }
        }

        public void Delete(ISavedGameMetadata metadata)
        {
            var androidMetadata = metadata as AndroidSnapshotMetadata;
            Misc.CheckNotNull(androidMetadata);
            using (mSnapshotsClient.Call<AndroidJavaObject>("delete", androidMetadata.JavaMetadata))
            {
                ;
            }
        }

        void InternalOpen(string filename, DataSource source, ConflictResolutionStrategy resolutionStrategy,
            bool prefetchDataOnConflict, ConflictCallback conflictCallback,
            Action<SavedGameRequestStatus, ISavedGameMetadata> completedCallback)
        {
            int conflictPolicy; // SnapshotsClient.java#RetentionPolicy
            switch (resolutionStrategy)
            {
                case ConflictResolutionStrategy.UseLastKnownGood:
                    conflictPolicy = 2 /* RESOLUTION_POLICY_LAST_KNOWN_GOOD */;
                    break;
                case ConflictResolutionStrategy.UseMostRecentlySaved:
                    conflictPolicy = 3 /* RESOLUTION_POLICY_MOST_RECENTLY_MODIFIED */;
                    break;
                case ConflictResolutionStrategy.UseLongestPlaytime:
                    conflictPolicy = 1 /* RESOLUTION_POLICY_LONGEST_PLAYTIME*/;
                    break;
                case ConflictResolutionStrategy.UseManual:
                    conflictPolicy = -1 /* RESOLUTION_POLICY_MANUAL */;
                    break;
                default:
                    conflictPolicy = 3 /* RESOLUTION_POLICY_MOST_RECENTLY_MODIFIED */;
                    break;
            }

            using (var task =
                mSnapshotsClient.Call<AndroidJavaObject>("open", filename, /* createIfNotFound= */ true,
                    conflictPolicy))
            {
                AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                    task,
                    dataOrConflict =>
                    {
                        if (dataOrConflict.Call<bool>("isConflict"))
                        {
                            var conflict = dataOrConflict.Call<AndroidJavaObject>("getConflict");
                            var original =
                                new AndroidSnapshotMetadata(conflict.Call<AndroidJavaObject>("getSnapshot"));
                            var unmerged =
                                new AndroidSnapshotMetadata(
                                    conflict.Call<AndroidJavaObject>("getConflictingSnapshot"));

                            // Instantiate the conflict resolver. Note that the retry callback closes over
                            // all the parameters we need to retry the open attempt. Once the conflict is
                            // resolved by invoking the appropriate resolution method on
                            // AndroidConflictResolver, the resolver will invoke this callback, which will
                            // result in this method being re-executed. This recursion will continue until
                            // all conflicts are resolved or an error occurs.
                            var resolver = new AndroidConflictResolver(
                                this,
                                mSnapshotsClient,
                                conflict,
                                original,
                                unmerged,
                                completedCallback,
                                () => InternalOpen(filename, source, resolutionStrategy,
                                    prefetchDataOnConflict,
                                    conflictCallback, completedCallback));

                            var originalBytes = original.JavaContents.Call<byte[]>("readFully");
                            var unmergedBytes = unmerged.JavaContents.Call<byte[]>("readFully");
                            conflictCallback(resolver, original, originalBytes, unmerged, unmergedBytes);
                        }
                        else
                        {
                            using (var snapshot = dataOrConflict.Call<AndroidJavaObject>("getData"))
                            {
                                var metadata = snapshot.Call<AndroidJavaObject>("freeze");
                                completedCallback(SavedGameRequestStatus.Success,
                                    new AndroidSnapshotMetadata(metadata));
                            }
                        }
                    });

                AddOnFailureListenerWithSignOut(
                    task,
                    exception =>
                    {
                        Logger.d("InternalOpen has failed: " + exception.Call<string>("toString"));
                        var status = mAndroidClient.IsAuthenticated()
                            ? SavedGameRequestStatus.InternalError
                            : SavedGameRequestStatus.AuthenticationError;
                        completedCallback(status, null);
                    }
                );
            }
        }

        void AddOnFailureListenerWithSignOut(AndroidJavaObject task, Action<AndroidJavaObject> callback)
        {
            AndroidTaskUtils.AddOnFailureListener(
                task,
                exception =>
                {
                    if (Misc.IsApiException(exception))
                    {
                        var statusCode = exception.Call<int>("getStatusCode");
                        if (statusCode == /* CommonStatusCodes.SignInRequired */ 4 ||
                            statusCode == /* GamesClientStatusCodes.CLIENT_RECONNECT_REQUIRED */ 26502)
                            mAndroidClient.SignOut();
                    }

                    callback(exception);
                });
        }

        ConflictCallback ToOnGameThread(ConflictCallback conflictCallback)
        {
            return (resolver, original, originalData, unmerged, unmergedData) =>
            {
                Logger.d("Invoking conflict callback");
                PlayGamesHelperObject.RunOnGameThread(() =>
                    conflictCallback(resolver, original, originalData, unmerged, unmergedData));
            };
        }

        internal static bool IsValidFilename(string filename)
        {
            if (filename == null) return false;

            return ValidFilenameRegex.IsMatch(filename);
        }

        static AndroidJavaObject AsMetadataChange(SavedGameMetadataUpdate update)
        {
            using (var builder =
                new AndroidJavaObject("com.google.android.gms.games.snapshot.SnapshotMetadataChange$Builder"))
            {
                if (update.IsCoverImageUpdated)
                    using (var bitmapFactory = new AndroidJavaClass("android.graphics.BitmapFactory"))
                    using (var bitmap = bitmapFactory.CallStatic<AndroidJavaObject>(
                        "decodeByteArray", update.UpdatedPngCoverImage, /* offset= */0,
                        update.UpdatedPngCoverImage.Length))
                    using (builder.Call<AndroidJavaObject>("setCoverImage", bitmap))
                    {
                        ;
                    }

                if (update.IsDescriptionUpdated)
                    using (builder.Call<AndroidJavaObject>("setDescription", update.UpdatedDescription))
                    {
                        ;
                    }

                if (update.IsPlayedTimeUpdated)
                    using (builder.Call<AndroidJavaObject>("setPlayedTimeMillis",
                        Convert.ToInt64(update.UpdatedPlayedTime.Value.TotalMilliseconds)))
                    {
                        ;
                    }

                return builder.Call<AndroidJavaObject>("build");
            }
        }

        static Action<T1, T2> ToOnGameThread<T1, T2>(Action<T1, T2> toConvert)
        {
            return (val1, val2) => PlayGamesHelperObject.RunOnGameThread(() => toConvert(val1, val2));
        }

        /// <summary>
        ///     A helper class that encapsulates the state around resolving a file conflict. It holds all
        ///     the state that is necessary to invoke <see cref="SnapshotManager.Resolve" /> as well as a
        ///     callback that will re-attempt to open the file after the resolution concludes.
        /// </summary>
        class AndroidConflictResolver : IConflictResolver
        {
            readonly AndroidSavedGameClient mAndroidSavedGameClient;
            readonly Action<SavedGameRequestStatus, ISavedGameMetadata> mCompleteCallback;
            readonly AndroidJavaObject mConflict;
            readonly AndroidSnapshotMetadata mOriginal;
            readonly Action mRetryFileOpen;
            readonly AndroidJavaObject mSnapshotsClient;
            readonly AndroidSnapshotMetadata mUnmerged;

            internal AndroidConflictResolver(AndroidSavedGameClient androidSavedGameClient,
                AndroidJavaObject snapshotClient, AndroidJavaObject conflict,
                AndroidSnapshotMetadata original, AndroidSnapshotMetadata unmerged,
                Action<SavedGameRequestStatus, ISavedGameMetadata> completeCallback, Action retryOpen)
            {
                mAndroidSavedGameClient = androidSavedGameClient;
                mSnapshotsClient = Misc.CheckNotNull(snapshotClient);
                mConflict = Misc.CheckNotNull(conflict);
                mOriginal = Misc.CheckNotNull(original);
                mUnmerged = Misc.CheckNotNull(unmerged);
                mCompleteCallback = Misc.CheckNotNull(completeCallback);
                mRetryFileOpen = Misc.CheckNotNull(retryOpen);
            }

            public void ResolveConflict(ISavedGameMetadata chosenMetadata, SavedGameMetadataUpdate metadataUpdate,
                byte[] updatedData)
            {
                var convertedMetadata = chosenMetadata as AndroidSnapshotMetadata;

                if (convertedMetadata != mOriginal && convertedMetadata != mUnmerged)
                {
                    Logger.e("Caller attempted to choose a version of the metadata that was not part " +
                             "of the conflict");
                    mCompleteCallback(SavedGameRequestStatus.BadInputError, null);
                    return;
                }

                using (var contentUpdate = mConflict.Call<AndroidJavaObject>("getResolutionSnapshotContents"))
                {
                    if (!contentUpdate.Call<bool>("writeBytes", updatedData))
                    {
                        Logger.e("Can't update snapshot contents during conflict resolution.");
                        mCompleteCallback(SavedGameRequestStatus.BadInputError, null);
                    }

                    using (var convertedMetadataChange = AsMetadataChange(metadataUpdate))
                    using (var task = mSnapshotsClient.Call<AndroidJavaObject>(
                        "resolveConflict",
                        mConflict.Call<string>("getConflictId"),
                        convertedMetadata.JavaMetadata.Call<string>("getSnapshotId"),
                        convertedMetadataChange,
                        contentUpdate))
                    {
                        AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                            task,
                            dataOrConflict => mRetryFileOpen());

                        mAndroidSavedGameClient.AddOnFailureListenerWithSignOut(
                            task,
                            exception =>
                            {
                                Logger.d("ResolveConflict failed: " + exception.Call<string>("toString"));
                                var status = mAndroidSavedGameClient.mAndroidClient.IsAuthenticated()
                                    ? SavedGameRequestStatus.InternalError
                                    : SavedGameRequestStatus.AuthenticationError;
                                mCompleteCallback(status, null);
                            }
                        );
                    }
                }
            }

            public void ChooseMetadata(ISavedGameMetadata chosenMetadata)
            {
                var convertedMetadata = chosenMetadata as AndroidSnapshotMetadata;

                if (convertedMetadata != mOriginal && convertedMetadata != mUnmerged)
                {
                    Logger.e("Caller attempted to choose a version of the metadata that was not part " +
                             "of the conflict");
                    mCompleteCallback(SavedGameRequestStatus.BadInputError, null);
                    return;
                }

                using (var task = mSnapshotsClient.Call<AndroidJavaObject>(
                    "resolveConflict", mConflict.Call<string>("getConflictId"), convertedMetadata.JavaSnapshot))
                {
                    AndroidTaskUtils.AddOnSuccessListener<AndroidJavaObject>(
                        task,
                        dataOrConflict => mRetryFileOpen());

                    mAndroidSavedGameClient.AddOnFailureListenerWithSignOut(
                        task,
                        exception =>
                        {
                            Logger.d("ChooseMetadata failed: " + exception.Call<string>("toString"));
                            var status = mAndroidSavedGameClient.mAndroidClient.IsAuthenticated()
                                ? SavedGameRequestStatus.InternalError
                                : SavedGameRequestStatus.AuthenticationError;
                            mCompleteCallback(status, null);
                        }
                    );
                }
            }
        }
    }
}
#endif