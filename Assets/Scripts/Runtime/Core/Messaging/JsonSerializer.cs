namespace UniVCon
{
    public class JsonSerializer {
        public string ToJson(object obj) => UnityEngine.JsonUtility.ToJson(obj, true);
        public T FromJson<T>(string json) => UnityEngine.JsonUtility.FromJson<T>(json);
    }
}
