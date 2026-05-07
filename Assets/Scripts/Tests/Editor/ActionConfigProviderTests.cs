using NUnit.Framework;
using UnityEngine;

public sealed class ActionConfigProviderTests
{
    [Test]
    public void TestProvidersKeepIndependentConfigState()
    {
        var config = ScriptableObject.CreateInstance<ActionConfigSO>();
        config.actionName = "OnlyInFirst";
        config.targetActionClassName = nameof(TestAction);

        var first = new TestActionConfigProvider(new[] { config });
        var second = new TestActionConfigProvider();

        Assert.IsNotNull(first.GetConfig("OnlyInFirst"));
        Assert.IsNull(second.GetConfig("OnlyInFirst"));

        UnityEngine.Object.DestroyImmediate(config);
    }

    [Test]
    public void GetConfigByTargetActionAcceptsFullNameAndTypeNameKeys()
    {
        var config = ScriptableObject.CreateInstance<ActionConfigSO>();
        config.actionName = "Test";
        config.targetActionClassName = nameof(TestAction);

        var provider = new TestActionConfigProvider(new[] { config });

        Assert.AreSame(config, provider.GetConfigByTargetAction(typeof(TestAction)));

        UnityEngine.Object.DestroyImmediate(config);
    }
}
