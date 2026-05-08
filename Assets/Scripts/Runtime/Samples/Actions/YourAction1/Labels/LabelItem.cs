namespace UniVCon
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;
    public class LabelItem : MonoBehaviour, ILabel {
        private string _identifyID;
        [Header("基础组件")] public Button actionButton;
        public Text title;
        public Text description;
        protected object _data;
        public LabelData _typedData;
        private System.Action _onClick;
        public GameObject SourcePrefab;
        public string identifyID => _identifyID;
        public void Awake() {
            SourcePrefab = this.gameObject;
        }
        public virtual void SetData(object data) {
            _data = data;
            _typedData = data as LabelData;
            _identifyID = _typedData.identifyID;
            UpdateUI();
        }
        public void SetData(LabelData data) {
            _data = data;
            _typedData = data;
            UpdateUI();
        }
        public virtual void Refresh() {
            UpdateUI();
        }
        protected virtual void UpdateUI() {
            if (_typedData == null) return;
            if (title != null) title.text = _typedData.title;
            if(description != null) description.text = _typedData.desc;
        }
        protected virtual void OnDestroy() {
            _onClick = null;
        }
        public void SetClickEvent(Action onClick) {
            _onClick = onClick;
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() => _onClick?.Invoke());
        }
    }
}
