#nullable enable
using Game;

namespace UnityLibrary
{
    public class TestState : VirtualMachine.IState
    {
        public bool initialized;
        public bool finalized;

        void VirtualMachine.IState.Initialize(VirtualMachine vm)
        {
            initialized = true;
        }

        void VirtualMachine.IState.Finalize(VirtualMachine vm)
        {
            finalized = true;
        }
    }
}