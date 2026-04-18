public interface IEventLabelController
{
    void AddLabel(LabelItem label);
    // 新增：根据唯一ID查找标签
    LabelItem TryGetLabel(string targetId);
    void ClearAll();
    void RemoveLabel(LabelItem label);
    // 销毁控制器自身（用于清理根节点等资源）
    void Destroy();
}