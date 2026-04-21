public interface ILabel
{
    string identifyID { get; }
    void SetData(object data);
    void Refresh();
}