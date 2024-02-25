#nullable enable
using UnityLibrary.Unity;
using UnityEditor;
using UnityEngine;
using Game.FunctionsLibrary;
using Game;
using Game.Functions;

namespace UnityLibrary
{
    public class LoadAssetFromAssetsDatabaseFunction : IFunction<LoadAssetFromAssetsDatabase>
    {
        object? IFunction.Invoke(VirtualMachine vm, FunctionInput inputs)
        {
            string? assetPath = inputs.Get(0) as string;
            if (assetPath is not null)
            {
                return AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            }
            else
            {
                return null;
            }
        }
    }
}