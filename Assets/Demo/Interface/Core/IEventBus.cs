
public interface IEventBus
{
    void Publish<T>(T evt) where T : IBaseEvent;
    void Subscribe<T>(System.Action<T> callback) where T : IBaseEvent;
    void UnSubscribe<T>(System.Action<T> callback) where T : IBaseEvent;
}