using System;
using System.Collections.Generic;
using UnityEngine;

public static class ActionConfigProvider
{
    private static Dictionary<string, ActionConfigSO> _configDict;

    public static IReadOnlyDictionary<string, ActionConfigSO> AllConfigs()
    {
        if (_configDict == null)
            LoadAllConfigs();
        return _configDict;
    }

    private static void LoadAllConfigs()
    {
        _configDict = new Dictionary<string, ActionConfigSO>();

        var configs = Resources.LoadAll<ActionConfigSO>("ActionConfigs");
        foreach (var c in configs)
        {
            if (!string.IsNullOrEmpty(c.actionName))
                _configDict[c.actionName] = c;
        }
    }

    public static ActionConfigSO GetConfig(string actionName)
    {
        if (_configDict == null)
            LoadAllConfigs();

        _configDict.TryGetValue(actionName, out var config);
        return config;
    }

    public static ActionConfigSO GetConfigByTargetAction(Type targetType)
    {
        if (targetType == null) return null;

        foreach (var config in AllConfigs().Values)
        {
            if (config.targetActionClassName == targetType.FullName)
            {
                return config;
            }
        }
        return null;
    }

    public static ScriptableObject GetScriptableObjectByType(Type soType)
    {
        if (soType == null || !typeof(ScriptableObject).IsAssignableFrom(soType))
            return null;

        var allSOs = Resources.LoadAll<ScriptableObject>("");
        foreach (var so in allSOs)
        {
            if (so.GetType() == soType || soType.IsAssignableFrom(so.GetType()))
                return so;
        }
        return null;
    }
}
