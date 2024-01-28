#nullable enable
using Library.Events;
using Library.Unity;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Library.Functions
{
    public readonly struct LoadScene
    {
        public LoadScene(ReadOnlySpan<char> address, Action<SceneInstance>? callback = null)
        {
            AsyncOperationHandle<SceneInstance> op = Addressables.LoadSceneAsync(address.ToString(), LoadSceneMode.Additive);
            op.Completed += (op) =>
            {
                if (op.OperationException is not null)
                {
                    Debug.LogException(op.OperationException);
                }
                else
                {
                    SceneInstance instance = op.Result;
                    op.Result.ActivateAsync().completed += (op) =>
                    {
                        if (!op.isDone)
                        {
                            Debug.LogError($"Couldn't activate {instance}");
                        }

                        SceneManager.SetActiveScene(instance.Scene);
                    };

                    callback?.Invoke(instance);
                    Host.VirtualMachine.Broadcast(new SceneLoaded(instance));
                }
            };
        }
    }
}