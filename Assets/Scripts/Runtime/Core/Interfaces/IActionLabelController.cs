namespace UniVCon
{
    using UnityEngine;
    public interface IActionLabelController {
        Transform RootTransform {
            get;
        }
        void Initialize();
        void AddLabel(ILabel label);
        void RemoveLabel(ILabel label);
        void ClearAll();
        void Destroy();
        ILabel TryGetLabel(string identifyID);
    }
}
