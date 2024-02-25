#nullable enable
using Game.Functions;
using Game.Systems;
using System;

namespace Game
{
    public class GameSystems : IDisposable
    {
        private readonly VirtualMachine vm;

        public GameSystems(VirtualMachine vm)
        {
            this.vm = vm;
            FunctionSystem functions = vm.AddSystem<FunctionSystem>();
            functions.RequireFunction<Instantiate>();
            functions.RequireFunction<Destroy>();
            functions.RequireFunction<LoadAsset>();
            functions.RequireFunction<ReleaseAsset>();
            functions.RequireFunction<LoadScene>();
            functions.RequireFunction<ReleaseScene>();
            functions.RequireFunction<LoadAssetFromAssetsDatabase>();
            functions.RequireFunction<LoadAssetsFromAssetsDatabase>();

            vm.AddSystem<InvokeScheduler>();
        }

        public void Dispose()
        {
            vm.RemoveSystem<InvokeScheduler>().Dispose();
            vm.RemoveSystem<FunctionSystem>().Dispose();
        }
    }
}