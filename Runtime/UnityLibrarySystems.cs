#nullable enable
using Game;
using Game.Systems;
using System;
using UnityLibrary.Systems;

namespace UnityLibrary
{
    public class UnityLibrarySystems : IDisposable
    {
        private readonly VirtualMachine vm;

        public UnityLibrarySystems(VirtualMachine vm)
        {
            this.vm = vm;
            FunctionSystem functions = vm.GetSystem<FunctionSystem>();
            functions.ImplementFunction(new DestroyFunction());
            functions.ImplementFunction(new InstantiateFunction());
            functions.ImplementFunction(new LoadAssetFunction());
            functions.ImplementFunction(new LoadSceneFunction());
            functions.ImplementFunction(new ReleaseAssetFunction());
            functions.ImplementFunction(new ReleaseSceneFunction());
            vm.AddSystem<UnityObjects>();
            vm.AddSystem<UnityEventDispatcher>();
            vm.AddSystem<ManageAddressableInitialAssets>();
            vm.AddSystem<InputSystemEventDispatcher>();
        }

        public void Dispose()
        {
            vm.RemoveSystem<InputSystemEventDispatcher>().Dispose();
            vm.RemoveSystem<ManageAddressableInitialAssets>().Dispose();
            vm.RemoveSystem<UnityEventDispatcher>().Dispose();
            vm.RemoveSystem<UnityObjects>();
        }
    }
}
