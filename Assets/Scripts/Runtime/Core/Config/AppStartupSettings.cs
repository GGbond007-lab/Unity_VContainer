namespace UniVCon
{
    using UnityEngine;
    [CreateAssetMenu(fileName = "AppStartupSettings", menuName = "UniVCon/App Startup Settings")] public sealed class AppStartupSettings : ScriptableObject {
        [SerializeField] private string initialSceneName = "ExampleScene1";
        [SerializeField] private bool enableDemoInput;
        public string InitialSceneName => initialSceneName;
        public bool EnableDemoInput => enableDemoInput;
    }
}
