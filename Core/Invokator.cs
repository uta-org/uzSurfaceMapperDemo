using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Extensions;
using uzLib.Lite.Extensions;
using uzSurfaceMapper.Core.Attrs;
using uzSurfaceMapper.Extensions;
using Debug = UnityEngine.Debug;

namespace uzSurfaceMapper.Core
{
    public class Invokator : MonoBehaviour
    {
        private static List<MethodInfo> AllMethods { get; set; }

        private static List<MethodModel> UpdateMethods { get; } = new List<MethodModel>();
        private static List<MethodModel> GUIMethods { get; } = new List<MethodModel>();

        private static List<BackgroundWorker> Workers { get; } = new List<BackgroundWorker>();

        private static Dictionary<Type, List<Tuple<MonoBehaviour, MethodInfo>>> Instances { get; } = new Dictionary<Type, List<Tuple<MonoBehaviour, MethodInfo>>>();
        private static ConcurrentBag<MethodInfo> PreparedMethods { get; } = new ConcurrentBag<MethodInfo>();

        private static HashSet<Type> Attributes { get; } = new HashSet<Type>();

        private Dictionary<string, Exception> UpdateException { get; set; } = new Dictionary<string, Exception>();
        private bool HasExceptions { get; set; }

        private static int Counter { get; set; }

        public bool m_stopAtExceptions = true;

        private bool awakeFinished;

        private void Awake()
        {
#if !IS_DEMO
            return;
#endif

            Attributes.Add(typeof(InvokeAtAwakeAttribute));
            Attributes.Add(typeof(InvokeAtUpdateAttribute));
            Attributes.Add(typeof(InvokeAtGUIAttribute));
            Attributes.Add(typeof(InvokeAtStartAttribute));

            StartCoroutine(AwakeCoroutine());
        }

        private IEnumerator AwakeCoroutine()
        {
            PrepareAllMethods();
            yield return new WaitUntil(() => AllMethods != null);

            GetAttributes(
                typeof(InvokeAtAwakeAttribute),
                typeof(InvokeAtStartAttribute),
                typeof(InvokeAtUpdateAttribute),
                typeof(InvokeAtGUIAttribute)
            );

            yield return new WaitUntil(() => Counter == 1);

            var procs = ProcessMethods();
            while (procs.MoveNext()) { yield return null; }

            ProcessMethods<InvokeAtAwakeAttribute>(model => model.Method.Invoke(model.Instance, null));
            ProcessMethods<InvokeAtUpdateAttribute>(UpdateMethods.Add);
            ProcessMethods<InvokeAtGUIAttribute>(GUIMethods.Add);

            //PreparedMethods.Clear();
            awakeFinished = true;
        }

        private IEnumerator Start()
        {
#if !IS_DEMO
            yield break;
#endif

            yield return new WaitUntil(() => awakeFinished);
            StartCoroutine(FindStartCoroutine());
        }

        private IEnumerator FindStartCoroutine()
        {
            var procs = ProcessMethods();
            while (procs.MoveNext()) { yield return null; }

            ProcessMethods<InvokeAtStartAttribute>(model =>
            {
                if (model.Method.ReturnType == typeof(IEnumerator))
                    model.Instance.StartCoroutine(model.Method.Name);
                else
                    model.Method.Invoke(model.Instance, null);
            });

            PreparedMethods.Clear();
        }

        private void Update()
        {
            if (m_stopAtExceptions && HasExceptions) return;

            foreach (var updateMethod in UpdateMethods)
            {
                if (UpdateException.ContainsKey(updateMethod.Method.Name)) continue;
                try
                {
                    updateMethod.Method.Invoke(updateMethod.Instance, null);
                }
                catch (Exception ex)
                {
                    HasExceptions = true;
                    Debug.LogException(ex);
                    UpdateException.Add(updateMethod.Method.Name, ex);
                }
            }
        }

        private void OnGUI()
        {
            if (m_stopAtExceptions && HasExceptions) return;

            foreach (var guiMethod in GUIMethods)
            {
                if (UpdateException.ContainsKey(guiMethod.Method.Name)) continue;
                try
                {
                    guiMethod.Method.Invoke(guiMethod.Instance, null);
                }
                catch (Exception ex)
                {
                    HasExceptions = true;
                    Debug.LogException(ex);
                    UpdateException.Add(guiMethod.Method.Name, ex);
                }
            }
        }

        private void OnDisable()
        {
            foreach (var worker in Workers) worker.CancelAsync();
        }

        private static void PrepareAllMethods()
        {
            Func<List<MethodInfo>> getMethods = () => AppDomain.CurrentDomain
                .GetAssemblies() // Returns all currenlty loaded assemblies
                .AsParallel()
                .SelectMany(x => x.GetTypes()) // returns all types defined in this assemblies
                .Where(x => x.IsClass) // only yields classes
                .SelectMany(x => x.GetMethods())
                .ToList();
            var bg = getMethods.RunAsync(m =>
            {
                AllMethods = m;
            });
            bg.WorkerSupportsCancellation = true;
            Workers.Add(bg);
        }

        private static void GetAttributes(params Type[] types)
        {
            GetAttributes(PrepareMethods, types);
        }

        private static void GetAttributes([NotNull] Action<List<MethodInfo>> methodsCallback, params Type[] types) // where T : Attribute
        {
            if (methodsCallback == null) throw new ArgumentNullException(nameof(methodsCallback));

            //var sw = Stopwatch.StartNew();
            Func<List<MethodInfo>> getMethods = () => AllMethods.AsParallel()
                 .Where(x => types.Any(t => x.GetCustomAttributes(t, false).FirstOrDefault() != null))
                 .Select(m => new { Name = $"{m.DeclaringType?.FullName}.{m.Name}", Method = m })
                 .DistinctBy(m => m.Name)
                 .Select(m => m.Method)
                 .ToList(); // returns only methods that have the InvokeAttribute

            var sw = Stopwatch.StartNew();
            var bg = getMethods.RunAsync(m =>
            {
                sw.Stop();
                Debug.Log($"Obtained {m.Count} methods in {sw.ElapsedMilliseconds} ms");
                methodsCallback(m);
            });
            bg.WorkerSupportsCancellation = true;
            Workers.Add(bg);
            //sw.Stop();
            //Debug.Log($"Get {attributes.Count} attributes of type {typeof(T)} in {sw.ElapsedMilliseconds} ms");
        }

        private static void PrepareMethods(List<MethodInfo> methods)
        {
            Debug.Log("Adding items to list");

            PreparedMethods.AddRange(methods);
            ++Counter;
        }

        private static IEnumerator ProcessMethods()
        {
            Debug.Log($"Processing {PreparedMethods.Count} methods.");

            int i = 0;
            long elapsed = 0;
            long lastElapsed = 0;
            var sw = Stopwatch.StartNew();

            foreach (var method in PreparedMethods)
            {
                if (method.DeclaringType == null) continue;
                if (Instances.ContainsKey(method.DeclaringType)) continue;
                var obj = (MonoBehaviour)FindObjectOfType(method.DeclaringType); // Find the instantiated class

                yield return null;

                var t = method.CustomAttributes.FirstOrDefault(attr => Attributes.Contains(attr.AttributeType))?.AttributeType;
                if (t == null)
                {
                    Debug.LogWarning("Null type found.");
                    continue;
                }
                if (Instances.ContainsKey(t))
                    Instances[t].Add(new Tuple<MonoBehaviour, MethodInfo>(obj, method));
                else
                    Instances.Add(t,
                        new List<Tuple<MonoBehaviour, MethodInfo>> { new Tuple<MonoBehaviour, MethodInfo>(obj, method) });

                sw.Stop();

                Debug.Log($"[{i}] Elapsed {sw.ElapsedMilliseconds} ms for method '{method.DeclaringType?.FullName}.{method.Name}'");

                elapsed += sw.ElapsedMilliseconds;

                sw.Reset();
                sw.Start();

                ++i;
            }
            Debug.Log($"Processed in {elapsed} ms");
        }

        private static void ProcessMethods<T>(Action<MethodModel> methodCallback)
        {
            if (methodCallback == null) throw new ArgumentNullException(nameof(methodCallback));
            try
            {
                var sw = Stopwatch.StartNew();

                //var obj = Activator.CreateInstance(method.DeclaringType);
                //method.Invoke(obj, null); // invoke the method

                var list = Instances[typeof(T)];
                long lastTime = 0;
                foreach (var tuple in list)
                {
                    methodCallback(new MethodModel(tuple.Item1, tuple.Item2));
                    long elapsed = sw.ElapsedMilliseconds - lastTime;
                    Debug.Log($"Executed method '{tuple.Item2.DeclaringType?.FullName}.{tuple.Item2.Name}()' of type '{typeof(T).Name}' in {elapsed} ms");
                    lastTime = sw.ElapsedMilliseconds;
                }

                //method.Invoke(obj, null);

                sw.Stop();

                Debug.Log($"[{typeof(T).Name}] Processed {list.Count} methods in {sw.ElapsedMilliseconds} ms");
            }
            catch (Exception ex)
            {
                // If the class isn't added to the inspector it'll throw a NRE.
                Debug.LogException(ex);
            }
        }

        internal class MethodModel
        {
            public MethodModel(MonoBehaviour _instance, MethodInfo _method)
            {
                Instance = _instance;
                Method = _method;
            }

            private MethodModel()
            {
            }

            public MonoBehaviour Instance { get; }
            public MethodInfo Method { get; }
        }
    }
}