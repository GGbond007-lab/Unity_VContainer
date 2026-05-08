namespace UniVCon
{
    using System;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using VContainer;
    using VContainer.Unity;
    using UniVCon.StateMachine;
    public class StateMachineExample : MonoBehaviour {
        private StateMachineFactory stateMachineFactory;
        private UniVCon.StateMachine.StateMachine stateMachine;
        [Inject] public void Construct(StateMachineFactory factory) {
            stateMachineFactory = factory;
        }
        private void Start() {
            if (stateMachineFactory == null) {
                Debug.LogWarning("[StateMachineExample] StateMachineFactory is not injected. Skipping example setup.");
                enabled = false;
                return;
            }
            stateMachine = stateMachineFactory.Create();
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
            stateMachine.AddState("TaskRunning", taskRunningState);
            stateMachine.AddState("TaskCompleted", taskCompletedState);
            stateMachine.AddState("TaskFailed", taskFailedState);
            stateMachine.AddState("TaskCancelled", taskCancelledState);
        }
        private void Update() {
            if (stateMachine == null) return;
            if (Input.GetKeyDown(KeyCode.S)) {
                stateMachine.ChangeState("TaskStart");
            }
            else if (Input.GetKeyDown(KeyCode.R)) {
                stateMachine.ChangeState("TaskRunning");
            }
            else if (Input.GetKeyDown(KeyCode.C)) {
                stateMachine.ChangeState("TaskCompleted");
            }
            else if (Input.GetKeyDown(KeyCode.F)) {
                stateMachine.ChangeState("TaskFailed");
            }
            else if (Input.GetKeyDown(KeyCode.X)) {
                stateMachine.ChangeState("TaskCancelled");
            }
        }
        private async UniTaskVoid ExampleTaskFlow() {
            Debug.Log("=== 浠诲姟娴佺▼绀轰緥 ===");
            var random = new System.Random();
            int result = random.Next(3);
            if (result == 0) {
            }
            else if (result == 1) {
            }
            else {
            }
            ExampleTaskFlow().Forget();
        }
    }
}
