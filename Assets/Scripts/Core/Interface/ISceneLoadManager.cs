using Cysharp.Threading.Tasks;

public interface ISceneLoadManager
{
    UniTask LoadSceneAsync(string sceneName);
}
