using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using uzSurfaceMapper.Core.Attrs;

namespace uzSurfaceMapper.Core
{
    public class Invokator : MonoBehaviour
    {
        private static List<MethodModel> UpdateMethods { get; } = new List<MethodModel>();
        private static List<MethodModel> GUIMethods { get; } = new List<MethodModel>();

        private Dictionary<string, Exception> UpdateException { get; set; } = new Dictionary<string, Exception>();
        private bool HasExceptions { get; set; }

        public bool m_stopAtExceptions = true;

        private void Awake()
        {
            var awakeMethods = GetAttributes<InvokeAtAwakeAttribute>();
            ProcessMethods(awakeMethods, model => model.Method.Invoke(model.Instance, null));

            var updateMethods = GetAttributes<InvokeAtUpdateAttribute>();
            ProcessMethods(updateMethods, UpdateMethods.Add);

            var guiMethods = GetAttributes<InvokeAtGUIAttribute>();
            ProcessMethods(guiMethods, GUIMethods.Add);
        }

        private void Start()
        {
            var startMethods = GetAttributes<InvokeAtStartAttribute>();
            ProcessMethods(startMethods, model =>
            {
                if (model.Method.ReturnType == typeof(IEnumerator))
                    model.Instance.StartCoroutine(model.Method.Name);
                else
                    model.Method.Invoke(model.Instance, null);
            });
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

        private static List<MethodInfo> GetAttributes<T>() where T : Attribute
        {
            return AppDomain.CurrentDomain.GetAssemblies() // Returns all currenlty loaded assemblies
                 .SelectMany(x => x.GetTypes()) // returns all types defined in this assemblies
                 .Where(x => x.IsClass) // only yields classes
                 .SelectMany(x => x.GetMethods()) // returns all methods defined in those classes
                 .Where(x => x.GetCustomAttributes(typeof(T), false).FirstOrDefault() != null)
                 .ToList(); // returns only methods that have the InvokeAttribute
        }

        private static void ProcessMethods(List<MethodInfo> methods, Action<MethodModel> methodCallback)
        {
            if (methodCallback == null) throw new ArgumentNullException(nameof(methodCallback));
            foreach (var method in methods) // iterate through all found methods
            {
                //var obj = Activator.CreateInstance(method.DeclaringType);
                //method.Invoke(obj, null); // invoke the method

                try
                {
                    var obj = (object)FindObjectOfType(method.DeclaringType); // Find the instantiated class
                    methodCallback(new MethodModel((MonoBehaviour)obj, method));
                    //method.Invoke(obj, null);
                }
                catch (Exception ex)
                {
                    // If the class isn't added to the inspector it'll throw a NRE.
                    Debug.LogException(ex);
                }
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