using System.Diagnostics;

/// <summary>
/// 场景加载完成事件 
/// </summary>
public class SceneLoadedEvent : BaseEvent
{
    public string SceneName { get; set; }

}