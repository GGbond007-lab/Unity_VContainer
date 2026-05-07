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
        Debug.Log($"[SceneLoadManager] Loading scene: {sceneName}");
        await SceneManager.LoadSceneAsync(sceneName).ToUniTask();

        var callbackData = new SceneLoadResponse
        {
            sceneName = sceneName,
            status = "loaded"
        };

        var json = _jsonSerializer.ToJson(callbackData);
        Debug.Log($"[SceneLoadManager] Scene loaded: {json}");
    }
}
