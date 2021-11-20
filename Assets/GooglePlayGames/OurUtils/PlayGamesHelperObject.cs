// <copyright file="PlayGamesHelperObject.cs" company="Google Inc.">
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GooglePlayGames.OurUtils
{
    public class PlayGamesHelperObject : MonoBehaviour
    {
        // our (singleton) instance
        static PlayGamesHelperObject instance;

        // are we a dummy instance (used in the editor?)
        static bool sIsDummy;

        // queue of actions to run on the game thread
        static readonly List<Action> sQueue = new List<Action>();

        // flag that alerts us that we should check the queue
        // (we do this just so we don't have to lock() the queue every
        // frame to check if it's empty or not).
        static volatile bool sQueueEmpty = true;

        // callback for application pause and focus events
        static readonly List<Action<bool>> sPauseCallbackList =
            new List<Action<bool>>();

        static readonly List<Action<bool>> sFocusCallbackList =
            new List<Action<bool>>();

        // member variable used to copy actions from the sQueue and
        // execute them on the game thread.  It is a member variable
        // to help minimize memory allocations.
        readonly List<Action> localQueue = new List<Action>();

        // Call this once from the game thread
        public static void CreateObject()
        {
            if (instance != null) return;

            if (Application.isPlaying)
            {
                // add an invisible game object to the scene
                var obj = new GameObject("PlayGames_QueueRunner");
                DontDestroyOnLoad(obj);
                instance = obj.AddComponent<PlayGamesHelperObject>();
            }
            else
            {
                instance = new PlayGamesHelperObject();
                sIsDummy = true;
            }
        }

        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void OnDisable()
        {
            if (instance == this) instance = null;
        }

        public static void RunCoroutine(IEnumerator action)
        {
            if (instance != null) RunOnGameThread(() => instance.StartCoroutine(action));
        }

        public static void RunOnGameThread(Action action)
        {
            if (action == null) throw new ArgumentNullException("action");

            if (sIsDummy) return;

            lock (sQueue)
            {
                sQueue.Add(action);
                sQueueEmpty = false;
            }
        }

        public void Update()
        {
            if (sIsDummy || sQueueEmpty) return;

            // first copy the shared queue into a local queue
            localQueue.Clear();
            lock (sQueue)
            {
                // transfer the whole queue to our local queue
                localQueue.AddRange(sQueue);
                sQueue.Clear();
                sQueueEmpty = true;
            }

            // execute queued actions (from local queue)
            // use a loop to avoid extra memory allocations using the
            // forEach
            for (var i = 0; i < localQueue.Count; i++) localQueue[i].Invoke();
        }

        public void OnApplicationFocus(bool focused)
        {
            foreach (var cb in sFocusCallbackList)
                try
                {
                    cb(focused);
                }
                catch (Exception e)
                {
                    Logger.e("Exception in OnApplicationFocus:" +
                             e.Message + "\n" + e.StackTrace);
                }
        }

        public void OnApplicationPause(bool paused)
        {
            foreach (var cb in sPauseCallbackList)
                try
                {
                    cb(paused);
                }
                catch (Exception e)
                {
                    Logger.e("Exception in OnApplicationPause:" +
                             e.Message + "\n" + e.StackTrace);
                }
        }

        /// <summary>
        ///     Adds a callback that is called when the Unity method OnApplicationFocus
        ///     is called.
        /// </summary>
        /// <see cref="OnApplicationFocus" />
        /// <param name="callback">Callback.</param>
        public static void AddFocusCallback(Action<bool> callback)
        {
            if (!sFocusCallbackList.Contains(callback)) sFocusCallbackList.Add(callback);
        }

        /// <summary>
        ///     Removes the callback from the list to call when handling OnApplicationFocus
        ///     is called.
        /// </summary>
        /// <returns><c>true</c>, if focus callback was removed, <c>false</c> otherwise.</returns>
        /// <param name="callback">Callback.</param>
        public static bool RemoveFocusCallback(Action<bool> callback)
        {
            return sFocusCallbackList.Remove(callback);
        }

        /// <summary>
        ///     Adds a callback that is called when the Unity method OnApplicationPause
        ///     is called.
        /// </summary>
        /// <see cref="OnApplicationPause" />
        /// <param name="callback">Callback.</param>
        public static void AddPauseCallback(Action<bool> callback)
        {
            if (!sPauseCallbackList.Contains(callback)) sPauseCallbackList.Add(callback);
        }

        /// <summary>
        ///     Removes the callback from the list to call when handling OnApplicationPause
        ///     is called.
        /// </summary>
        /// <returns><c>true</c>, if focus callback was removed, <c>false</c> otherwise.</returns>
        /// <param name="callback">Callback.</param>
        public static bool RemovePauseCallback(Action<bool> callback)
        {
            return sPauseCallbackList.Remove(callback);
        }
    }
}