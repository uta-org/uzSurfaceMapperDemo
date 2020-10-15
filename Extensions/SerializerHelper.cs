using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using uzSurfaceMapper.Core.Generators;
using Newtonsoft.Json;
using UnityEngine;

namespace uzSurfaceMapper.Extensions
{
    public static class SerializerHelper
    {
        public static string Serialize<T>(this T data, SerializeFormat format = SerializeFormat.JSON)
        {
            switch (format)
            {
                case SerializeFormat.XML:
                    return SerializeToXml(data);

                case SerializeFormat.JSON:
                    return SerializeToJson(data);

                default:
                    return string.Empty;
            }
        }

        public static T Deserialize<T>(this string data, SerializeFormat format = SerializeFormat.JSON)
        {
            switch (format)
            {
                case SerializeFormat.XML:
                    return DeserializeToXml<T>(data);

                case SerializeFormat.JSON:
                    return DeserializeFromJson<T>(data);

                default:
                    return default;
            }
        }

        private static string SerializeToXml<T>(T data)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            // xmlSerializer.WriteProcessingInstruction("xml", "version='1.0'");

            using (var stringWriter = new StringWriter())
            {
                xmlSerializer.Serialize(stringWriter, data);
                return stringWriter.ToString();
            }
        }

        private static T DeserializeToXml<T>(string data)
        {
            try
            {
                var xmlSerializer = new XmlSerializer(data.GetType());
                using (var stream = GenerateStreamFromString(data))
                {
                    var result = xmlSerializer.Deserialize(stream);
                    return (T) result;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.Log(data);
                return default;
            }
        }

        private static Stream GenerateStreamFromString(string s)
        {
            // using
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream, Encoding.UTF8);

            writer.Write(s);
            writer.Flush();
            stream.Position = 0;

            return stream;
        }

        private static string SerializeToJson<T>(T data)
        {
            return JsonConvert.SerializeObject(data, !MapGenerator.IsDebugging ? Formatting.Indented : Formatting.None);
            // throw new NotImplementedException();
        }

        private static T DeserializeFromJson<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data);
            // throw new NotImplementedException();
        }
    }

    public enum SerializeFormat
    {
        XML = 1,
        JSON = 2
    }
}