#nullable enable
using Game;
using Game.Events;
using Game.Systems;
using System;
using UnityLibrary.Systems;
using UnityLibrary.Unity;

namespace UnityLibrary
{
    /// <summary>
    /// Added by <see cref="UnityApplication"/> before its virtual machine initializes.
    /// </summary>
    public class AllEditorSystems : IDisposable, IListener<SystemAdded>
    {
        private readonly VirtualMachine vm;

        public AllEditorSystems(VirtualMachine vm)
        {
            this.vm = vm;
            vm.AddSystem<PlayValidationTester>();
            vm.AddSystem<CustomPlayButton>();
            vm.AddSystem<TestBeforeEnteringPlay>();
        }

        public void Dispose()
        {
            vm.RemoveSystem<TestBeforeEnteringPlay>().Dispose();
            vm.RemoveSystem<CustomPlayButton>().Dispose();
            vm.RemoveSystem<PlayValidationTester>();
        }

        void IListener<SystemAdded>.Receive(VirtualMachine vm, ref SystemAdded e)
        {
            if (e.type == typeof(FunctionSystem))
            {
                FunctionSystem functions = vm.GetSystem<FunctionSystem>();
                functions.ImplementFunction(new LoadAssetFromAssetsDatabaseFunction());
                functions.ImplementFunction(new LoadAssetsFromAssetsDatabaseFunction());
            }
        }
    }
}