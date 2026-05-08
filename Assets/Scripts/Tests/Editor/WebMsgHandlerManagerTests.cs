namespace UniVCon.Tests
{
    
    using UniVCon;using Cysharp.Threading.Tasks;
    using NUnit.Framework;
    public sealed class WebMsgHandlerManagerTests {
        [Test] public async System.Threading.Tasks.Task ReceiveMessageFromWebReturnsInvalidJson() {
            var sender = new CapturingMessageSender();
            var manager = new WebMsgHandlerManager(new ActionDispatcher((_, _) => new TestAction(), new ActionStack(), new TestActionConfigProvider()), sender);
            var result = await manager.ReceiveMessageFromWeb("{bad json");
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ActionErrorCode.InvalidJson, result.ErrorCode);
            Assert.AreEqual(ActionErrorCode.InvalidJson, sender.LastError.ErrorCode);
        }
        [Test] public async System.Threading.Tasks.Task ReceiveMessageFromWebReturnsMissingAction() {
            var sender = new CapturingMessageSender();
            var manager = new WebMsgHandlerManager(new ActionDispatcher((_, _) => new TestAction(), new ActionStack(), new TestActionConfigProvider()), sender);
            var result = await manager.ReceiveMessageFromWeb(@"{""type"":""message"",""actionName"":""Missing"",""funcName"":""Run"",""data"":{}}");
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ActionErrorCode.ActionNotFound, result.ErrorCode);
            Assert.AreEqual("Missing", result.ActionName);
        }
        [Test] public async System.Threading.Tasks.Task ReceiveMessageFromWebRejectsUnsupportedType() {
            var sender = new CapturingMessageSender();
            var manager = new WebMsgHandlerManager(new ActionDispatcher((_, _) => new TestAction(), new ActionStack(), new TestActionConfigProvider()), sender);
            var result = await manager.ReceiveMessageFromWeb(@"{""type"":""event"",""actionName"":""Any"",""funcName"":""Run"",""data"":{}}");
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ActionErrorCode.InvalidMessageType, result.ErrorCode);
            Assert.AreEqual(ActionErrorCode.InvalidMessageType, sender.LastError.ErrorCode);
        }
        private sealed class CapturingMessageSender : IMessageSender {
            public ActionExecutionResult LastError {
                get;
                private set;
            }
            public void SendActionMessage(string type, string actionName, string funcName, object data) {
            }
            public void SendCurrentMessage(string type, string funcName, object data) {
            }
            public void SendError(ActionExecutionResult result) => LastError = result;
            public Cysharp.Threading.Tasks.UniTask SendActionMessageAsync(string type, string actionName, string funcName, object data) => Cysharp.Threading.Tasks.UniTask.CompletedTask;
            public Cysharp.Threading.Tasks.UniTask SendCurrentMessageAsync(string type, string funcName, object data) => Cysharp.Threading.Tasks.UniTask.CompletedTask;
            public Cysharp.Threading.Tasks.UniTask SendErrorAsync(ActionExecutionResult result) {
                LastError = result;
                return Cysharp.Threading.Tasks.UniTask.CompletedTask;
            }
        }
    }
}
