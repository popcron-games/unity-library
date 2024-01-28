#nullable enable
namespace Library
{
    public class TestState : IState
    {
        public bool initialized;
        public bool finalized;

        void IState.Initialize(VirtualMachine vm)
        {
            initialized = true;
        }

        void IState.Finalize(VirtualMachine vm)
        {
            finalized = true;
        }
    }
}