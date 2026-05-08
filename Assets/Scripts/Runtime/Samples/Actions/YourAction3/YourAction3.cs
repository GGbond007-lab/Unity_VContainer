namespace UniVCon
{
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    public class YourAction3 : BaseAction {
        public override void OnInitialize() {
            Debug.Log("[YourAction3] Exp OnInitialize.");
        }
        public UniTask ExpPing() {
            Debug.Log("[YourAction3] ExpPing called.");
            return UniTask.CompletedTask;
        }
        public UniTask ExpReceiveData(object data) {
            if (TryConvertData(data, out Dictionary<string, object> payload)) {
                Debug.Log($"[YourAction3] ExpReceiveData keys: {string.Join(", ", payload.Keys)}");
            }
            else {
                Debug.LogWarning("[YourAction3] ExpReceiveData failed to convert payload.");
            }
            return UniTask.CompletedTask;
        }
        public async UniTask ExpCreateLabel(object data) {
            if (!TryConvertData(data, out List<LabelData> labels) || labels == null) {
                Debug.LogWarning("[YourAction3] ExpCreateLabel received invalid label data.");
                return;
            }
            foreach (var labelData in labels) {
                if (string.IsNullOrEmpty(labelData.prefabKey)) continue;
                var prefab = await LabelManager.LoadLabelPrefab(labelData.prefabKey);
                var label = LabelManager.CreateLabel(prefab);
                if (label == null) continue;
                label.SetData(labelData);
                label.Refresh();
            }
        }
        public UniTask ExpClearLabels() {
            LabelManager.ClearAllLabelsToPool();
            Debug.Log("[YourAction3] ExpClearLabels called.");
            return UniTask.CompletedTask;
        }
        public UniTask ExpAfterPing(object data) {
            Debug.Log("[YourAction3] ExpAfterPing callback called.");
            return UniTask.CompletedTask;
        }
        public UniTask ExpSendDataBack(object data) {
            MessageSender.SendActionMessage( type: "message", actionName: "YourAction3", funcName: "ExpReceiveDataCallback", data: new {
                actionId = ActionId, received = data
            }
            );
            return UniTask.CompletedTask;
        }
        public UniTask ExpOnOtherActionExecuted(ActionMethodExecutedMessage message) {
            Debug.Log($"[YourAction3] Exp observed {message.Action.GetType().Name}.{message.MethodName}");
            return UniTask.CompletedTask;
        }
    }
}
