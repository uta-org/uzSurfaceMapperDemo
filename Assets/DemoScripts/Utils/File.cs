using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using uzSurfaceMapper.Extensions;
using Object = UnityEngine.Object;

namespace uzSurfaceMapperDemo.Utils
{
    public static class File
    {
        public static void WriteAllText(string _path, string _str)
        {
            // TODO?
        }

        public static Stream Open(string _path, FileMode _create)
        {
            PrintError();
            return null;
        }

        public static bool Exists(string _path)
        {
            return UnityWebRequest.Head(_path).responseCode < 400;
        }

        public static bool Exists(string _path, out object result)
        {
            return Exists<object>(_path, out result);
        }

        public static bool Exists<T>(string _path, out T result)
        {
            T r = default;
            bool isFinish = false;
            Mono.StartCoroutine(F.AsyncReadFileWithWWW<T>(_path, _result =>
            {
                r = _result;
                isFinish = true;
            }));
            while (!isFinish) { }

            result = r;
            return result != null;
        }

        public static string ReadAllText(string _path)
        {
            string result = null;
            bool isFinish = false;
            Mono.StartCoroutine(F.AsyncReadFileWithWWW<string>(_path, _result =>
            {
                result = _result;
                isFinish = true;
            }));
            while (!isFinish) { }
            return result;
        }

        public static byte[] ReadAllBytes(string _path)
        {
            byte[] result = null;
            bool isFinish = false;
            Mono.StartCoroutine(F.AsyncReadFileWithWWW<byte[]>(_path, _result =>
            {
                result = _result;
                isFinish = true;
            }));
            while (!isFinish) { }
            return result;
        }

        public static Stream Create(string _fileName)
        {
            PrintError();
            return null;
        }

        public static Stream OpenWrite(string _fileName)
        {
            PrintError();
            return null;
        }

        public static void WriteAllBytes(string _filePath, byte[] _encodeToPng)
        {
            PrintError();
        }

        public static void Copy(string _valueTextureName, string _destinationFile)
        {
            PrintError();
        }

        private static void PrintError()
        {
            Debug.LogError("Cannot use this method on WebGL!");
        }

        private static void SetMono()
        {
            if (Mono != null)
                return;

            Mono = Object.FindObjectOfType<MonoBehaviour>();
        }

        private static MonoBehaviour Mono { get; set; }
    }
}