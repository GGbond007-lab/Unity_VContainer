namespace UniVCon.StateMachine
{
    using VContainer;
    public class StateMachineFactory {
        private readonly IObjectResolver resolver;
        public StateMachineFactory(IObjectResolver resolver) {
            this.resolver = resolver;
        }
        public StateMachine Create() {
            return new StateMachine(resolver);
        }
    }
}
