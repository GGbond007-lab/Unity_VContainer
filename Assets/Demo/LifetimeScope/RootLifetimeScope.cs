using VContainer;
using VContainer.Unity;

public class RootLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // 1. 事件总线
        builder.Register<IEventBus, EventBus>(Lifetime.Singleton);
        // 工具类
        builder.Register<JsonSerializer>(Lifetime.Singleton);

        // 通信层 
        //builder.Register<IWebBridge, WebBridge>(Lifetime.Singleton);

        // 全局管理器
        builder.Register<ISceneLoadManager, SceneLoadManager>(Lifetime.Singleton);
        builder.Register<IDataSyncManager, DataSyncManager>(Lifetime.Singleton);
        builder.Register<ILabelManager, LabelManager>(Lifetime.Singleton);

        // 入口
        builder.RegisterEntryPoint<RootEntryPoint>();
    }
}

// 根入口
public class RootEntryPoint : IStartable
{
    private readonly ISceneLoadManager _sceneLoader;

    public RootEntryPoint(ISceneLoadManager sceneLoader)
    {
        _sceneLoader = sceneLoader;
    }

    public void Start()
    {
        // 启动 → 加载场景1
        _sceneLoader.LoadSceneAsync("ExampleScene1");
    }
}