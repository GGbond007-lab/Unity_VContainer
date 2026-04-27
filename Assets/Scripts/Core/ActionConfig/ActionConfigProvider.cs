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

        // 注意：你文件夹是 EventConfigs 吗？如果是 ActionConfigs 就改这里
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

    // 🔥 🔥 🔥 已修复：用 targetEventClassName 查找
    public static ActionConfigSO GetConfigByTargetScript(Type targetType)
    {
        if (targetType == null) return null;

        foreach (var config in AllConfigs().Values)
        {
            // 用 类名字符串 匹配，不再使用 MonoScript
            if (config.targetEventClassName == targetType.FullName)
            {
                return config;
            }
        }
        return null;
    }
}