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

        var configs = Resources.LoadAll<ActionConfigSO>("EventConfigs");
        foreach (var c in configs)
        {
            if (!string.IsNullOrEmpty(c.actionName))
                _configDict[c.actionName] = c;
        }
    }

    public static ActionConfigSO GetConfig(string eventName)
    {
        AllConfigs();
        _configDict.TryGetValue(eventName, out var config);
        return config;
    }
    // 🔥 按 targetEventScript 绑定的类类型查找配置
    public static ActionConfigSO GetConfigByTargetScript(Type targetType)
    {
        foreach (var config in AllConfigs().Values)
        {
            if (config.targetEventScript != null && config.targetEventScript.GetClass() == targetType)
            {
                return config;
            }
        }
        return null;
    }
}