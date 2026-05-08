namespace UniVCon
{
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using VContainer;
    public static partial class ActionRegistry {
        public delegate UniTask ActionMethodDelegate(BaseAction action, object data);
        public delegate UniTask SubscribeMethodDelegate(BaseAction listener, ActionMethodExecutedMessage message);
        public static void RegisterActions(IContainerBuilder builder) {
            foreach (var type in ActionTypes) {
                builder.Register(type, Lifetime.Transient).AsSelf();
            }
        }
        public static void RegisterLabelControllers(IContainerBuilder builder) {
            foreach (var type in LabelControllerTypes) {
                builder.Register(type, Lifetime.Transient).AsSelf();
            }
        }
        public static void RegisterStates(IContainerBuilder builder) {
            foreach (var type in StateTypes) {
                builder.Register(type, Lifetime.Transient);
            }
        }
        public static bool IsRegisteredAction(Type type) => Contains(ActionTypes, type);
        public static Type GetActionType(string typeName) {
            if (string.IsNullOrEmpty(typeName)) return null;
            foreach (var type in ActionTypes) {
                if (type.Name == typeName || type.FullName == typeName) return type;
            }
            return null;
        }
        public static bool TryGetWebCallableMethod(Type actionType, string methodName, out ActionMethodDelegate method) {
            return WebCallableMethods.TryGetValue((actionType, methodName), out method);
        }
        public static bool TryGetCallbackMethod(Type actionType, string methodName, out ActionMethodDelegate method) {
            return CallbackMethods.TryGetValue((actionType, methodName), out method);
        }
        public static bool TryGetSubscribeMethod(Type actionType, string methodName, out SubscribeMethodDelegate method) {
            return SubscribeMethods.TryGetValue((actionType, methodName), out method);
        }
        private static bool Contains(IReadOnlyList<Type> types, Type target) {
            foreach (var type in types) {
                if (type == target) return true;
            }
            return false;
        }
    }
}
