// WebApi/Requests/SceneLoadRequest.cs
using System;

[Serializable]
public class SceneLoadRequest : WebRequest
{
    public string sceneName;
}