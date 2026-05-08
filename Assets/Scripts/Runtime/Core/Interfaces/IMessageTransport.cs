namespace UniVCon
{
    using Cysharp.Threading.Tasks;
    public interface IMessageTransport {
        UniTask SendAsync(string json);
    }
}
