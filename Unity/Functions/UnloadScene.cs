#nullable enable
using Library.Events;
using Library.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Library.Functions
{
    public readonly struct UnloadScene
    {
        public UnloadScene(SceneInstance scene)
        {
            AsyncOperationHandle<SceneInstance> op = Addressables.UnloadSceneAsync(scene);
            op.Completed += (op) =>
            {
                if (op.OperationException is not null)
                {
                    Debug.LogException(op.OperationException);
                }
                else
                {
                    Host.VirtualMachine.Broadcast(new SceneUnloaded(op.Task.Result.Scene));
                }
            };
        }
    }
}