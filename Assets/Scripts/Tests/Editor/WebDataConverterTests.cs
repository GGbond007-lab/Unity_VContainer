using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

public sealed class WebDataConverterTests
{
    [Test]
    public void ConvertsJTokenToTypedList()
    {
        var converter = new WebDataConverter();
        var token = JToken.Parse(@"[
            {""identifyID"": ""1"", ""prefabKey"": ""Lable"", ""title"": ""A"", ""desc"": ""B"", ""deviceName"": ""Device""}
        ]");

        var labels = converter.ConvertData<List<LabelData>>(token);

        Assert.AreEqual(1, labels.Count);
        Assert.AreEqual("Device", labels[0].deviceName);
    }

    [Test]
    public void TryConvertReturnsFalseForInvalidPayload()
    {
        var converter = new WebDataConverter();

        var ok = converter.TryConvertData<int>("not-an-int", out _);

        Assert.IsFalse(ok);
    }
}
