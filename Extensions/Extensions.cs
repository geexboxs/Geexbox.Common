using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Geexbox.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Geexbox.Extensions
{
    public static class Extensions
    {
        public static string ComputeMd5(this string value)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            var bytValue = System.Text.Encoding.UTF8.GetBytes(value);
            var bytHash = md5.ComputeHash(bytValue);
            md5.Clear();
            string sTemp = "";
            for (int i = 0; i < bytHash.Length; i++)
            {
                sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
            }
            return sTemp.ToLower();
        }

        public static string ComputeMd5(this byte[] bytes)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            var bytHash = md5.ComputeHash(bytes);
            md5.Clear();
            string sTemp = "";
            for (int i = 0; i < bytHash.Length; i++)
            {
                sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
            }
            return sTemp.ToLower();
        }

        public static string ComputeMd5(this Stream stream)
        {
            if (!stream.CanSeek)
            {
                throw new NotSupportedException("流类型不支持该方法,请使用带有out参数的重载");
            }
            var position = stream.Position;
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            var bytHash = md5.ComputeHash(stream);
            stream.Position = position;
            md5.Clear();
            string sTemp = "";
            for (int i = 0; i < bytHash.Length; i++)
            {
                sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
            }
            return sTemp.ToLower();
        }

        public static string ComputeMd5WithOutStream(this Stream stream, out MemoryStream outStream)
        {
            var position = stream.Position;
            outStream = new MemoryStream();
            stream.CopyTo(outStream);
            outStream.Position = position;
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            var bytHash = md5.ComputeHash(outStream);
            outStream.Position = position;
            md5.Clear();
            string sTemp = "";
            for (int i = 0; i < bytHash.Length; i++)
            {
                sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
            }
            return sTemp.ToLower();
        }
        /// <summary>
        /// 将浏览器FileReader读取的dataurl转化成标准的base64字符串,同时获取其文件扩展名
        /// </summary>
        /// <param name="base64WithWebHeader"></param>
        /// <param name="fileExt"></param>
        /// <returns></returns>
        public static (string base64, string ext) DataUrlToBase64WithExt(this string base64WithWebHeader)
        {
            var splitIndex = base64WithWebHeader.IndexOf(",");
            var fileExt = "";
            if (splitIndex <= 0)
            {
                throw new Exception("未找到Base64的Web Header");
            }
            var webHeader = base64WithWebHeader.Split(new[] { ',' }, 2)[0];
            if (webHeader.Contains("jpeg"))
            {
                fileExt = "jpeg";
                return (base64WithWebHeader.Substring(splitIndex + 1), fileExt);
            }
            if (webHeader.Contains("png"))
            {
                fileExt = "png";
                return (base64WithWebHeader.Substring(splitIndex + 1), fileExt);
            }
            throw new Exception("不支持的Base64文件格式");
        }
        public static JsonSerializerSettings DefaultSerializeSettings { get; set; } = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Converters = new List<JsonConverter>()
            {
                new StringEnumConverter(),
                new EnumerationConverter(),
            },
        };

        public static JsonSerializerSettings IgnoreErrorSerializeSettings { get; set; } = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Error = (sender, args) =>
            {
                args.ErrorContext.Handled = true;
            },
            TypeNameHandling = TypeNameHandling.Auto,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            DateFormatString = "yyyy-MM-dd HH:mm:ss",
            Converters = new List<JsonConverter>()
            {
                new StringEnumConverter()
            },
        };

        public static string ToJson(this object @this, bool ignoreError = true)
        {
            if (ignoreError)
            {
                return JsonConvert.SerializeObject(@this, IgnoreErrorSerializeSettings);
            }
            else
            {
                return JsonConvert.SerializeObject(@this, DefaultSerializeSettings);
            }
        }

        public static T ToObject<T>(this string @this, bool ignoreError = true)
        {
            if (ignoreError)
            {
                return JsonConvert.DeserializeObject<T>(@this, IgnoreErrorSerializeSettings);
            }
            else
            {
                return JsonConvert.DeserializeObject<T>(@this, DefaultSerializeSettings);
            }
        }
    }
}
