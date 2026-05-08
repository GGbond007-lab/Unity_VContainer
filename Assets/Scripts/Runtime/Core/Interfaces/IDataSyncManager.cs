namespace UniVCon
{
    public interface IDataSyncManager {
        void UpdateRealTimeData(string data);
        string GetCurrentData();
    }
}
