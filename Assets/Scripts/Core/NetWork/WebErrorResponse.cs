[System.Serializable]
public sealed class WebErrorResponse
{
    public bool ok;
    public string code;
    public string message;
    public string actionName;
    public string funcName;
    public object data;

    public static WebErrorResponse FromResult(ActionExecutionResult result)
    {
        return new WebErrorResponse
        {
            ok = result.Success,
            code = result.ErrorCode.ToString(),
            message = result.Message,
            actionName = result.ActionName,
            funcName = result.FuncName,
            data = result.Data
        };
    }
}
