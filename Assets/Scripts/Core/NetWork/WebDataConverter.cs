using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class WebDataConverter
{
    public bool TryConvertData<T>(object data, out T result)
    {
        try
        {
            result = ConvertData<T>(data);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[WebDataConverter] Convert data to {typeof(T).Name} failed: {e.Message}");
            result = default;
            return false;
        }
    }

    public T ConvertData<T>(object data)
    {
        if (data == null)
            return default;

        if (data is T matched)
            return matched;

        if (data is JToken token)
            return token.ToObject<T>();

        if (data is string json)
        {
            if (typeof(T) == typeof(string))
                return (T)(object)json;

            return JsonConvert.DeserializeObject<T>(json);
        }

        return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(data));
    }
}
