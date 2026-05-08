namespace UniVCon.Tests
{
    
    using UniVCon;using System;
    using NUnit.Framework;
    using UnityEngine;
    public sealed class ActionDispatcherTests {
        [Test] public async System.Threading.Tasks.Task DispatchReturnsStructuredErrorWhenActionFactoryThrows() {
            var config = ScriptableObject.CreateInstance<ActionConfigSO>();
            config.actionName = nameof(YourAction3);
            config.targetActionClassName = nameof(YourAction3);
            var configProvider = new TestActionConfigProvider(new[] {
                config
            }
            );
            var dispatcher = new ActionDispatcher( (_, _) => throw new InvalidOperationException("resolve failed"), new ActionStack(), configProvider);
            var result = await dispatcher.Dispatch(nameof(YourAction3), "Run", null);
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ActionErrorCode.ExecutionFailed, result.ErrorCode);
            Assert.AreEqual(nameof(YourAction3), result.ActionName);
            StringAssert.Contains("resolve failed", result.Message);
            UnityEngine.Object.DestroyImmediate(config);
        }
        [Test] public async System.Threading.Tasks.Task DispatchToCurrentUsesStackTopWhenActionNameIsEmpty() {
            var stack = new ActionStack();
            var action = new TestAction();
            stack.TryPush(action);
            var dispatcher = new ActionDispatcher( (_, _) => throw new AssertionException("Factory should not be called."), stack, new TestActionConfigProvider());
            var result = await dispatcher.Dispatch(null, "RunCurrent", null);
            Assert.IsTrue(result.Success);
            Assert.AreEqual("RunCurrent", result.FuncName);
        }
    }
}
