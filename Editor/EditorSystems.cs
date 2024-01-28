#nullable enable
using Library.Systems;
using Library.Unity;
using System;

namespace Library
{
    /// <summary>
    /// Added by <see cref="Host"/> before its virtual machine initializes.
    /// </summary>
    public class EditorSystems : IDisposable
    {
        private readonly VirtualMachine vm;

        public EditorSystems(VirtualMachine vm)
        {
            this.vm = vm;
            vm.AddSystem(new PlayValidationTester());
            vm.AddSystem(new CustomPlayButton(vm));
            vm.AddSystem(new TestBeforeEnteringPlay(vm));
        }

        public void Dispose()
        {
            vm.RemoveSystem<TestBeforeEnteringPlay>().Dispose();
            vm.RemoveSystem<CustomPlayButton>().Dispose();
            vm.RemoveSystem<PlayValidationTester>();
        }
    }
}