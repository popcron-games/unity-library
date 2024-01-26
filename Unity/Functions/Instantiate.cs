#nullable enable
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Library.Unity
{
    public readonly struct Instantiate
    {
        public Instantiate(ReadOnlySpan<char> address, Action<GameObject>? callback = null)
        {
            AsyncOperationHandle<GameObject> op = Addressables.InstantiateAsync(address.ToString());
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
        }

        public Instantiate(ReadOnlySpan<char> address, Transform parent, Action<GameObject>? callback = null)
        {
            AsyncOperationHandle<GameObject> op = Addressables.InstantiateAsync(address.ToString(), parent);
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
        }
    }
}
