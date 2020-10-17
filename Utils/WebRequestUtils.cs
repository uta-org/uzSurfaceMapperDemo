using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

namespace APIScripts.Utils
{
    public static class WebRequestUtils
    {
        private static Regex domainRegex = new Regex("^\\s*\\w+(?:\\.\\w+)+(\\/.*)?$");

        //[RequiredByNativeCode]
        internal static string RedirectTo(string baseUri, string redirectUri)
        {
            Uri relativeUri = redirectUri[0] != '/' ? new Uri(redirectUri, UriKind.RelativeOrAbsolute) : new Uri(redirectUri, UriKind.Relative);
            return relativeUri.IsAbsoluteUri ? relativeUri.AbsoluteUri : new Uri(new Uri(baseUri, UriKind.Absolute), relativeUri).AbsoluteUri;
        }

        public static string MakeInitialUrl(string targetUrl, string localUrl = "http://localhost/")
        {
            if (string.IsNullOrEmpty(targetUrl))
                return "";
            bool prependProtocol = false;
            Uri baseUri = new Uri(localUrl);
            Uri targetUri = (Uri)null;
            if (targetUrl[0] == '/')
            {
                targetUri = new Uri(baseUri, targetUrl);
                prependProtocol = true;
            }
            if (targetUri == (Uri)null && domainRegex.IsMatch(targetUrl))
            {
                targetUrl = baseUri.Scheme + "://" + targetUrl;
                prependProtocol = true;
            }
            FormatException formatException = (FormatException)null;
            try
            {
                if (targetUri == (Uri)null && targetUrl[0] != '.')
                    targetUri = new Uri(targetUrl);
            }
            catch (FormatException ex)
            {
                formatException = ex;
            }
            if (targetUri == (Uri)null)
            {
                try
                {
                    targetUri = new Uri(baseUri, targetUrl);
                    prependProtocol = true;
                }
                catch (FormatException ex)
                {
                    throw formatException;
                }
            }
            return MakeUriString(targetUri, targetUrl, prependProtocol);
        }

        internal static string MakeUriString(Uri targetUri, string targetUrl, bool prependProtocol)
        {
            if (targetUri.IsFile)
            {
                if (!targetUri.IsLoopback)
                    return targetUri.OriginalString;
                string encoded = targetUri.AbsolutePath;
                if (encoded.Contains("%"))
                    encoded = URLDecode(encoded);
                if (encoded.Length > 0 && encoded[0] != '/')
                    encoded = "/" + encoded;
                return "file://" + encoded;
            }
            string scheme = targetUri.Scheme;
            if (!prependProtocol && targetUrl.Length >= scheme.Length + 2 && targetUrl[scheme.Length + 1] != '/')
            {
                StringBuilder stringBuilder = new StringBuilder(scheme, targetUrl.Length);
                stringBuilder.Append(':');
                if (scheme == "jar")
                {
                    string encoded = targetUri.AbsolutePath;
                    if (encoded.Contains("%"))
                        encoded = URLDecode(encoded);
                    if (encoded.StartsWith("file:/") && encoded.Length > 6 && encoded[6] != '/')
                    {
                        stringBuilder.Append("file://");
                        stringBuilder.Append(encoded.Substring(5));
                    }
                    else
                        stringBuilder.Append(encoded);
                    return stringBuilder.ToString();
                }
                stringBuilder.Append(targetUri.PathAndQuery);
                stringBuilder.Append(targetUri.Fragment);
                return stringBuilder.ToString();
            }
            return targetUrl.Contains("%") ? targetUri.OriginalString : targetUri.AbsoluteUri;
        }

        private static string URLDecode(string encoded) => Encoding.UTF8.GetString(URLDecode(Encoding.UTF8.GetBytes(encoded)));

        public static byte[] URLDecode(byte[] toEncode) => Decode(toEncode, urlEscapeChar, urlSpace);

        public static byte[] Decode(byte[] input, byte escapeChar, byte[] space)
        {
            using (MemoryStream memoryStream1 = new MemoryStream(input.Length))
            {
                for (int index = 0; index < input.Length; ++index)
                {
                    if (ByteSubArrayEquals(input, index, space))
                    {
                        index += space.Length - 1;
                        memoryStream1.WriteByte((byte)32);
                    }
                    else if ((int)input[index] == (int)escapeChar && index + 2 < input.Length)
                    {
                        int num1 = index + 1;
                        MemoryStream memoryStream2 = memoryStream1;
                        byte[] b = input;
                        int offset = num1;
                        index = offset + 1;
                        int num2 = (int)Hex2Byte(b, offset);
                        memoryStream2.WriteByte((byte)num2);
                    }
                    else
                        memoryStream1.WriteByte(input[index]);
                }
                return memoryStream1.ToArray();
            }
        }

        private static bool ByteSubArrayEquals(byte[] array, int index, byte[] comperand)
        {
            if (array.Length - index < comperand.Length)
                return false;
            for (int index1 = 0; index1 < comperand.Length; ++index1)
            {
                if ((int)array[index + index1] != (int)comperand[index1])
                    return false;
            }
            return true;
        }

        private static byte[] ucHexChars = DefaultEncoding.GetBytes("0123456789ABCDEF");
        private static byte[] lcHexChars = DefaultEncoding.GetBytes("0123456789abcdef");
        private static byte urlEscapeChar = 37;

        private static byte[] urlSpace = new byte[1]
        {
            (byte) 43
        };

        private static byte[] dataSpace = DefaultEncoding.GetBytes("%20");
        private static byte[] urlForbidden = DefaultEncoding.GetBytes("@&;:<>=?\"'/\\!#%+$,{}|^[]`");
        private static byte qpEscapeChar = 61;
        private static byte[] qpSpace = new byte[1] { (byte)95 };
        private static byte[] qpForbidden = DefaultEncoding.GetBytes("&;=?\"'%+_");

        private static byte Hex2Byte(byte[] b, int offset)
        {
            byte num1 = 0;
            for (int index = offset; index < offset + 2; ++index)
            {
                byte num2 = (byte)((uint)num1 * 16U);
                int num3 = (int)b[index];
                if (num3 >= 48 && num3 <= 57)
                    num3 -= 48;
                else if (num3 >= 65 && num3 <= 75)
                    num3 -= 55;
                else if (num3 >= 97 && num3 <= 102)
                    num3 -= 87;
                if (num3 > 15)
                    return 63;
                num1 = (byte)((uint)num2 + (uint)(byte)num3);
            }
            return num1;
        }

        private static byte[] Byte2Hex(byte b, byte[] hexChars) => new byte[2]
        {
            hexChars[(int) b >> 4],
            hexChars[(int) b & 15]
        };

        internal static Encoding DefaultEncoding => Encoding.ASCII;
    }
}