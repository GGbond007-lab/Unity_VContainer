namespace UniVCon
{
    using Cysharp.Threading.Tasks;
    using Newtonsoft.Json;
    using UnityEngine;
    using VContainer.Unity;

    public class InputService : IInputService, ITickable
    {
        private const string CreateYourAction1Labels = @"
{
    ""type"": ""message"",
    ""actionName"": ""YourAction1"",
    ""funcName"": ""SpawnLabelList"",
    ""data"": [
        {""identifyID"": ""1"", ""prefabKey"": ""Lable"", ""title"": ""Label A"", ""desc"": ""Running normally"", ""deviceName"": ""Device_001""},
        {""identifyID"": ""2"", ""prefabKey"": ""Lable"", ""title"": ""Label B"", ""desc"": ""Standby mode"", ""deviceName"": ""Device_002""}
    ]
}";

        private const string PingYourAction3 = @"
{
    ""type"": ""message"",
    ""actionName"": ""YourAction3"",
    ""funcName"": ""ExpPing"",
    ""data"": {}
}";

        private const string InvalidAction = @"
{
    ""type"": ""message"",
    ""actionName"": ""MissingAction"",
    ""funcName"": ""ExpPing"",
    ""data"": {}
}";

        private const string InvalidFunction = @"
{
    ""type"": ""message"",
    ""actionName"": ""YourAction3"",
    ""funcName"": ""MissingFunction"",
    ""data"": {}
}";

        private readonly WebMsgHandlerManager _msgManager;
        private readonly ActionStack _actionStack;
        private readonly ILabelManager _labelManager;

        public InputService(WebMsgHandlerManager msgManager, ActionStack actionStack, ILabelManager labelManager)
        {
            _msgManager = msgManager;
            _actionStack = actionStack;
            _labelManager = labelManager;
            Debug.Log("[InputService] constructed. 1=YourAction1 labels, 2=YourAction3 ping, 8=invalid func, 9=invalid action, 0=pop.");
        }

        public void Tick()
        {
            CheckInput();
        }

        public void CheckInput()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                SendExample(CreateYourAction1Labels).Forget();

            if (Input.GetKeyDown(KeyCode.Alpha2))
                SendExample(PingYourAction3).Forget();

            if (Input.GetKeyDown(KeyCode.Alpha8))
                SendExample(InvalidFunction).Forget();

            if (Input.GetKeyDown(KeyCode.Alpha9))
                SendExample(InvalidAction).Forget();

            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                _actionStack.Pop();
                _labelManager.DebugPoolStatus();
            }
        }

        private async UniTaskVoid SendExample(string json)
        {
            var result = await _msgManager.ReceiveMessageFromWeb(json);
            Debug.Log($"[InputService Result] {JsonConvert.SerializeObject(WebErrorResponse.FromResult(result), Formatting.Indented)}");
        }
    }
}
