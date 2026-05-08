namespace UniVCon
{
    using UnityEngine;
    public interface IStartupSettingsProvider {
        string InitialSceneName {
            get;
        }
        bool EnableDemoInput {
            get;
        }
    }
    public sealed class StartupSettingsProvider : IStartupSettingsProvider {
        private const string ResourcePath = "AppStartupSettings";
        private const string DefaultInitialSceneName = "ExampleScene1";
        private readonly AppStartupSettings _settings;
        public StartupSettingsProvider() {
            _settings = Resources.Load<AppStartupSettings>(ResourcePath);
        }
        public string InitialSceneName {
            get {
                if (_settings != null && !string.IsNullOrWhiteSpace(_settings.InitialSceneName)) return _settings.InitialSceneName;
                return DefaultInitialSceneName;
            }
        }
        public bool EnableDemoInput => _settings != null && _settings.EnableDemoInput;
    }
}
