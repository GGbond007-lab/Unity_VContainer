using UnityEngine;
using VContainer;

public class Test : MonoBehaviour
{
    private YourEvent1 _event1;

    [Inject] private EventStack _eventStack;

    void Start()
    {

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 从事件栈中查找，只有消息创建过的事件才会被找到
            _event1 = _eventStack.FindEvent<YourEvent1>();

            if (_event1 != null)
            {
                Debug.Log("✅ 真的有一个已存在的 YourEvent1 实例！");
            }
            else
            {
                Debug.Log("❌ 没有已存在的实例，不会创建任何东西！");
            }
            // 空安全调用，没有实例时不会执行
            _event1?.Print1();
        }
    }
}