using System;
using Idem.CompactJson;
using UnityEngine;

namespace Idem.Tools
{
    public static class JsonUtil
    {
        public static bool TryParse<T>(string json, out T result)
        {
            try
            {
                result = Serializer.Parse<T>(json);
                return true;
            }
            catch (Exception)
            {
                result = default;
                return false;
            }
        }

        public static T Parse<T>(string json)
        {
            try
            {
                return Serializer.Parse<T>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Could not parse json '{json}' to {typeof(T).Name}: {e.Message}\n{e.StackTrace}");
                return default;
            }
        }
    }

    public static class Extentions
    {
        public static string ToJson(this object obj, bool pretty = false)
        {
            return Serializer.ToString(obj, pretty);
        }

        public static T FromJson<T>(this string json)
        {
            return Serializer.Parse<T>(json);
        }
    }
}