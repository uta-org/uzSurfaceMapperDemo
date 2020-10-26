using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using APIScripts.Utils;
using Newtonsoft.Json;

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

        public static byte[] SerializeBin<T>(this T obj, FloatProgressChangedEventHandler callback)
        {
            //if (callback == null) throw new ArgumentNullException(nameof(callback));

            using (var stream = new ProgressStream())
            {
                stream.ProgressChanged += callback;

                var binSerializer = new BinaryFormatter();
                binSerializer.Serialize(stream, obj);
                return stream.ToArray();
            }
        }

        // DSerialize collection of any type to a byte stream

        public static T DeserializeBin<T>(this byte[] serializedObj, FloatProgressChangedEventHandler callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            T obj;
            using (var stream = new ProgressStream(serializedObj))
            {
                stream.ProgressChanged += callback;

                var binSerializer = new BinaryFormatter();
                obj = (T)binSerializer.Deserialize(stream);
            }
            return obj;
        }

        public static byte[] Serialize<T>(this T obj, FloatProgressChangedEventHandler callback)
        {
            //if (callback == null) throw new ArgumentNullException(nameof(callback));

            var serializer = new JsonSerializer();
            using (var stream = new ProgressStream())
            {
                stream.ProgressChanged += callback;

                //var binSerializer = new BinaryFormatter();
                //binSerializer.Serialize(stream, obj);
                //return stream.ToArray();
                using (var writer = new StreamWriter(stream))
                using (var jsonTextWriter = new JsonTextWriter(writer))
                    serializer.Serialize(jsonTextWriter, obj);

                return stream.ToArray();
            }
        }

        // DSerialize collection of any type to a byte stream

        public static T Deserialize<T>(this string str, FloatProgressChangedEventHandler callback)
        {
            return Encoding.UTF8.GetBytes(str.ToCharArray()).Deserialize<T>(callback);
        }

        public static T Deserialize<T>(this byte[] data, FloatProgressChangedEventHandler callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            T obj;
            var serializer = new JsonSerializer();
            using (var stream = new ProgressStream(data))
            {
                stream.ProgressChanged += callback;

                //var binSerializer = new BinaryFormatter();
                //obj = (T)binSerializer.Deserialize(stream);

                using (var reader = new StreamReader(stream, Encoding.UTF8))
                using (var jsonTextReader = new JsonTextReader(reader))
                {
                    obj = (T)serializer.Deserialize(jsonTextReader);
                }
            }

            return obj;
        }

        public static string ReadFile(string path, FloatProgressChangedEventHandler callback)
        {
            using (var destination = new MemoryStream())
            using (var source = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var stream = new ProgressBaseStream(source))
            {
                stream.ProgressChanged += callback;
                stream.CopyTo(destination);
                return Encoding.UTF8.GetString(destination.ToArray());
            }
        }

        /*
         * Thanks to: https://stackoverflow.com/questions/38445215/progress-while-deserializing-json
            public JObject DeserializeViaStream(string filename)
            {
                object obj;
                var serializer = new JsonSerializer();
                using (var sr = new StreamReader(new FileStream(filename, FileMode.Open)))
                {
                    using (var jsonTextReader = new JsonTextReader(sr))
                    {
                        obj = serializer.Deserialize(jsonTextReader);
                    }
                }
                return (JObject) obj;
            }
         *
         */
    }
}