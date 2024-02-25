#nullable enable
using Game;
using Game.Functions;
using Game.FunctionsLibrary;
using System;
using UnityEditor;
using Object = UnityEngine.Object;

namespace UnityLibrary
{
    public class LoadAssetsFromAssetsDatabaseFunction : IFunction<LoadAssetsFromAssetsDatabase>
    {
        object? IFunction.Invoke(VirtualMachine vm, FunctionInput inputs)
        {
            string? searchFilter = inputs.Get(0) as string;
            Type? type = inputs.Get(1) as Type;
            if (searchFilter is not null)
            {
                string[] guids = AssetDatabase.FindAssets(searchFilter);
                Object[] assets = new Object[guids.Length];
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    assets[i] = AssetDatabase.LoadAssetAtPath(path, type);
                }

                return assets;
            }
            else
            {
                return null;
            }
        }
    }
}