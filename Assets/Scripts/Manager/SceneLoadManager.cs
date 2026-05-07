using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadManager : ISceneLoadManager
{
    private readonly JsonSerializer _jsonSerializer;

    public SceneLoadManager(JsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    public async UniTask LoadSceneAsync(string sceneName)
    {
        Debug.Log($"[SceneLoadManager] 开始加载：{sceneName}");
        await SceneManager.LoadSceneAsync(sceneName).ToUniTask();

        var callbackData = new SceneLoadResponse
        {
            sceneName = sceneName,
            status = "loaded"
        };
        string json = _jsonSerializer.ToJson(callbackData);
        Debug.Log($"[SceneLoadManager] 加载完成：{json}");
    }
}
