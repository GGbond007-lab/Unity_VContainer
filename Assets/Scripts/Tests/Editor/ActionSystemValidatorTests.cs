namespace UniVCon.Tests
{
    using NUnit.Framework;
    using UniVCon.Editor;

    public sealed class ActionSystemValidatorTests
    {
        [Test]
        public void ValidatorFindsNoIssuesInCurrentConfigs()
        {
            var errors = ActionSystemValidator.Validate();

            Assert.IsEmpty(errors);
        }
    }
}
