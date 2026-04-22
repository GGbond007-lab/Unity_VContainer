using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class Scene1_LifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // 配置
        foreach (var config in EventConfigProvider.AllConfigs().Values)
        {
            builder.Register<EventConfigSO>(c => config, Lifetime.Transient);
        }

        // 消息处理器
        var handlerTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IEventMsgHandler).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

        foreach (var type in handlerTypes)
        {
            builder.Register(type, Lifetime.Scoped).AsImplementedInterfaces();
        }

        builder.Register<WebMsgHandlerManager>(Lifetime.Scoped);

        // 🔥 修复 1：只注册事件类型，不让容器自动创建实例
        var eventTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IBaseEvent).IsAssignableFrom(t));

        foreach (var type in eventTypes)
        {

            builder.Register(type, Lifetime.Transient).AsSelf();
        }

        var labelControllerTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IEventLabelController).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);

        foreach (var lcType in labelControllerTypes)
        {
            //builder.Register(lcType, Lifetime.Transient).AsSelf().AsImplementedInterfaces();
            builder.Register(lcType, Lifetime.Transient).AsSelf();
        }

        // 事件栈
        builder.Register<EventStack>(Lifetime.Singleton);

        // 输入服务
        builder.RegisterEntryPoint<InputService>().As<IInputService>();

        builder.Register<Func<Type, IBaseEvent>>(container =>
        {
            return eventType =>
            {
                // 使用容器解析以保证依赖注入
                var evt = (IBaseEvent)container.Resolve(eventType);
                var stack = container.Resolve<EventStack>();
                stack.Push(evt);
                return evt;
            };
        }, Lifetime.Scoped);

        builder.RegisterEntryPoint<Scene1EntryPoint>();
    }
}

public class Scene1EntryPoint : IStartable
{
    private readonly ILabelManager _labelManager;
    private readonly IEventBus _eventBus;

    public Scene1EntryPoint(ILabelManager labelManager, IEventBus eventBus)
    {
        _labelManager = labelManager;
        _eventBus = eventBus;
    }

    public void Start()
    {
        Debug.Log("[ExampleScene1] 初始化完成");
    }
}