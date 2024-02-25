#nullable enable
using UnityLibrary.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Game.FunctionsLibrary;
using Game;
using Game.Functions;

namespace UnityLibrary
{
    public class ReleaseAssetFunction : IFunction<ReleaseAsset>
    {
        object? IFunction.Invoke(VirtualMachine vm, FunctionInput inputs)
        {
            Object? asset = inputs.Get(0) as Object;
            if (asset != null)
            {
                Addressables.Release(asset);
            }

            return null;
        }
    }
}
