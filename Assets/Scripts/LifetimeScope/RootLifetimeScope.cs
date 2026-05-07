using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class RootLifetimeScope : LifetimeScope
{
    //[SerializeField]
    //private MonoBehaviour coroutineRunner;

    protected override void Configure(IContainerBuilder builder)
    {
        // 1. Action 总线
        builder.Register<IActionBus, ActionBus>(Lifetime.Singleton);
        // 工具类
        builder.Register<JsonSerializer>(Lifetime.Singleton);
        builder.Register<WebDataConverter>(Lifetime.Singleton);

        // 通信层
        builder.Register<WebMsgHandlerManager>(Lifetime.Scoped);// 这个处理消息
        builder.Register<IMessageSender, WebMessageSender>(Lifetime.Singleton);// 这个发送消息
        builder.Register<ISceneLoadManager, SceneLoadManager>(Lifetime.Singleton);// 场景管理暂时没写
        // 标签管理
        builder.Register<ILabelManager, LabelManager>(Lifetime.Singleton);

        // 状态机
        builder.Register<StateMachineFactory>(Lifetime.Singleton);
        ActionRegistry.RegisterStates(builder);
        ActionRegistry.RegisterHandlers(builder);
        ActionRegistry.RegisterActions(builder);
        ActionRegistry.RegisterLabelControllers(builder);

        // Action 栈
        builder.Register<ActionStack>(Lifetime.Singleton);

        // 输入服务
        builder.RegisterEntryPoint<InputService>().As<IInputService>();

        builder.Register<Func<Type, object[], IBaseAction>>(container =>
        {
            return (actionType, args) =>
            {
                if (!ActionRegistry.IsRegisteredAction(actionType))
                {
                    throw new InvalidOperationException($"Action type is not registered in {nameof(ActionRegistry)}: {actionType.FullName}");
                }

                // 支持传构造函数参数
                var action = (IBaseAction)container.Resolve(actionType, args);
                var stack = container.Resolve<ActionStack>();
                stack.Push(action);
                return action;
            };
        }, Lifetime.Scoped);

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
        _sceneLoader.LoadSceneAsync("ExampleScene1").Forget();
    }
}
