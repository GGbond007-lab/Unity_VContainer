using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.StateMachine;

namespace VContainer.StateMachine
{
    public class TaskStartState : IState
    {
        public event Action OnEnter;
        public event Action OnUpdate;
        public event Action OnExit;

        public void Enter()
        {
            //Debug.Log("Task Start State Entered");
            OnEnter?.Invoke();
        }

        public void Update()
        {
            //Debug.Log("Task Start State Updating");
            OnUpdate?.Invoke();
        }

        public void Exit()
        {
            //Debug.Log("Task Start State Exited");
            OnExit?.Invoke();
        }

        public async UniTask EnterAsync()
        {
            Debug.Log("Task Start State Entering (Async)");
            OnEnter?.Invoke();
            await UniTask.Delay(500);
            Debug.Log("Task Start State Entered (Async)");
        }

        public async UniTask UpdateAsync()
        {
            Debug.Log("Task Start State Updating (Async)");
            OnUpdate?.Invoke();
            await UniTask.Yield();
        }

        public async UniTask ExitAsync()
        {
            Debug.Log("Task Start State Exiting (Async)");
            OnExit?.Invoke();
            await UniTask.Delay(300);
            Debug.Log("Task Start State Exited (Async)");
        }
    }

    public class TaskRunningState : IState
    {
        public event Action OnEnter;
        public event Action OnUpdate;
        public event Action OnExit;

        public void Enter()
        {
            //Debug.Log("Task Running State Entered");
            OnEnter?.Invoke();
        }

        public void Update()
        {
            //Debug.Log("Task Running State Updating");
            OnUpdate?.Invoke();
        }

        public void Exit()
        {
            //Debug.Log("Task Running State Exited");
            OnExit?.Invoke();
        }

        public async UniTask EnterAsync()
        {
            Debug.Log("Task Running State Entering (Async)");
            OnEnter?.Invoke();
            await UniTask.Delay(300);
            Debug.Log("Task Running State Entered (Async)");
        }

        public async UniTask UpdateAsync()
        {
            Debug.Log("Task Running State Updating (Async)");
            OnUpdate?.Invoke();
            await UniTask.Yield();
        }

        public async UniTask ExitAsync()
        {
            Debug.Log("Task Running State Exiting (Async)");
            OnExit?.Invoke();
            await UniTask.Delay(300);
            Debug.Log("Task Running State Exited (Async)");
        }
    }

    public class TaskCompletedState : IState
    {
        public event Action OnEnter;
        public event Action OnUpdate;
        public event Action OnExit;

        public void Enter()
        {
            //Debug.Log("Task Completed State Entered");
            OnEnter?.Invoke();
        }

        public void Update()
        {
            //Debug.Log("Task Completed State Updating");
            OnUpdate?.Invoke();
        }

        public void Exit()
        {
            //Debug.Log("Task Completed State Exited");
            OnExit?.Invoke();
        }

        public async UniTask EnterAsync()
        {
            Debug.Log("Task Completed State Entering (Async)");
            OnEnter?.Invoke();
            await UniTask.Delay(800);
            Debug.Log("Task Completed State Entered (Async)");
        }

        public async UniTask UpdateAsync()
        {
            Debug.Log("Task Completed State Updating (Async)");
            OnUpdate?.Invoke();
            await UniTask.Yield();
        }

        public async UniTask ExitAsync()
        {
            Debug.Log("Task Completed State Exiting (Async)");
            OnExit?.Invoke();
            await UniTask.Delay(300);
            Debug.Log("Task Completed State Exited (Async)");
        }
    }

    public class TaskFailedState : IState
    {
        public event Action OnEnter;
        public event Action OnUpdate;
        public event Action OnExit;

        public void Enter()
        {
            //Debug.Log("Task Failed State Entered");
            OnEnter?.Invoke();
        }

        public void Update()
        {
            //Debug.Log("Task Failed State Updating");
            OnUpdate?.Invoke();
        }

        public void Exit()
        {
            //Debug.Log("Task Failed State Exited");
            OnExit?.Invoke();
        }

        public async UniTask EnterAsync()
        {
            Debug.Log("Task Failed State Entering (Async)");
            OnEnter?.Invoke();
            await UniTask.Delay(800);
            Debug.Log("Task Failed State Entered (Async)");
        }

        public async UniTask UpdateAsync()
        {
            Debug.Log("Task Failed State Updating (Async)");
            OnUpdate?.Invoke();
            await UniTask.Yield();
        }

        public async UniTask ExitAsync()
        {
            Debug.Log("Task Failed State Exiting (Async)");
            OnExit?.Invoke();
            await UniTask.Delay(300);
            Debug.Log("Task Failed State Exited (Async)");
        }
    }

    public class TaskCancelledState : IState
    {
        public event Action OnEnter;
        public event Action OnUpdate;
        public event Action OnExit;

        public void Enter()
        {
            //Debug.Log("Task Cancelled State Entered");
            OnEnter?.Invoke();
        }

        public void Update()
        {
            //Debug.Log("Task Cancelled State Updating");
            OnUpdate?.Invoke();
        }

        public void Exit()
        {
            //Debug.Log("Task Cancelled State Exited");
            OnExit?.Invoke();
        }

        public async UniTask EnterAsync()
        {
            Debug.Log("Task Cancelled State Entering (Async)");
            OnEnter?.Invoke();
            await UniTask.Delay(600);
            Debug.Log("Task Cancelled State Entered (Async)");
        }

        public async UniTask UpdateAsync()
        {
            Debug.Log("Task Cancelled State Updating (Async)");
            OnUpdate?.Invoke();
            await UniTask.Yield();
        }

        public async UniTask ExitAsync()
        {
            Debug.Log("Task Cancelled State Exiting (Async)");
            OnExit?.Invoke();
            await UniTask.Delay(300);
            Debug.Log("Task Cancelled State Exited (Async)");
        }
    }
}