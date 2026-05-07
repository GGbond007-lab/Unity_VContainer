using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using VContainer.StateMachine;

public class StateMachineExample : MonoBehaviour
{
    private StateMachineFactory stateMachineFactory;
    private StateMachine stateMachine;

    [Inject]
    public void Construct(StateMachineFactory factory)
    {
        stateMachineFactory = factory;
    }

    private void Start()
    {
        if (stateMachineFactory == null)
        {
            Debug.LogWarning("[StateMachineExample] StateMachineFactory is not injected. Skipping example setup.");
            enabled = false;
            return;
        }

        // 创建状态机
        stateMachine = stateMachineFactory.Create();
        
        // 创建任务状态实例并添加回调
        var taskStartState = new TaskStartState();
        taskStartState.OnEnter += () => Debug.Log("Task Start State Entered - Callback triggered!");
        taskStartState.OnUpdate += () => Debug.Log("Task Start State Updating - Callback triggered!");
        taskStartState.OnExit += () => Debug.Log("Task Start State Exited - Callback triggered!");
        
        var taskRunningState = new TaskRunningState();
        taskRunningState.OnEnter += () => Debug.Log("Task Running State Entered - Callback triggered!");
        taskRunningState.OnUpdate += () => Debug.Log("Task Running State Updating - Callback triggered!");
        taskRunningState.OnExit += () => Debug.Log("Task Running State Exited - Callback triggered!");
        
        var taskCompletedState = new TaskCompletedState();
        taskCompletedState.OnEnter += () => Debug.Log("Task Completed State Entered - Callback triggered!");
        taskCompletedState.OnUpdate += () => Debug.Log("Task Completed State Updating - Callback triggered!");
        taskCompletedState.OnExit += () => Debug.Log("Task Completed State Exited - Callback triggered!");
        
        var taskFailedState = new TaskFailedState();
        taskFailedState.OnEnter += () => Debug.Log("Task Failed State Entered - Callback triggered!");
        taskFailedState.OnUpdate += () => Debug.Log("Task Failed State Updating - Callback triggered!");
        taskFailedState.OnExit += () => Debug.Log("Task Failed State Exited - Callback triggered!");
        
        var taskCancelledState = new TaskCancelledState();
        taskCancelledState.OnEnter += () => Debug.Log("Task Cancelled State Entered - Callback triggered!");
        taskCancelledState.OnUpdate += () => Debug.Log("Task Cancelled State Updating - Callback triggered!");
        taskCancelledState.OnExit += () => Debug.Log("Task Cancelled State Exited - Callback triggered!");
        
        // 添加状态
        stateMachine.AddState("TaskStart", taskStartState);
        stateMachine.AddState("TaskRunning", taskRunningState);
        stateMachine.AddState("TaskCompleted", taskCompletedState);
        stateMachine.AddState("TaskFailed", taskFailedState);
        stateMachine.AddState("TaskCancelled", taskCancelledState);
        
        // 初始状态（同步）
        //stateMachine.ChangeState("TaskStart");
        
        // 示例：使用异步状态转换模拟任务流程
        ExampleTaskFlow().Forget();
    }

    private void Update()
    {
        if (stateMachine == null)
            return;

        // 同步更新
        stateMachine.Update();
        
        //// 异步更新
        //if (Input.GetKeyDown(KeyCode.U))
        //{
        //    stateMachine.UpdateAsync();
        //}
        
        // 同步状态切换
        if (Input.GetKeyDown(KeyCode.S))
        {
            stateMachine.ChangeState("TaskStart");
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            stateMachine.ChangeState("TaskRunning");
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            stateMachine.ChangeState("TaskCompleted");
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            stateMachine.ChangeState("TaskFailed");
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            stateMachine.ChangeState("TaskCancelled");
        }
        
        //// 异步状态切换
        //if (Input.GetKeyDown(KeyCode.Alpha1))
        //{
        //    stateMachine.ChangeStateAsync("TaskStart");
        //}
        //else if (Input.GetKeyDown(KeyCode.Alpha2))
        //{
        //    stateMachine.ChangeStateAsync("TaskRunning");
        //}
        //else if (Input.GetKeyDown(KeyCode.Alpha3))
        //{
        //    stateMachine.ChangeStateAsync("TaskCompleted");
        //}
        //else if (Input.GetKeyDown(KeyCode.Alpha4))
        //{
        //    stateMachine.ChangeStateAsync("TaskFailed");
        //}
        //else if (Input.GetKeyDown(KeyCode.Alpha5))
        //{
        //    stateMachine.ChangeStateAsync("TaskCancelled");
        //}
    }
    
    private async UniTaskVoid ExampleTaskFlow()
    {
        Debug.Log("=== 任务流程示例 ===");
        
        // 任务开始
        await stateMachine.ChangeStateAsync("TaskStart");
        
        // 模拟任务准备时间
        await UniTask.Delay(1000);
        
        // 任务进行中
        await stateMachine.ChangeStateAsync("TaskRunning");
        
        // 模拟任务执行时间
        await UniTask.Delay(3000);
        
        // 随机选择任务结果
        var random = new System.Random();
        int result = random.Next(3);
        
        if (result == 0)
        {
            // 任务完成
            await stateMachine.ChangeStateAsync("TaskCompleted");
        }
        else if (result == 1)
        {
            // 任务失败
            await stateMachine.ChangeStateAsync("TaskFailed");
        }
        else
        {
            // 任务取消
            await stateMachine.ChangeStateAsync("TaskCancelled");
        }
        
        // 等待2秒后重新开始
        await UniTask.Delay(2000);
        ExampleTaskFlow().Forget();
    }
}
