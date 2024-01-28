#nullable enable
using Library.Systems;
using System;

namespace Library.Unity
{
    public class UnitySystems : IDisposable
    {
        private readonly VirtualMachine vm;

        public UnitySystems(VirtualMachine vm)
        {
            this.vm = vm;
            vm.AddSystem(new UnityObjects());
            vm.AddSystem(new UnityEventDispatcher());
            vm.AddSystem(new ManageInitialAssets());
            vm.AddSystem(new InputSystemEventDispatcher(vm));
        }

        public void Dispose()
        {
            vm.RemoveSystem<InputSystemEventDispatcher>();
            vm.RemoveSystem<ManageInitialAssets>().Dispose();
            vm.RemoveSystem<UnityEventDispatcher>().Dispose();
            vm.RemoveSystem<UnityObjects>();
        }
    }
}
