#nullable enable
using System;

namespace Library.Unity
{
    /// <summary>
    /// Added by <see cref="Host"/> through reflection.
    /// </summary>
    public class EditorSystems : IDisposable
    {
        private readonly VirtualMachine vm;

        public EditorSystems(VirtualMachine vm)
        {
            this.vm = vm;
            vm.AddSystem(new PlayValidationTester());
            vm.AddSystem(new CustomPlayButton(vm));
            vm.AddSystem(new EnterPlayValidationTester(vm));
        }

        public void Dispose()
        {
            vm.RemoveSystem<EnterPlayValidationTester>().Dispose();
            vm.RemoveSystem<CustomPlayButton>().Dispose();
            vm.RemoveSystem<PlayValidationTester>();
        }
    }
}