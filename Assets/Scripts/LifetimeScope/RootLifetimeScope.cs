using System;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

public class RootLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<IActionBus, ActionBus>(Lifetime.Singleton);
        builder.Register<IActionConfigProvider, AddressableActionConfigProvider>(Lifetime.Singleton);
        builder.Register<IStartupSettingsProvider, StartupSettingsProvider>(Lifetime.Singleton);
        builder.Register<JsonSerializer>(Lifetime.Singleton);
        builder.Register<WebDataConverter>(Lifetime.Singleton);

        builder.Register<ActionDispatcher>(Lifetime.Scoped);
        builder.Register<WebMsgHandlerManager>(Lifetime.Scoped);
        builder.Register<IMessageTransport, DebugMessageTransport>(Lifetime.Singleton);
        builder.Register<IMessageSender, WebMessageSender>(Lifetime.Singleton);
        builder.Register<ISceneLoadManager, SceneLoadManager>(Lifetime.Singleton);
        builder.Register<ILabelManager, LabelManager>(Lifetime.Singleton);

        builder.Register<StateMachineFactory>(Lifetime.Singleton);
        ActionRegistry.RegisterStates(builder);
        ActionRegistry.RegisterActions(builder);
        ActionRegistry.RegisterLabelControllers(builder);

        builder.Register<ActionStack>(Lifetime.Singleton);

        builder.Register<Func<Type, object[], IBaseAction>>(container =>
        {
            return (actionType, args) =>
            {
                if (!ActionRegistry.IsRegisteredAction(actionType))
                {
                    throw new InvalidOperationException($"Action type is not registered in {nameof(ActionRegistry)}: {actionType.FullName}");
                }

                return (IBaseAction)container.Resolve(actionType, args);
            };
        }, Lifetime.Scoped);

        builder.RegisterEntryPoint<RootEntryPoint>();
    }
}

public class RootEntryPoint : IStartable
{
    private readonly ISceneLoadManager _sceneLoader;
    private readonly IActionConfigProvider _configProvider;
    private readonly IStartupSettingsProvider _startupSettings;

    public RootEntryPoint(
        ISceneLoadManager sceneLoader,
        IActionConfigProvider configProvider,
        IStartupSettingsProvider startupSettings)
    {
        _sceneLoader = sceneLoader;
        _configProvider = configProvider;
        _startupSettings = startupSettings;
    }

    public void Start()
    {
        StartAsync().Forget();
    }

    private async UniTaskVoid StartAsync()
    {
        try
        {
            await _configProvider.InitializeAsync();

            var initialSceneName = _startupSettings.InitialSceneName;
            if (string.IsNullOrWhiteSpace(initialSceneName))
            {
                UnityEngine.Debug.LogWarning("[RootEntryPoint] Initial scene is empty. Startup scene load skipped.");
                return;
            }

            await _sceneLoader.LoadSceneAsync(initialSceneName);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"[RootEntryPoint] Startup sequence failed: {e}");
        }
    }
}
