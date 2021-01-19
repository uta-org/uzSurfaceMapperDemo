using System;
using System.Threading;
using DepotToolkit.CommonCode;
#if !UZSURFACEMAPPER
using DepotToolkit.Utils;
#endif
using uzLib.Lite.ExternalCode.Extensions;
#if !UZSURFACEMAPPER
using uzSourceToolkit.ThirdParty.uSrcTools.Extensions;
#endif

namespace UnityEngine.Core
{
#if UNITY_2020 || UNITY_2019 || UNITY_2018 || UNITY_2017 || UNITY_5

    /// <summary>
    ///     Inherit from this base class to create a singleton.
    ///     e.g. public class MyClassName : Singleton<MyClassName> {}
    /// </summary>
    public class MonoSingleton<T> : MonoBehaviour // , IStarted
        where T : MonoBehaviour
    {
        // Check to see if we're about to be destroyed.
        private static bool m_ShuttingDown;

        private static readonly object m_Lock = new object();
        protected static T m_Instance;

        /// <summary>
        ///     Access singleton instance through this propriety.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (m_ShuttingDown)
                {
                    // FIX: Shutting down takes affect after playing a scene, this makes (ie) SteamWorkshopWrapper unavailable on Editor
                    // (check for ExecuteInEditrMode attribute, if available, shutthing down shuld not be performed)
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    //                    if (MonoSingletonSettings.ShowWarning)
                    //#pragma warning disable 162
                    //                        // ReSharper disable once HeuristicUnreachableCode
                    //                        Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                    //                                     "' already destroyed. Returning null.");
                    //#pragma warning restore 162
                    return null;
                }

                lock (m_Lock)
                {
                    if (m_Instance == null)
                    {
                        // Search for existing instance.
                        try
                        {
                            m_Instance = (T)FindObjectOfType(typeof(T));
                        }
                        catch (UnityException)
                        {
                            // Call with the Dispatcher...

#if !UZSURFACEMAPPER
                            var job = new JobWrapper<T>(Impl_GetInstance);
                            job.ExecuteSync();
                            m_Instance = job.Result;
#endif
                        }

                        // Create new instance if one doesn't already exist.
                        if (m_Instance == null)
                        {
                            // Need to create a new GameObject to attach the singleton to.
                            try
                            {
                                var singletonObject = new GameObject();
                                m_Instance = singletonObject.AddComponent<T>();
                                singletonObject.name = typeof(T).Name + " (Singleton)";

                                //((MonoSingleton<T>)m_Instance).IsStarted = false;

                                // Make instance persistent.
#if !UZSURFACEMAPPER
                                if (IsPlaying()) ThreadSafeUtils.ExecuteInUnityThread(() => DontDestroyOnLoad(singletonObject)); // TODO
#endif
                            }
                            catch (UnityException ex)
                            {
#if !UZSURFACEMAPPER
                                var singletonObject = CreateGameObject();
                                m_Instance = AddComponent<T>(singletonObject);
                                ThreadSafeUtils.ExecuteInUnityThread(() => singletonObject.name = typeof(T).Name + " (Singleton)");
#else
                                Debug.LogException(ex);
#endif
                            }
                        }
                    }

                    return m_Instance;
                }
            }
            protected set => m_Instance = value;
        }

        private static void Impl_GetInstance(out T result)
        {
            result = (T)FindObjectOfType(typeof(T));
        }

        private static void Impl_CreateInstance(out GameObject obj)
        {
            obj = new GameObject();
        }

        private static void Impl_IsPlaying(out bool result)
        {
            result = Application.isPlaying;
        }

        private static void Impl_AddComponent<TMethod>(GameObject obj, out TMethod result)
            where TMethod : Component
        {
            result = obj.AddComponent<TMethod>();
        }

#if !UZSURFACEMAPPER
        private static GameObject CreateGameObject()
        {
            var job = new JobWrapper<GameObject>(Impl_CreateInstance);
            job.ExecuteSync();
            return job.Result;
        }

        private static TMethod AddComponent<TMethod>(GameObject gameObject)
            where TMethod : Component
        {
            var job = new JobWrapper<GameObject, TMethod>(gameObject, Impl_AddComponent);
            job.ExecuteSync();
            return job.Result;
        }

        private static bool IsPlaying()
        {
            var job = new JobWrapper<bool>(Impl_IsPlaying);
            job.ExecuteSync();
            return job.Result;
        }

        public bool ExecuteInEditMode => GetType().IsExecutingInEditMode();
#endif

        public bool IsStarted { get; set; }

#if !UZSURFACEMAPPER
        public static T Create()
        {
            var go = new GameObject(typeof(T).Name);
            return go.GetOrAddComponent<T>();
        }

        private void OnApplicationQuit()
        {
            if (!ExecuteInEditMode) m_ShuttingDown = true;
        }

        private void OnDestroy()
        {
            if (!ExecuteInEditMode) m_ShuttingDown = true;
        }
#endif
    }

#endif
}