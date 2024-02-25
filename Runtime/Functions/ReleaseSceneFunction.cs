#nullable enable
using Game;
using Game.Functions;
using Game.FunctionsLibrary;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace UnityLibrary
{
    public class ReleaseSceneFunction : IFunction<ReleaseScene>
    {
        object? IFunction.Invoke(VirtualMachine vm, FunctionInput inputs)
        {
            object? address = inputs.Get(0);
            Action? callback = inputs.Get(1) as Action;
            if (address is not null)
            {
                int hash = address.GetHashCode();
                if (LoadedScenes.map.TryGetValue(hash, out SceneInstance instance))
                {
                    LoadedScenes.map.Remove(hash);
                    Addressables.UnloadSceneAsync(instance).Completed += (op) =>
                    {
                        if (op.OperationException != null)
                        {
                            Debug.LogException(op.OperationException);
                        }
                        else
                        {
                            callback?.Invoke();
                        }
                    };
                }
                else if (address is string text)
                {
                    for (int i = 0; i < SceneManager.loadedSceneCount; i++)
                    {
                        Scene scene = SceneManager.GetSceneAt(i);
                        if (scene.name == text || scene.path == text)
                        {
                            SceneManager.UnloadSceneAsync(scene).completed += (op) =>
                            {
                                callback?.Invoke();
                            };

                            break;
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Could not find scene with address '{address}'");
                }
            }

            return null;
        }
    }
}
