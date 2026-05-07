#if UNITY_EDITOR
using ActionSystem.Editor;
using NUnit.Framework;

public sealed class ActionSystemValidatorTests
{
    [Test]
    public void ValidatorFindsNoIssuesInCurrentConfigs()
    {
        var errors = ActionSystemValidator.Validate();

        Assert.IsEmpty(errors);
    }
}
#endif
