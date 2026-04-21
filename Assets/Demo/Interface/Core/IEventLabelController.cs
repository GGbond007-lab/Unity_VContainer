using UnityEngine;

public interface IEventLabelController
{
    Transform RootTransform { get; }
    void Initialize();
    void AddLabel(ILabel label); 
    void RemoveLabel(ILabel label); 
    void ClearAll();
    void Destroy();
    ILabel TryGetLabel(string identifyID); 
}