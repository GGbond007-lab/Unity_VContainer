namespace UniVCon
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    public enum ActionStackState {
        Idle, Pushing, Popping, Clearing
    }
    public enum ActionPushPolicy {
        AllowDuplicates, RejectSameTypeOnTop, BringExistingToTop
    }
    public sealed class ActionStack {
        private readonly Stack<IBaseAction> _stack = new();
        private readonly object _sync = new();
        private IBaseAction _lastAction;
        public ActionStackState State {
            get;
            private set;
        }
        = ActionStackState.Idle;
        public bool IsBusy => State != ActionStackState.Idle;
        public int Count {
            get {
                lock (_sync) {
                    return _stack.Count;
                }
            }
        }
        public ActionExecutionResult TryPush( IBaseAction action, ActionPushPolicy policy = ActionPushPolicy.RejectSameTypeOnTop) {
            if (action == null) {
                return ActionExecutionResult.Fail(ActionErrorCode.ExecutionFailed, "Cannot push a null action.");
            }
            ActionExecutionResult result;
            IBaseAction actionToDestroy = null;
            IBaseAction actionToInitialize = null;
            List<IBaseAction> poppedActions = null;
            lock (_sync) {
                if (IsBusy) {
                    return ActionExecutionResult.Fail( ActionErrorCode.ActionBusy, $"ActionStack is busy: {State}.", action.GetType().Name);
                }
                State = ActionStackState.Pushing;
                try {
                    if (policy == ActionPushPolicy.RejectSameTypeOnTop && _stack.Count > 0 && _stack.Peek().GetType() == action.GetType()) {
                        actionToDestroy = action;
                        result = ActionExecutionResult.Fail( ActionErrorCode.DuplicateAction, $"{action.GetType().Name} is already on top.", action.GetType().Name);
                    }
                    else if (policy == ActionPushPolicy.BringExistingToTop) {
                        var existing = FindActionUnsafe(action.GetType());
                        if (existing != null) {
                            actionToDestroy = action;
                            PopToUnsafe(existing, out poppedActions);
                            result = ActionExecutionResult.Ok(existing.GetType().Name);
                        }
                        else {
                            if (_stack.Count > 0) _lastAction = _stack.Peek();
                            _stack.Push(action);
                            actionToInitialize = action;
                            Debug.Log($"[ActionStack] Push {action.GetType().Name}, count={_stack.Count}");
                            result = ActionExecutionResult.Ok(action.GetType().Name);
                        }
                    }
                    else {
                        if (_stack.Count > 0) _lastAction = _stack.Peek();
                        _stack.Push(action);
                        actionToInitialize = action;
                        Debug.Log($"[ActionStack] Push {action.GetType().Name}, count={_stack.Count}");
                        result = ActionExecutionResult.Ok(action.GetType().Name);
                    }
                }
                finally {
                    State = ActionStackState.Idle;
                }
            }
            actionToDestroy?.OnDestroy();
            DestroyActions(poppedActions);
            if (actionToInitialize != null) {
                actionToInitialize.OnInitialize();
                actionToInitialize.OnPushed();
            }
            return result;
        }
        public void Push(IBaseAction action) {
            TryPush(action);
        }
        public IBaseAction Pop() {
            IBaseAction action;
            lock (_sync) {
                if (_stack.Count == 0 || IsBusy) return null;
                State = ActionStackState.Popping;
                try {
                    action = _stack.Pop();
                    Debug.Log($"[ActionStack] Pop {action.GetType().Name}, count={_stack.Count}");
                }
                finally {
                    State = ActionStackState.Idle;
                }
            }
            action.OnDestroy();
            return action;
        }
        public T FindAction<T>() where T : class, IBaseAction {
            lock (_sync) {
                return _stack.OfType<T>().FirstOrDefault();
            }
        }
        public IBaseAction GetCurrentAction() {
            lock (_sync) {
                return _stack.Count > 0 ? _stack.Peek() : null;
            }
        }
        public IBaseAction GetLastAction() {
            lock (_sync) {
                return _lastAction;
            }
        }
        public bool IsAtBottom(IBaseAction action) {
            lock (_sync) {
                return _stack.Count == 1 && _stack.Peek() == action;
            }
        }
        public bool PopTo<T>() where T : class, IBaseAction {
            return PopTo(FindAction<T>());
        }
        public bool PopTo(IBaseAction targetAction) {
            if (targetAction == null) return false;
            List<IBaseAction> poppedActions;
            lock (_sync) {
                if (IsBusy) return false;
                State = ActionStackState.Popping;
                try {
                    if (!PopToUnsafe(targetAction, out poppedActions)) return false;
                }
                finally {
                    State = ActionStackState.Idle;
                }
            }
            DestroyActions(poppedActions);
            return true;
        }
        public void PopAll() {
            List<IBaseAction> poppedActions = new();
            lock (_sync) {
                if (IsBusy) return;
                State = ActionStackState.Clearing;
                try {
                    while (_stack.Count > 0) {
                        poppedActions.Add(_stack.Pop());
                    }
                }
                finally {
                    State = ActionStackState.Idle;
                }
            }
            DestroyActions(poppedActions);
        }
        private IBaseAction FindActionUnsafe(System.Type actionType) {
            foreach (var action in _stack) {
                if (action.GetType() == actionType) return action;
            }
            return null;
        }
        private bool PopToUnsafe(IBaseAction targetAction) {
            return PopToUnsafe(targetAction, out _);
        }
        private bool PopToUnsafe(IBaseAction targetAction, out List<IBaseAction> poppedActions) {
            poppedActions = new List<IBaseAction>();
            if (!_stack.Contains(targetAction)) return false;
            while (_stack.Count > 0 && _stack.Peek() != targetAction) {
                poppedActions.Add(_stack.Pop());
            }
            return true;
        }
        private static void DestroyActions(IEnumerable<IBaseAction> actions) {
            if (actions == null) return;
            foreach (var action in actions) {
                action?.OnDestroy();
            }
        }
    }
}
