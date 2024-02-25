#nullable enable
using UnityLibrary.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Game.FunctionsLibrary;
using Game;
using Game.Functions;

namespace UnityLibrary
{
    public class DestroyFunction : IFunction<Destroy>
    {
        object? IFunction.Invoke(VirtualMachine vm, FunctionInput inputs)
        {
            Object? asset = inputs.Get(0) as Object;
            if (asset is GameObject gameObject)
            {
                Addressables.ReleaseInstance(gameObject);
            }
            else
            {
                Object.Destroy(asset);
            }

            return null;
        }
    }
}
