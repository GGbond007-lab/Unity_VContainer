using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class Scene1_LifetimeScope : LifetimeScope
{
    //[SerializeField] private EventConfigSO _yourEvent1Config;
    protected override void Configure(IContainerBuilder builder)
    {
        // 自动注册所有 EventConfigSO
        // 注意：不能直接多次 RegisterInstance 同一种实现类型（都会是 EventConfigSO 的实现类型），
        // 会导致 VContainer 在构建集合时因为多个 Singleton 相同 ImplementationType 冲突。
        // 这里通过工厂注册为 Transient（返回相同实例）来避免冲突，同时仍可在容器中 Resolve 集合。
        foreach (var config in EventConfigProvider.AllConfigs().Values)
        {
            builder.Register<EventConfigSO>(c => config, Lifetime.Transient);
        }
        
        //builder.Register<IEventMsgHandler, YourEvent1MsgHandler>(Lifetime.Scoped);
        // 🔥 自动收集所有程序集里的 IEventMsgHandler 实现类
        var handlerTypes = System.AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type =>
                typeof(IEventMsgHandler).IsAssignableFrom(type)
                && type.IsClass
                && !type.IsAbstract
            );

        // 🔥 自动批量注册到 VContainer
        foreach (var type in handlerTypes)
        {
            builder.Register(typeof(IEventMsgHandler), type, Lifetime.Scoped);
        }

        // 管理器
        builder.Register<WebMsgHandlerManager>(Lifetime.Scoped);

        // 获取当前程序集里所有继承自 BaseEvent 的非抽象类
        var eventTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                t.IsSubclassOf(typeof(BaseEvent))
            );

        // 遍历自动注册
        foreach (var type in eventTypes)
        {
            builder.Register(type, Lifetime.Transient);
        }

        // 为每个具体事件的标签控制器注册实现（按事件类型注册具体 controller）
        // 这里示例注册 YourEvent1 的专属 LabelController，使其和事件实例同作用域/生命周期
        builder.Register<YourEvent1LabelController>(Lifetime.Transient);

        // 2. 事件栈
        builder.Register<EventStack>(Lifetime.Singleton);

        // 3. 输入服务 (修正：将 EntryPoint 和接口绑定到同一个实例)
        builder.RegisterEntryPoint<InputService>().As<IInputService>().AsSelf();

        // 4. 注册具体的事件类型 (必须注册，否则 Resolve(eventType) 会报错)
        //builder.Register<SceneLoadedEvent>(Lifetime.Transient);

        // 5. 注册 LabelManager (修正：补充缺少的依赖)
        // 假设实现类是 LabelManager，请根据你实际的类名修改
        //builder.Register<ILabelManager, LabelManager>(Lifetime.Singleton);

        // 在 VContainer 中，注册带参数的 Func 需要手动注入 IObjectResolver
        builder.Register<Func<Type, IBaseEvent>>(container =>
        {
            return eventType =>
            {
                // 从容器解析具体的事件实例
                var evt = (IBaseEvent)container.Resolve(eventType);

                // 获取事件栈并入栈
                var stack = container.Resolve<EventStack>();
                stack.Push(evt);

                // 注意：不要在工厂里执行事件的 Initialize，
                // 因为事件通常需要在创建后设置数据再初始化（避免 NullReference）。
                // Initialize 应由创建者在设置完事件的 Data 后调用。

                return evt;
            };
        }, Lifetime.Scoped);

        // 7. 场景入口
        builder.RegisterEntryPoint<Scene1EntryPoint>();
    }
}

// 场景入口
public class Scene1EntryPoint : IStartable
{
    private readonly ILabelManager _labelManager;
    private readonly IWebBridge _webBridge;
    private readonly IEventBus _eventBus;
    public Scene1EntryPoint(ILabelManager labelManager, IWebBridge webBridge, IEventBus eventBus)
    {
        _labelManager = labelManager;
        _webBridge = webBridge;
        _eventBus = eventBus;
    }

    public void Start()
    {
        UnityEngine.Debug.Log("[ExampleScene1] 场景初始化完成");
    }
}