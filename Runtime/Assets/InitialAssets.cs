#nullable enable
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityLibrary.Events;

namespace UnityLibrary
{
    [CreateAssetMenu(menuName = "Unity Library/Initial Assets")]
    public sealed class InitialAssets : CustomScriptableObject, IListener<Validate>, IInitialData
    {
        [SerializeField]
        private List<Object> assets = new();

        public IReadOnlyList<T> GetAllThatAre<T>()
        {
            List<T> throwawayList = new();
            foreach (Object obj in assets)
            {
                if (obj is T t)
                {
                    throwawayList.Add(t);
                }
            }

            return throwawayList;
        }

        public bool TryAdd(Object asset)
        {
            if (!assets.Contains(asset))
            {
                assets.Add(asset);
                return true;
            }
            else return false;
        }

        public bool TryRemove(Object asset)
        {
            if (assets.Contains(asset))
            {
                assets.Remove(asset);
                return true;
            }
            else return false;
        }

        void IListener<Validate>.Receive(VirtualMachine vm, ref Validate ev)
        {
            for (int i = 0; i < assets.Count; i++)
            {
                Object asset = assets[i];
                Assert.IsNotNull(asset, $"Asset is null at index {i} on {this}");
                if (asset is IListener<Validate> listener)
                {
                    listener.Receive(vm, ref ev);
                }
            }
        }
    }
}
