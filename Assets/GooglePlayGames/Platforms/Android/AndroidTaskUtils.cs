using System;
using UnityEngine;

#if UNITY_ANDROID
#pragma warning disable 0642 // Possible mistaken empty statement

namespace GooglePlayGames.Android
{
    internal class AndroidTaskUtils
    {
        AndroidTaskUtils()
        {
        }

        public static void AddOnSuccessListener<T>(AndroidJavaObject task, Action<T> callback)
        {
            using (task.Call<AndroidJavaObject>("addOnSuccessListener",
                new TaskOnSuccessProxy<T>(callback, /* disposeResult= */ true)))
            {
                ;
            }
        }

        public static void AddOnSuccessListener<T>(AndroidJavaObject task, bool disposeResult, Action<T> callback)
        {
            using (task.Call<AndroidJavaObject>("addOnSuccessListener",
                new TaskOnSuccessProxy<T>(callback, disposeResult)))
            {
                ;
            }
        }

        public static void AddOnFailureListener(AndroidJavaObject task, Action<AndroidJavaObject> callback)
        {
            using (task.Call<AndroidJavaObject>("addOnFailureListener", new TaskOnFailedProxy(callback)))
            {
                ;
            }
        }

        public static void AddOnCompleteListener<T>(AndroidJavaObject task, Action<T> callback)
        {
            using (task.Call<AndroidJavaObject>("addOnCompleteListener", new TaskOnCompleteProxy<T>(callback)))
            {
                ;
            }
        }

        class TaskOnCompleteProxy<T> : AndroidJavaProxy
        {
            readonly Action<T> mCallback;

            public TaskOnCompleteProxy(Action<T> callback)
                : base("com/google/android/gms/tasks/OnCompleteListener")
            {
                mCallback = callback;
            }

            public void onComplete(T result)
            {
                if (result is IDisposable)
                    using ((IDisposable) result)
                    {
                        mCallback(result);
                    }
                else
                    mCallback(result);
            }
        }

        class TaskOnSuccessProxy<T> : AndroidJavaProxy
        {
            readonly Action<T> mCallback;
            readonly bool mDisposeResult;

            public TaskOnSuccessProxy(Action<T> callback, bool disposeResult)
                : base("com/google/android/gms/tasks/OnSuccessListener")
            {
                mCallback = callback;
                mDisposeResult = disposeResult;
            }

            public void onSuccess(T result)
            {
                if (result is IDisposable && mDisposeResult)
                    using ((IDisposable) result)
                    {
                        mCallback(result);
                    }
                else
                    mCallback(result);
            }
        }

        class TaskOnFailedProxy : AndroidJavaProxy
        {
            readonly Action<AndroidJavaObject> mCallback;

            public TaskOnFailedProxy(Action<AndroidJavaObject> callback)
                : base("com/google/android/gms/tasks/OnFailureListener")
            {
                mCallback = callback;
            }

            public void onFailure(AndroidJavaObject exception)
            {
                using (exception)
                {
                    mCallback(exception);
                }
            }
        }
    }
}
#endif