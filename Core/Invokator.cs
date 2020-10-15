using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using uzSurfaceMapper.Core.Attrs;

namespace uzSurfaceMapper.Core
{
    public class Invokator : MonoBehaviour
    {
        public void Awake()
        {
            var methods = AppDomain.CurrentDomain.GetAssemblies() // Returns all currenlty loaded assemblies
                .SelectMany(x => x.GetTypes()) // returns all types defined in this assemblies
                .Where(x => x.IsClass) // only yields classes
                .SelectMany(x => x.GetMethods()) // returns all methods defined in those classes
                .Where(x => x.GetCustomAttributes(typeof(InvokeAtAwakeAttribute), false).FirstOrDefault() != null)
                .ToList(); // returns only methods that have the InvokeAttribute

            foreach (var method in methods) // iterate through all found methods
            {
                //var obj = Activator.CreateInstance(method.DeclaringType); // Instantiate the class
                //method.Invoke(obj, null); // invoke the method

                try
                {
                    var obj = (object)FindObjectOfType(method.DeclaringType);
                    // method.DeclaringType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                    method.Invoke(obj, null);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }
    }
}