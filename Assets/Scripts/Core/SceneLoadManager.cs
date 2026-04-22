using UnityEngine;

public class SceneLoadManager : ISceneLoadManager
{
    //private readonly IWebBridge _webBridge;
    private readonly JsonSerializer _jsonSerializer;

    public SceneLoadManager( JsonSerializer jsonSerializer)
    {
        //_webBridge = webBridge;
        _jsonSerializer = jsonSerializer;
    }

    /// <summary>
    /// 异步加载场景 + 加载完成回调前端
    /// </summary>
    public async void LoadSceneAsync(string sceneName)
    {
        UnityEngine.Debug.Log($"[SceneLoadManager] 开始加载：{sceneName}");
        await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);

        // 加载完成 → 通知前端
        var callbackData = new SceneLoadResponse
        {
            sceneName = sceneName,
            status = "loaded"
        };
        string json = _jsonSerializer.ToJson(callbackData);
        //_webBridge.SendMessageToWeb(json);

        //UnityEngine.Debug.Log($"[SceneLoadManager] 加载完成：{sceneName}，已通知前端 ");
    }
}