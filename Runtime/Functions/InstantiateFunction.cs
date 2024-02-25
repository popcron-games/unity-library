#nullable enable
using UnityLibrary.Unity;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Game.FunctionsLibrary;
using Game.Functions;
using Game;

namespace UnityLibrary
{
    public class InstantiateFunction : IFunction<Instantiate>
    {
        object? IFunction.Invoke(VirtualMachine vm, FunctionInput inputs)
        {
            object? address = inputs.Get(0);
            Transform? parent = inputs.Get(1) as Transform;
            Action<object>? callback = inputs.Get(2) as Action<object>;
            if (address is not null)
            {
                AsyncOperationHandle<GameObject> op = Addressables.InstantiateAsync(address, parent);
                op.Completed += (op) =>
                {
                    if (op.OperationException is not null)
                    {
                        Debug.LogException(op.OperationException);
                    }
                    else
                    {
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
