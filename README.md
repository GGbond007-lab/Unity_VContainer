# UniVCon

UniVCon is a Unity action-routing framework built around VContainer, UniTask, Addressables, and configuration-driven web messages.

The project lets an external frontend send JSON messages to Unity. Unity resolves the target Action from `ActionConfigSO`, creates or reuses an Action through VContainer, executes the configured method, and sends structured results or errors back through a replaceable message transport.

## Unity Version

- Unity `6000.3.6f1`
- URP `17.3.0`
- Addressables `2.8.1`
- VContainer `1.17.0`
- UniTask is loaded through a Git package and locked by `Packages/packages-lock.json`

Keep `Packages/packages-lock.json` committed so every teammate resolves the same Git package revision.

## Runtime Flow

1. `RootLifetimeScope` registers core services in VContainer.
2. `RootEntryPoint` initializes `IActionConfigProvider`.
3. `IStartupSettingsProvider` provides the initial scene name.
4. `WebMsgHandlerManager` receives a web JSON payload.
5. `ActionDispatcher` switches to the Unity main thread, resolves the target Action, pushes it to `ActionStack`, and calls `BaseAction.OnExecute`.
6. `BaseAction` finds the configured method binding and executes the generated delegate from `ActionRegistry.g.cs`.
7. `IMessageSender` serializes responses and delivers them through `IMessageTransport`.

## Message Protocol

Incoming messages currently support only `type: "message"`.

```json
{
  "type": "message",
  "actionName": "YourAction3",
  "funcName": "ExpPing",
  "data": {}
}
```

Fields:

- `type`: Must be `"message"`.
- `actionName`: Target frontend action name configured in `ActionConfigSO`. Leave empty to dispatch to the current top Action.
- `funcName`: Frontend function name configured in `ActionConfigSO.methodBinds`.
- `data`: JSON payload passed to the Action method.

Errors are returned as structured `WebErrorResponse` payloads with an `ActionErrorCode`.

## Creating An Action

Use the editor menu:

```text
Action System/Create Action
```

The tool creates:

- An Action script under `Assets/Scripts/Action/<ActionName>/`
- An `ActionConfigSO` under `Assets/Resources/ActionConfigs/`
- An Addressables entry labeled `ActionConfig`
- A regenerated `ActionRegistry.g.cs`

Action methods exposed to the web must return `UniTask` and accept either zero parameters or one `object` parameter.

```csharp
public UniTask ExpPing()
{
    return UniTask.CompletedTask;
}

public UniTask ExpReceiveData(object data)
{
    return UniTask.CompletedTask;
}
```

## Configuration Rules

Each `ActionConfigSO` should define:

- `actionName`: The frontend-facing action name.
- `targetActionScript`: The runtime Action script.
- `methodBinds`: Mappings from frontend `funcName` to Unity method names.
- `subscribeBinds`: Optional Action-to-Action observation rules.
- `requiredSOs`: Optional ScriptableObject dependencies loaded by `BaseAction.GetSO<T>()`.

All `ActionConfigSO` assets must be Addressable and have the `ActionConfig` label.

## Registry Generation

`ActionRegistry.g.cs` is generated code and should not be edited manually.

Use:

```text
Action System/Generate Registry
Action System/Repair Registry
```

The editor hook regenerates the registry before entering Play Mode and before builds. The validator reports configuration errors such as missing methods, duplicate web function names, missing Addressables labels, and unresolved Action scripts.

## Startup Settings

Startup scene selection is provided by `IStartupSettingsProvider`.

Optional resource asset:

```text
Assets/Resources/AppStartupSettings.asset
```

If the asset is missing, the fallback initial scene is `ExampleScene1`.

## Message Transport

`WebMessageSender` no longer owns the network implementation. It serializes messages and delegates delivery to `IMessageTransport`.

The default transport is `DebugMessageTransport`, which logs outgoing JSON. Replace it in `RootLifetimeScope` with a WebSocket, HTTP, TCP, or WebGL bridge implementation when integrating with a real frontend.

## Demo Input

`InputService` is a development/demo helper. It is not registered by default in `RootLifetimeScope`, so production runtime does not automatically bind keyboard shortcuts.

If you want keyboard-driven local testing, register it in a demo-specific LifetimeScope or behind a development-only flag.

## Tests

Editor tests live under:

```text
Assets/Scripts/Tests/Editor
```

They cover:

- `ActionBus`
- `ActionStack`
- `ActionDispatcher`
- `ActionConfigProvider` behavior through a test provider
- `WebDataConverter`
- `WebMsgHandlerManager`
- `ActionSystemValidator`

Run them from Unity Test Runner in Edit Mode.

## Encoding

All C# scripts in `Assets/Scripts` should be saved as UTF-8. Prefer ASCII for logs and protocol strings unless localized user-facing text is required.
