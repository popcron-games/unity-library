#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Platformer.Functions
{
    public readonly struct LoadAssetsWithLabel<T>
    {
        public LoadAssetsWithLabel(ReadOnlySpan<char> label, Action<IList<T>> callback)
        {
            AsyncOperationHandle<IList<T>> op = Addressables.LoadAssetsAsync<T>(label.ToString(), null);
            op.Completed += (e) =>
            {
                if (e.OperationException is not null)
                {
                    Debug.LogException(e.OperationException);
                    callback.Invoke(new List<T>());
                }
                else
                {
                    callback.Invoke(e.Result);
                }
            };
        }
    }
}