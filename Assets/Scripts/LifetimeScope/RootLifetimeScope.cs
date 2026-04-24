using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using VContainer.StateMachine;

public class RootLifetimeScope : LifetimeScope
{
    //[SerializeField]
    //private MonoBehaviour coroutineRunner;
    
    protected override void Configure(IContainerBuilder builder)
    {
        // 1. 事件总线
        builder.Register<IActionBus, ActionBus>(Lifetime.Singleton);
        // 工具类
        builder.Register<JsonSerializer>(Lifetime.Singleton);

        // 通信层 
        builder.Register<WebMsgHandlerManager>(Lifetime.Scoped);//这个处理消息
        builder.Register<IMessageSender, WebMessageSender>(Lifetime.Singleton);//这个发送消息
        // 全局管理器
        builder.Register<ISceneLoadManager, SceneLoadManager>(Lifetime.Singleton);
        //builder.Register<IDataSyncManager, DataSyncManager>(Lifetime.Singleton);
        builder.Register<ILabelManager, LabelManager>(Lifetime.Singleton);

        // 协程运行器
        //builder.RegisterInstance(coroutineRunner);

        // 状态机
        builder.Register<StateMachineFactory>(Lifetime.Singleton);
        
        // 自动注册所有继承自 IState 的状态
        AutoRegisterStates(builder);

        // 入口
       // builder.RegisterEntryPoint<RootEntryPoint>();

    }
    
    private void AutoRegisterStates(IContainerBuilder builder)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var stateTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IState).IsAssignableFrom(t));
        
        foreach (var type in stateTypes)
        {
            builder.Register(type, Lifetime.Transient);
        }
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