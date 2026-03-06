using System;
using System.Collections.Generic;
using UnityEngine;

namespace BindingSystem
{
    /// <summary>
    /// Internal singleton MonoBehaviour that provides Update/FixedUpdate dispatch,
    /// interval timers, component destruction tracking, and main-thread queueing.
    /// Created automatically and hidden from the hierarchy.
    /// </summary>
    [AddComponentMenu("")]
    public class BindRunner : MonoBehaviour
    {
        private static BindRunner _instance;
        private static bool _applicationQuitting;
        private static int _mainThreadId;
        private static bool _mainThreadIdSet;

        // Main thread dispatch queue
        private static readonly Queue<Action> _mainThreadQueue = new Queue<Action>();
        private static readonly object _queueLock = new object();

        // Update callbacks
        private readonly List<Action> _updateCallbacks = new List<Action>();
        private readonly List<Action> _fixedUpdateCallbacks = new List<Action>();
        private readonly List<Action> _lateUpdateCallbacks = new List<Action>();
        private readonly List<Action> _endOfFrameCallbacks = new List<Action>();

        // Interval callbacks
        private readonly List<IntervalEntry> _intervalEntries = new List<IntervalEntry>();

        // Destruction tracking
        private readonly Dictionary<int, DestroyTracker> _destroyTrackers = new Dictionary<int, DestroyTracker>();

        /// <summary>Returns true if the current thread is the main thread.</summary>
        public static bool IsMainThread
        {
            get
            {
                if (!_mainThreadIdSet) return true; // Before initialization, assume main thread
                return System.Threading.Thread.CurrentThread.ManagedThreadId == _mainThreadId;
            }
        }

        private static BindRunner Instance
        {
            get
            {
                if (_instance == null && !_applicationQuitting)
                {
                    var go = new GameObject("[BindRunner]");
                    go.hideFlags = HideFlags.HideInHierarchy;
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<BindRunner>();

                    _mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
                    _mainThreadIdSet = true;
                }
                return _instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            _mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            _mainThreadIdSet = true;
            _applicationQuitting = false;
            _ = Instance; // Force creation
        }

        private void OnApplicationQuit()
        {
            _applicationQuitting = true;
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        #region Main Thread Queue

        /// <summary>Enqueue an action to be executed on the main thread in the next Update.</summary>
        public static void EnqueueMainThread(Action action)
        {
            if (action == null) return;
            lock (_queueLock)
            {
                _mainThreadQueue.Enqueue(action);
            }
            // Ensure instance exists
            _ = Instance;
        }

        #endregion

        #region Update / FixedUpdate / LateUpdate Dispatch

        /// <summary>Register a callback to be called every Update.</summary>
        public static Action RegisterUpdate(Action callback)
        {
            Instance._updateCallbacks.Add(callback);
            return () => Instance?._updateCallbacks.Remove(callback);
        }

        /// <summary>Register a callback to be called every FixedUpdate.</summary>
        public static Action RegisterFixedUpdate(Action callback)
        {
            Instance._fixedUpdateCallbacks.Add(callback);
            return () => Instance?._fixedUpdateCallbacks.Remove(callback);
        }

        /// <summary>Register a callback to be called every LateUpdate.</summary>
        public static Action RegisterLateUpdate(Action callback)
        {
            Instance._lateUpdateCallbacks.Add(callback);
            return () => Instance?._lateUpdateCallbacks.Remove(callback);
        }

        /// <summary>Register a callback to be called at the end of every frame.</summary>
        public static Action RegisterEndOfFrame(Action callback)
        {
            Instance._endOfFrameCallbacks.Add(callback);
            return () => Instance?._endOfFrameCallbacks.Remove(callback);
        }

        /// <summary>Register a callback to be called at a custom time interval.</summary>
        public static Action RegisterInterval(float intervalSeconds, Action callback)
        {
            var entry = new IntervalEntry
            {
                Interval = intervalSeconds,
                Callback = callback,
                Elapsed = 0f
            };
            Instance._intervalEntries.Add(entry);
            return () => Instance?._intervalEntries.Remove(entry);
        }

        #endregion

        #region Destroy Tracking

        /// <summary>
        /// Track destruction of a Component and invoke a callback when it is destroyed.
        /// </summary>
        public static void TrackDestroy(Component component, Action onDestroy)
        {
            if (component == null || onDestroy == null) return;

            var instance = Instance;
            if (instance == null) return;

            int id = component.GetInstanceID();

            if (!instance._destroyTrackers.TryGetValue(id, out var tracker))
            {
                // Add a DestroyTracker component to the same GameObject
                tracker = component.gameObject.GetComponent<DestroyTracker>();
                if (tracker == null)
                {
                    tracker = component.gameObject.AddComponent<DestroyTracker>();
                }
                instance._destroyTrackers[id] = tracker;
            }

            tracker.AddCallback(id, onDestroy);
        }

        /// <summary>Remove a destroy tracker entry.</summary>
        internal static void RemoveTracker(int instanceId)
        {
            if (_instance != null)
                _instance._destroyTrackers.Remove(instanceId);
        }

        #endregion

        #region Unity Callbacks

        private void Update()
        {
            // Process main thread queue
            lock (_queueLock)
            {
                while (_mainThreadQueue.Count > 0)
                {
                    var action = _mainThreadQueue.Dequeue();
                    try { action(); }
                    catch (Exception e) { Debug.LogException(e); }
                }
            }

            // Update callbacks
            for (int i = _updateCallbacks.Count - 1; i >= 0; i--)
            {
                try { _updateCallbacks[i](); }
                catch (Exception e) { Debug.LogException(e); }
            }

            // Interval callbacks
            float dt = Time.deltaTime;
            for (int i = _intervalEntries.Count - 1; i >= 0; i--)
            {
                var entry = _intervalEntries[i];
                entry.Elapsed += dt;
                if (entry.Elapsed >= entry.Interval)
                {
                    entry.Elapsed -= entry.Interval;
                    try { entry.Callback(); }
                    catch (Exception e) { Debug.LogException(e); }
                }
            }

            // End of frame callbacks
            for (int i = _endOfFrameCallbacks.Count - 1; i >= 0; i--)
            {
                try { _endOfFrameCallbacks[i](); }
                catch (Exception e) { Debug.LogException(e); }
            }
        }

        private void FixedUpdate()
        {
            for (int i = _fixedUpdateCallbacks.Count - 1; i >= 0; i--)
            {
                try { _fixedUpdateCallbacks[i](); }
                catch (Exception e) { Debug.LogException(e); }
            }
        }

        private void LateUpdate()
        {
            for (int i = _lateUpdateCallbacks.Count - 1; i >= 0; i--)
            {
                try { _lateUpdateCallbacks[i](); }
                catch (Exception e) { Debug.LogException(e); }
            }
        }

        #endregion

        #region Internal Types

        private class IntervalEntry
        {
            public float Interval;
            public Action Callback;
            public float Elapsed;
        }

        #endregion
    }

    /// <summary>
    /// Helper component attached to tracked GameObjects to detect when they are destroyed.
    /// </summary>
    [AddComponentMenu("")]
    public class DestroyTracker : MonoBehaviour
    {
        private readonly Dictionary<int, List<Action>> _callbacks = new Dictionary<int, List<Action>>();

        public void AddCallback(int instanceId, Action callback)
        {
            if (!_callbacks.TryGetValue(instanceId, out var list))
            {
                list = new List<Action>();
                _callbacks[instanceId] = list;
            }
            list.Add(callback);
        }

        private void OnDestroy()
        {
            foreach (var kvp in _callbacks)
            {
                foreach (var callback in kvp.Value)
                {
                    try { callback(); }
                    catch (Exception e) { Debug.LogException(e); }
                }
                BindRunner.RemoveTracker(kvp.Key);
            }
            _callbacks.Clear();
        }
    }
}
