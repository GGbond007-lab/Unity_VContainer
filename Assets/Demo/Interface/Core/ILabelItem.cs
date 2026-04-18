public interface ILabelItem
{
    void SetData(object data);
    void SetClickEvent(System.Action onClick);
    void Refresh();
}