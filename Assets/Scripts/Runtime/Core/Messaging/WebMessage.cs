namespace UniVCon
{
    [System.Serializable]
    public class WebMessageEFD
    {
        public string type;
        public string actionName;
        public string funcName;
        public object data;
    }

    [System.Serializable]
    public class WebMessageFD
    {
        public string type;
        public string funcName;
        public object data;
    }
}
