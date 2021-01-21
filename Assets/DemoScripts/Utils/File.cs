﻿using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using uzSurfaceMapper.Core.Attrs.CodeAnalysis;
using uzSurfaceMapper.Extensions;
using Object = UnityEngine.Object;

namespace uzSurfaceMapperDemo.Utils
{
    public static class File
    {
        private const int Timeout = 60;

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
            if (Application.isEditor) return System.IO.File.Exists(_path);
            return UnityWebRequest.Head(_path).responseCode < 400;
        }

        public static bool Exists(string _path, out object result)
        {
            return Exists<object>(_path, out result);
        }

        public static bool Exists<T>(string _path, out T result)
        {
            SetMono();
            var time = Time.time;

            T r = default;
            bool isFinish = false;
            Mono.StartCoroutine(F.AsyncReadFileWithWWW<T>(_path, _result =>
            {
                r = _result;
                isFinish = true;
            }));
            while (!isFinish)
            {
                if (Time.time - time > Timeout) throw new TimeoutException("Can't check if file exists due to timeout exceeded.");
            }

            result = r;
            return result != null;
        }

        //public static void ReadAllText(string _path, Action<string> result)
        //{
        //    SetMono();
        //    Mono.StartCoroutine(F.AsyncReadFileWithWWW(_path, result));
        //}

        [WIP] // TODO: not working
        public static string ReadAllText(string _path)
        {
            SetMono();
            var time = Time.time;

            string result = null;
            bool isFinish = false;
            Mono.StartCoroutine(F.AsyncReadFileWithWWW<string>(_path, _result =>
            {
                result = _result;
                isFinish = true;
            }));
            while (!isFinish)
            {
                if (Time.time - time > Timeout) throw new TimeoutException("Can't read file due to timeout exceeded.");
            }
            return result;
        }

        public static byte[] ReadAllBytes(string _path)
        {
            SetMono();
            var time = Time.time;

            byte[] result = null;
            bool isFinish = false;
            Mono.StartCoroutine(F.AsyncReadFileWithWWW<byte[]>(_path, _result =>
            {
                result = _result;
                isFinish = true;
            }));
            while (!isFinish)
            {
                if (Time.time - time > Timeout) throw new TimeoutException("Can't read file due to timeout exceeded.");
            }
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

        public static void WriteAllBytes(string _filePath, byte[] _byte)
        {
            if (Application.isEditor)
            {
                System.IO.File.WriteAllBytes(_filePath, _byte);
                return;
            }
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