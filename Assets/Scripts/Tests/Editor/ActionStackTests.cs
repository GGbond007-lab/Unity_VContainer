namespace UniVCon.Tests
{
    
    using UniVCon;using NUnit.Framework;
    public sealed class ActionStackTests {
        [Test] public void PushRejectsDuplicateTopAction() {
            var stack = new ActionStack();
            var first = new TestAction();
            var second = new TestAction();
            var firstResult = stack.TryPush(first);
            var secondResult = stack.TryPush(second);
            Assert.IsTrue(firstResult.Success);
            Assert.IsFalse(secondResult.Success);
            Assert.AreEqual(ActionErrorCode.DuplicateAction, secondResult.ErrorCode);
            Assert.AreEqual(1, stack.Count);
            Assert.IsTrue(second.Destroyed);
        }
        [Test] public void PopDestroysActionAndCancelsToken() {
            var stack = new ActionStack();
            var action = new TestAction();
            stack.TryPush(action);
            stack.Pop();
            Assert.IsTrue(action.Destroyed);
            Assert.IsTrue(action.CancellationToken.IsCancellationRequested);
            Assert.AreEqual(0, stack.Count);
        }
    }
}
