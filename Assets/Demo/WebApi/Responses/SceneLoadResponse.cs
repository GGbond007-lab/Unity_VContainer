// WebApi/Responses/SceneLoadResponse.cs
using System;

[Serializable]
public class SceneLoadResponse : WebResponse
{
    public string sceneName;
    public string status; // "loaded"
}