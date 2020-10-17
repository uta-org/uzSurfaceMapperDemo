using System;
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

        private void Awake()
        {
            var awakeMethods = GetAttributes<InvokeAtAwakeAttribute>();
            ProcessMethods(awakeMethods, model => model.Method.Invoke(model.Instance, null));

            var updateMethods = GetAttributes<InvokeAtUpdateAttribute>();
            ProcessMethods(updateMethods, UpdateMethods.Add);
        }

        private void Update()
        {
            foreach (var updateMethod in UpdateMethods)
            {
                updateMethod.Method.Invoke(updateMethod.Instance, null);
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
                    methodCallback(new MethodModel(obj, method));
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
            public MethodModel(object _instance, MethodInfo _method)
            {
                Instance = _instance;
                Method = _method;
            }

            private MethodModel()
            {
            }

            public object Instance { get; }
            public MethodInfo Method { get; }
        }
    }
}