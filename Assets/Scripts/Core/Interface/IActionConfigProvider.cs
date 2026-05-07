using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IActionConfigProvider
{
    bool IsInitialized { get; }

    UniTask InitializeAsync();
    IReadOnlyDictionary<string, ActionConfigSO> AllConfigs();
    ActionConfigSO GetConfig(string actionName);
    ActionConfigSO GetConfigByTargetAction(Type targetType);
    ScriptableObject GetScriptableObjectByType(Type soType);
}
