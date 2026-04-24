using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Analytics.IAnalytic;

public class YourAction2 : BaseAction
{
    #region 事件触发时处理的方法，四种重载可选
    // 1. 无参数
    //void Print() { Debug.Log("🔥 成功！YourEvent2.Print 执行了！"); }

    // 2. 接收数据（你传的数据类型）
    //void Print(List<LabelData> data) { }

    // 3. 接收事件本身
    //void Print(YourEvent1 evt) { Debug.Log("🔥 成功！:"+evt.EventId); }

    // 4. 接收 object 数据
    public void Print(object data) {
        if (data is not List<LabelData> dataList)
        {
            Debug.LogError("数据格式错误！");
            return;
        }
        foreach (var itemData in dataList)
        {
            Debug.Log("🔥 成功！:" + itemData.deviceName);
        }

    }
    #endregion
}
