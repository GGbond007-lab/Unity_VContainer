using NUnit.Framework;

public sealed class ActionBusTests
{
    [Test]
    public void PublishContinuesWhenSubscriberThrows()
    {
        var bus = new ActionBus();
        var called = false;

        bus.Subscribe<string>(_ => throw new System.InvalidOperationException("boom"));
        bus.Subscribe<string>(_ => called = true);

        bus.Publish("hello");

        Assert.IsTrue(called);
    }

    [Test]
    public void DisposedSubscriptionStopsReceivingMessages()
    {
        var bus = new ActionBus();
        var count = 0;
        var subscription = bus.Subscribe<int>(_ => count++);

        bus.Publish(1);
        subscription.Dispose();
        subscription.Dispose();
        bus.Publish(2);

        Assert.AreEqual(1, count);
    }
}
