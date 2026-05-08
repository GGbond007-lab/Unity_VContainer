# UniVCon

UniVCon 是一个基于 VContainer、UniTask、Addressables 和配置驱动 Web 消息的 Unity Action 路由框架。

项目的核心目标是：外部前端向 Unity 发送 JSON 消息，Unity 根据 `ActionConfigSO` 找到目标 Action，通过 VContainer 创建或复用 Action，执行配置好的方法，并通过可替换的消息传输层返回结构化结果或错误。

## Unity 版本

- Unity `6000.3.6f1`
- URP `17.3.0`
- Addressables `2.8.1`
- VContainer `1.17.0`
- UniTask 通过 Git 包加载，并由 `Packages/packages-lock.json` 锁定版本

请保持 `Packages/packages-lock.json` 被提交到版本库中，这样团队成员能解析到一致的依赖版本。

## 脚本结构

当前脚本按照 Runtime、Editor、Tests 三条线拆分。Runtime 下再区分框架核心和示例代码，让“可复用框架”和“Demo 使用方式”边界更清楚。

```text
Assets/Scripts
├─ Runtime
│  ├─ UniVCon.Runtime.asmdef
│  ├─ Core
│  │  ├─ Actions
│  │  ├─ Config
│  │  ├─ Interfaces
│  │  ├─ Labels
│  │  ├─ LifetimeScopes
│  │  ├─ Messaging
│  │  ├─ Scene
│  │  └─ StateMachine
│  └─ Samples
│     ├─ Actions
│     ├─ Input
│     └─ StateMachine
├─ Editor
│  ├─ UniVCon.Editor.asmdef
│  └─ ActionSystem
└─ Tests
   └─ Editor
      └─ UniVCon.Tests.Editor.asmdef
```

### Runtime/Core

`Runtime/Core` 是框架主体，正式项目应该优先依赖这里的能力。

- `Actions`：Action 生命周期、Action 栈、Action 派发、Action Registry。
- `Config`：Action 配置、启动配置、配置 Provider、Inspector 辅助特性。
- `Interfaces`：运行时抽象接口，例如消息发送、配置读取、场景加载、标签管理。
- `Labels`：Addressables prefab 标签对象池和标签生命周期管理。
- `LifetimeScopes`：VContainer 根容器和场景容器注册入口。
- `Messaging`：Web 消息协议、JSON 转换、消息处理、消息发送和 Transport。
- `Scene`：场景加载响应、场景加载服务、Demo 场景绑定逻辑。
- `StateMachine`：通用状态机核心类型。

### Runtime/Samples

`Runtime/Samples` 是随框架保留的 Demo 和使用样板。这里的代码会参与运行时编译，但语义上不是框架核心。

- `Samples/Actions`：`YourAction1`、`YourAction2`、`YourAction3`、`SceneLoadedAction` 示例。
- `Samples/Input`：`InputService` 键盘快捷键调试服务。
- `Samples/StateMachine`：状态机示例状态和示例入口。

`InputService` 会继续在 `RootLifetimeScope` 中默认注册。这样进入 Play Mode 后可以直接通过键盘快捷键触发 Demo 消息，体现项目的调试能力；如果不注册，示例能力就会被藏起来，不利于框架演示和快速验证。

### Editor/ActionSystem

`Editor/ActionSystem` 是编辑器工具层，只在 Editor 下编译。

- `ActionCreator`：通过菜单创建新的 Action 和 `ActionConfigSO`。
- `ActionConfigSOEditor`：提供配置 Inspector 辅助选择方法和订阅目标。
- `ActionRegistryGenerator`：生成 `ActionRegistry.g.cs`。
- `ActionRegistryAutoGenerateHook`：进入 Play Mode 和 Build 前自动生成 Registry。
- `ActionRegistryAssetPostprocessor`：脚本、配置、Addressables 变化时触发修复。
- `ActionRegistryMenu`：菜单入口。
- `ActionSystemValidator`：配置校验。
- `ActionSystemValidatorWindow`：可视化校验窗口。

### Tests/Editor

`Tests/Editor` 是 Edit Mode 测试目录，覆盖核心运行链路和编辑器校验逻辑。

- `ActionBusTests`
- `ActionConfigProviderTests`
- `ActionDispatcherTests`
- `ActionStackTests`
- `ActionSystemValidatorTests`
- `WebDataConverterTests`
- `WebMsgHandlerManagerTests`

## 命名空间

所有新增 C# 类型都应该放在 `UniVCon` 命名空间体系下。

- Runtime 框架、Demo Actions、Managers、消息类型、LifetimeScopes：`namespace UniVCon`
- 状态机核心类型：`namespace UniVCon.StateMachine`
- 编辑器工具和校验器：`namespace UniVCon.Editor`
- Edit Mode 测试和测试 fake：`namespace UniVCon.Tests`

asmdef 的 `rootNamespace` 已经和这个布局对齐：

- `UniVCon.Runtime` -> `UniVCon`
- `UniVCon.Editor` -> `UniVCon.Editor`
- `UniVCon.Tests.Editor` -> `UniVCon.Tests`

`ActionRegistry.g.cs` 会生成到 `namespace UniVCon` 中，`ActionCreator` 模板也会创建 `namespace UniVCon` 下的新 Action。

## 后续新增代码方向

新增脚本时请优先按职责放置，避免再回到散落的全局目录。

| 新增内容 | 推荐目录 | 说明 |
| --- | --- | --- |
| 新的框架 Action 基础能力 | `Assets/Scripts/Runtime/Core/Actions` | 例如 Action 生命周期、栈策略、执行结果模型。 |
| 新的配置或启动设置 | `Assets/Scripts/Runtime/Core/Config` | 例如启动参数、配置 Provider、配置 SO。 |
| 新的运行时接口 | `Assets/Scripts/Runtime/Core/Interfaces` | 先抽象，再由 Core 或 Samples 实现。 |
| 新的消息协议/传输适配 | `Assets/Scripts/Runtime/Core/Messaging` | 例如 WebSocket、HTTP、TCP、WebGL JS bridge。 |
| 新的场景加载能力 | `Assets/Scripts/Runtime/Core/Scene` | 例如加载策略、场景切换响应模型。 |
| 新的标签/对象池能力 | `Assets/Scripts/Runtime/Core/Labels` | 例如不同 Label prefab 的生命周期管理。 |
| 新的状态机核心能力 | `Assets/Scripts/Runtime/Core/StateMachine` | 保持 `UniVCon.StateMachine` 命名空间。 |
| 新的 Demo Action | `Assets/Scripts/Runtime/Samples/Actions/<ActionName>` | 示例、教程、调试入口放这里。 |
| 新的 Demo 输入/调试服务 | `Assets/Scripts/Runtime/Samples/Input` | 允许默认注册，用于运行时演示。 |
| 新的 Demo 状态机示例 | `Assets/Scripts/Runtime/Samples/StateMachine` | 不要和核心状态机混在一起。 |
| 新的编辑器工具 | `Assets/Scripts/Editor/ActionSystem` | 只放 Editor 编译代码。 |
| 新的 Edit Mode 测试 | `Assets/Scripts/Tests/Editor` | 使用 `UniVCon.Tests` 命名空间。 |

如果某段代码会被正式项目复用，请放在 `Runtime/Core`。如果只是展示“怎么用”，请放在 `Runtime/Samples`。这条线很重要：它能让框架长期扩展时不把 Demo 行为误认为核心依赖。

## 运行时流程

1. `RootLifetimeScope` 在 VContainer 中注册核心服务、Demo 输入服务和启动入口。
2. `RootEntryPoint` 初始化 `IActionConfigProvider`。
3. `IStartupSettingsProvider` 提供初始场景名。
4. `SceneLoadManager` 加载初始场景。
5. `InputService` 或外部 Web 入口发送 JSON 消息。
6. `WebMsgHandlerManager` 解析并校验 Web JSON 消息。
7. `ActionDispatcher` 切回 Unity 主线程，解析目标 Action。
8. `ActionStack` 创建、复用、压栈或弹出 Action。
9. `BaseAction.OnExecute` 根据 `ActionConfigSO` 调用绑定方法。
10. `ActionRegistry.g.cs` 提供生成好的方法委托，避免运行时反射调用。
11. `IMessageSender` 序列化响应，并通过 `IMessageTransport` 发送。

## 消息协议

当前 Web 输入消息使用 `type: "message"`。

```json
{
  "type": "message",
  "actionName": "YourAction3",
  "funcName": "ExpPing",
  "data": {}
}
```

字段说明：

- `type`：必须是 `"message"`。
- `actionName`：目标前端 Action 名，来自 `ActionConfigSO.actionName`。为空时会派发给当前栈顶 Action。
- `funcName`：前端函数名，来自 `ActionConfigSO.methodBinds.webFuncName`。
- `data`：传给 Action 方法的 JSON payload。

错误会以结构化 `WebErrorResponse` 返回，并携带 `ActionErrorCode`。

常见错误：

- 非法 JSON：`ActionErrorCode.InvalidJson`
- 不支持的 `type`：`ActionErrorCode.InvalidMessageType`
- 未知 `actionName`：`ActionErrorCode.ActionNotFound`
- 未知 `funcName`：`ActionErrorCode.FunctionNotFound`

## 创建 Action

使用 Unity 菜单：

```text
Action System/Create Action
```

工具会创建：

- `Assets/Scripts/Runtime/Samples/Actions/<ActionName>/` 下的 Action 脚本
- `Assets/Resources/ActionConfigs/ActionConfig_<ActionName>.asset`
- 一个带有 `ActionConfig` label 的 Addressables entry
- 重新生成后的 `Assets/Scripts/Runtime/Core/Actions/ActionRegistry.g.cs`

能暴露给 Web 的 Action 方法必须返回 `UniTask`，并且只能接收零个参数或一个 `object` 参数。

```csharp
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UniVCon
{
    public sealed class ExampleAction : BaseAction
    {
        public UniTask ExpPing()
        {
            Debug.Log("ExampleAction.ExpPing");
            return UniTask.CompletedTask;
        }

        public UniTask ExpReceiveData(object data)
        {
            Debug.Log(data);
            return UniTask.CompletedTask;
        }
    }
}
```

## ActionConfigSO 配置规则

每个 `ActionConfigSO` 应该定义：

- `actionName`：前端使用的 Action 名。
- `targetActionScript`：目标运行时 Action 脚本。
- `targetActionClassName`：由 `targetActionScript` 自动同步出来的完整类型名。
- `methodBinds`：前端 `funcName` 到 Unity 方法名的映射。
- `subscribeBinds`：可选的 Action 之间观察/订阅关系。
- `requiredSOs`：可选的 ScriptableObject 依赖，会通过 `BaseAction.GetSO<T>()` 读取。

所有 `ActionConfigSO` 资产都必须是 Addressable，并且必须带有 `ActionConfig` label。

## Registry 生成

`ActionRegistry.g.cs` 是生成代码，不要手动编辑。

生成文件位置：

```text
Assets/Scripts/Runtime/Core/Actions/ActionRegistry.g.cs
```

常用菜单：

```text
Action System/Generate Registry
Action System/Repair Registry
Action System/Validate Configs
```

编辑器 hook 会在进入 Play Mode 前和构建前自动重新生成 Registry。Validator 会报告配置错误，例如方法缺失、重复 web 函数名、缺少 Addressables label、Action 脚本无法解析等。

## 启动设置

启动场景由 `IStartupSettingsProvider` 提供。

可选资源文件：

```text
Assets/Resources/AppStartupSettings.asset
```

创建方式：

```text
Create/UniVCon/App Startup Settings
```

如果这个资源不存在，默认初始场景是：

```text
ExampleScene1
```

启动链路：

```text
RootLifetimeScope
-> RootEntryPoint.Start
-> IActionConfigProvider.InitializeAsync
-> IStartupSettingsProvider.InitialSceneName
-> SceneLoadManager.LoadSceneAsync
-> InputService keyboard debugging
```

## Demo 键盘输入

`InputService` 默认注册在 `RootLifetimeScope` 中，所以只要程序运行，就会自动启用键盘驱动的 Demo 调试能力。

默认按键：

- `1`：发送 `YourAction1.SpawnLabelList` 示例消息。
- `2`：发送 `YourAction3.ExpPing` 示例消息。
- `8`：发送一个不存在的函数，用来测试错误链路。
- `9`：发送一个不存在的 Action，用来测试错误链路。
- `0`：弹出当前 Action，并打印 Label 对象池状态。

项目故意保持 Demo 输入默认启用。这样运行时不需要额外场景配置，也能直接展示消息管线。如果未来某个正式部署版本需要关闭键盘调试，可以用环境专用 LifetimeScope 或启动设置替换默认 `InputService` 注册。

## 消息传输层

运行时文件：

```text
Assets/Scripts/Runtime/Core/Interfaces/IMessageTransport.cs
Assets/Scripts/Runtime/Core/Messaging/DebugMessageTransport.cs
Assets/Scripts/Runtime/Core/Messaging/WebMessageSender.cs
```

默认行为：

- `WebMessageSender` 负责把响应序列化为 JSON。
- `DebugMessageTransport` 负责把发送出的 JSON 打印到 Console。

如果要接入真实前端，可以新增一个 transport 实现：

```csharp
namespace UniVCon
{
    using Cysharp.Threading.Tasks;

    public sealed class WebSocketMessageTransport : IMessageTransport
    {
        public UniTask SendAsync(string json)
        {
            // WebSocket.Send(json);
            return UniTask.CompletedTask;
        }
    }
}
```

然后在 `RootLifetimeScope` 中替换默认注册：

```csharp
builder.Register<IMessageTransport, WebSocketMessageTransport>(Lifetime.Singleton);
```

后续推荐扩展方向：

- WebSocket transport
- HTTP transport
- TCP transport
- WebGL JS bridge transport
- 用于测试的 in-memory transport

## 模块示例

### Web 消息入口

运行时入口：

```text
Assets/Scripts/Runtime/Core/Messaging/WebMsgHandlerManager.cs
```

示例消息：

```json
{
  "type": "message",
  "actionName": "YourAction3",
  "funcName": "ExpPing",
  "data": {}
}
```

预期行为：

- `WebMsgHandlerManager` 校验 JSON 和 `type`。
- `ActionDispatcher` 解析配置中的目标 Action。
- `ActionStack` 推入或复用 Action。
- `BaseAction.OnExecute` 调用 `ActionRegistry.g.cs` 生成的委托。
- `IMessageSender` 通过 `IMessageTransport` 发送错误或响应。

### Action 示例

示例 Action：

```text
Assets/Scripts/Runtime/Samples/Actions/YourAction1/YourAction1.cs
Assets/Scripts/Runtime/Samples/Actions/YourAction2/YourAction2.cs
Assets/Scripts/Runtime/Samples/Actions/YourAction3/YourAction3.cs
Assets/Scripts/Runtime/Samples/Actions/SceneLoadedAction.cs
```

建议先使用 `YourAction3`，因为它的行为最简单。

```json
{
  "type": "message",
  "actionName": "YourAction3",
  "funcName": "ExpPing",
  "data": {}
}
```

如果使用键盘 Demo，进入 Play Mode 后按 `2` 即可触发这条链路。

### Label 示例

运行时文件：

```text
Assets/Scripts/Runtime/Core/Labels/LabelManager.cs
Assets/Scripts/Runtime/Samples/Actions/YourAction1/Labels/ILabel.cs
Assets/Scripts/Runtime/Samples/Actions/YourAction1/Labels/LabelItem.cs
Assets/Scripts/Runtime/Samples/Actions/YourAction1/Labels/LabelNewItem.cs
Assets/Scripts/Runtime/Samples/Actions/YourAction1/Labels/YourActionLabelController.cs
```

典型用途：

- 通过 Addressables 加载 Label prefab。
- 使用对象池复用 Label 实例。
- 在 Action 生命周期结束时清理当前 Action 相关 Label。

可以进入 Play Mode 后按 `1` 触发 `YourAction1.SpawnLabelList`，观察 Label 创建和对象池日志。

### Scene 示例

运行时文件：

```text
Assets/Scripts/Runtime/Core/Scene/SceneLoadManager.cs
Assets/Scripts/Runtime/Core/Interfaces/ISceneLoadManager.cs
Assets/Scripts/Runtime/Core/Scene/SceneLoadResponse.cs
Assets/Scripts/Runtime/Core/Scene/SceneManagerBinder.cs
```

典型用途：

- `SceneLoadManager` 封装异步场景加载。
- `SceneLoadResponse` 表达场景加载结果。
- `SceneManagerBinder` 是 Demo 场景绑定示例，负责把场景对象绑定给示例配置。

正式项目中建议逐步减少 `GameObject.Find` 和运行时写 ScriptableObject 的方式，改为 Inspector 引用、VContainer scene scope 注册或场景上下文对象注入。

### StateMachine 示例

核心文件：

```text
Assets/Scripts/Runtime/Core/StateMachine/IState.cs
Assets/Scripts/Runtime/Core/StateMachine/StateMachine.cs
Assets/Scripts/Runtime/Core/StateMachine/StateMachineFactory.cs
```

示例文件：

```text
Assets/Scripts/Runtime/Samples/StateMachine/ExampleStates.cs
Assets/Scripts/Runtime/Samples/StateMachine/StateMachineExample.cs
```

典型用途：

- `IState` 定义进入、更新、退出状态的协议。
- `StateMachine` 管理状态切换。
- `StateMachineFactory` 负责创建状态机。
- `StateMachineExample` 展示如何组织示例状态。

## 测试使用方式

Editor 测试位于：

```text
Assets/Scripts/Tests/Editor
```

在 Unity 中运行：

1. 打开 `Window/Test Runner`。
2. 选择 `Edit Mode`。
3. 运行 `UniVCon.Tests.Editor` 下的全部测试，或单独运行某个测试类。

也可以在命令行中先做 C# 编译验证：

```powershell
dotnet build UniVCon.Runtime.csproj
dotnet build UniVCon.Editor.csproj
dotnet build UniVCon.Tests.Editor.csproj
```

各测试模块说明：

| 测试 | 覆盖内容 |
| --- | --- |
| `ActionBusTests` | Action 之间的本地事件发布和订阅。 |
| `ActionConfigProviderTests` | 配置 Provider 的初始化、查找和测试 fake 行为。 |
| `ActionDispatcherTests` | Web 消息到 Action 执行的派发链路。 |
| `ActionStackTests` | Action push、pop、reuse、destroy 生命周期。 |
| `ActionSystemValidatorTests` | 配置校验器对错误配置的识别。 |
| `WebDataConverterTests` | JSON payload 到目标数据结构的转换。 |
| `WebMsgHandlerManagerTests` | JSON 解析、type 校验、错误响应和正常派发。 |

测试 fake：

```text
Assets/Scripts/Tests/Editor/TestAction.cs
Assets/Scripts/Tests/Editor/TestActionConfigProvider.cs
```

测试里应该优先使用测试 fake 或测试辅助类注入配置，不要把测试专用写入能力放回生产接口。

## 运行前检查清单

进入 Play Mode 测试示例前，建议按下面顺序检查：

1. 使用 Unity `6000.3.6f1` 打开项目。
2. 等 Unity 完成资源导入和脚本编译。
3. 打开包含 `RootLifetimeScope` 的启动场景。
4. 执行 `Action System/Generate Registry`。
5. 执行 `Action System/Validate Configs`。
6. 进入 Play Mode。
7. 按 `1`、`2`、`8`、`9`、`0` 验证正常链路和错误链路。
8. 在 Console 中观察启动、场景加载、Action 派发、发送和错误日志。

## 编码约定

项目脚本、README 和配置文本都应保存为 UTF-8。包含中文日志、中文说明或 Addressables key 的文件尤其需要注意编码，否则可能出现乱码 key，进而导致配置查找失败。

当前建议：

- C# 脚本使用 UTF-8。
- README 使用 UTF-8。
- 新增中文字符串后，在 Unity 和 Git diff 中都检查是否显示正常。
- 不要把已经乱码的 key 继续扩散到新配置里。

## 维护约定

- 不要手动编辑 `ActionRegistry.g.cs`。
- 修改 Action 方法、ActionConfig 或订阅关系后，重新生成 Registry。
- 新增 `ActionConfigSO` 后，确认 Addressables entry 带有 `ActionConfig` label。
- Runtime 框架能力放 `Runtime/Core`，Demo 和教程能力放 `Runtime/Samples`。
- Editor 工具只能放 `Editor/ActionSystem`，不要进入 Runtime asmdef。
- 测试代码只能放 `Tests/Editor`，不要污染生产接口。
- 如果新增真实网络通信层，请通过 `IMessageTransport` 替换，不要把具体 WebSocket、HTTP 或 TCP 实现写死进 `WebMessageSender`。
