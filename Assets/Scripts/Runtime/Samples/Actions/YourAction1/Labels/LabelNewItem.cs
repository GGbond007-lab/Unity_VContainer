namespace UniVCon
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;
    public class LabelNewItem : MonoBehaviour, ILabel {
        private string _identifyID;
        [Header("基础组件")] public Button actionButton1;
        public Button actionButton2;
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
        public void SetClickEvent1(Action onClick) {
            _onClick = onClick;
            actionButton1.onClick.RemoveAllListeners();
            actionButton1.onClick.AddListener(() => _onClick?.Invoke());
        }
        public void SetClickEvent2(Action onClick) {
            _onClick = onClick;
            actionButton2.onClick.RemoveAllListeners();
            actionButton2.onClick.AddListener(() => _onClick?.Invoke());
        }
    }
}
