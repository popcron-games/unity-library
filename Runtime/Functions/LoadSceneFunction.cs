#nullable enable
using UnityLibrary.Unity;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using Game.FunctionsLibrary;
using Game.Functions;
using Game;

namespace UnityLibrary
{
    public class LoadSceneFunction : IFunction<LoadScene>
    {
        object? IFunction.Invoke(VirtualMachine vm, FunctionInput inputs)
        {
            object? address = inputs.Get(0);
            Action<object>? callback = inputs.Get(1) as Action<object>;
            if (address is not null)
            {
                AsyncOperationHandle<SceneInstance> op = Addressables.LoadSceneAsync(address);
                op.Completed += (op) =>
                {
                    if (op.OperationException is not null)
                    {
                        callback?.Invoke(op.OperationException);
                    }
                    else
                    {
                        LoadedScenes.map.Add(address.GetHashCode(), op.Result);
                        callback?.Invoke(op.Result);
                    }
                };

                return op;
            }
            else
            {
                return null;
            }
        }
    }
}
