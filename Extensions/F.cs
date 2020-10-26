using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DepotToolkit.CommonCode
{
    public static class F
    {
        public static bool IsNullFile(this string path, out string contents)
        {
            contents = File.Exists(path) ? File.ReadAllText(path) : null;
            return string.IsNullOrEmpty(contents) || contents == null;
        }

        public static T FromBaseClassToDerivedClass<T>(this object baseObj)
            where T : new() //, params object[] args)
        {
            var derivedObj = new T(); //(T)Activator.CreateInstance(typeof(T));
            var t = baseObj.GetType();

            foreach (var fieldInf in t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                fieldInf.SetValue(derivedObj, fieldInf.GetValue(baseObj));

            foreach (var propInf in t.GetProperties(
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                try
                {
                    propInf.SetValue(derivedObj, propInf.GetValue(baseObj));
                }
                catch
                {
                    // Some properties hasn't setter...
                }

            return derivedObj;
        }
    }
}