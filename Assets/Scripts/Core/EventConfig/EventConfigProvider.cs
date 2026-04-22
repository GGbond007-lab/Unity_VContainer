using System;
using System.Collections.Generic;
using UnityEngine;

public static class EventConfigProvider
{
    private static Dictionary<string, EventConfigSO> _configDict;

    public static IReadOnlyDictionary<string, EventConfigSO> AllConfigs()
    {
        if (_configDict == null)
            LoadAllConfigs();
        return _configDict;
    }

    private static void LoadAllConfigs()
    {
        _configDict = new Dictionary<string, EventConfigSO>();

        var configs = Resources.LoadAll<EventConfigSO>("EventConfigs");
        foreach (var c in configs)
        {
            if (!string.IsNullOrEmpty(c.eventName))
                _configDict[c.eventName] = c;
        }
    }

    public static EventConfigSO GetConfig(string eventName)
    {
        AllConfigs();
        _configDict.TryGetValue(eventName, out var config);
        return config;
    }
    // 🔥 按 targetEventScript 绑定的类类型查找配置
    public static EventConfigSO GetConfigByTargetScript(Type targetType)
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