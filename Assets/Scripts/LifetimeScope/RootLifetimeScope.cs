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
        //builder.Register<ISceneLoadManager, SceneLoadManager>(Lifetime.Singleton);//场景管理暂时没写
        //标签管理
        builder.Register<ILabelManager, LabelManager>(Lifetime.Singleton);


        // 状态机
        builder.Register<StateMachineFactory>(Lifetime.Singleton);
        
        // 自动注册所有继承自 IState 的状态
        AutoRegisterStates(builder);

        // 配置
        foreach (var config in ActionConfigProvider.AllConfigs().Values)
        {
            builder.Register<ActionConfigSO>(c => config, Lifetime.Transient);
        }

        // 消息处理器
        var handlerTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IActionMsgHandler).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

        foreach (var type in handlerTypes)
        {
            builder.Register(type, Lifetime.Scoped).AsImplementedInterfaces();
        }



        // 只注册事件类型，不让容器自动创建实例
        var actionTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IBaseAction).IsAssignableFrom(t));

        foreach (var type in actionTypes)
        {

            builder.Register(type, Lifetime.Transient).AsSelf();
        }

        var labelControllerTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IActionLabelController).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

        foreach (var lcType in labelControllerTypes)
        {
            builder.Register(lcType, Lifetime.Transient).AsSelf();
        }

        // 事件栈
        builder.Register<ActionStack>(Lifetime.Singleton);

        // 输入服务
        builder.RegisterEntryPoint<InputService>().As<IInputService>();

        //builder.Register<Func<Type, IBaseAction>>(container =>
        //{
        //    return actionType =>
        //    {
        //        // 使用容器解析以保证依赖注入
        //        var evt = (IBaseAction)container.Resolve(actionType);
        //        var stack = container.Resolve<ActionStack>();
        //        stack.Push(evt);
        //        return evt;
        //    };
        //}, Lifetime.Scoped);
        builder.Register<Func<Type, object[], IBaseAction>>(container =>
        {
            return (actionType, args) =>
            {
                // 支持传构造函数参数！
                var evt = (IBaseAction)container.Resolve(actionType, args);
                var stack = container.Resolve<ActionStack>();
                stack.Push(evt);
                return evt;
            };
        }, Lifetime.Scoped);
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