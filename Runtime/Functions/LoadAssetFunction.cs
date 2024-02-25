#nullable enable
using UnityLibrary.Unity;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;
using Game.FunctionsLibrary;
using Game.Functions;
using Game;

namespace UnityLibrary
{
    public class LoadAssetFunction : IFunction<LoadAsset>
    {
        object? IFunction.Invoke(VirtualMachine vm, FunctionInput inputs)
        {
            object? address = inputs.Get(0);
            Action<object>? callback = inputs.Get(1) as Action<object>;
            if (address is not null)
            {
                var op = Addressables.LoadAssetAsync<Object>(address);
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
