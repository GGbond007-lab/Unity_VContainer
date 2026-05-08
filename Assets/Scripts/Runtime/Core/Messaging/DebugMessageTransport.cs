namespace UniVCon
{
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    public sealed class DebugMessageTransport : IMessageTransport {
        public UniTask SendAsync(string json) {
            Debug.Log($"<color=green>[MESSAGE TRANSPORT]</color>\n{json}");
            return UniTask.CompletedTask;
        }
    }
}
