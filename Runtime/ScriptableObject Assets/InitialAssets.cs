#nullable enable
using Game;
using Game.Events;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityLibrary
{
    [CreateAssetMenu(menuName = "Unity Library/Initial Assets")]
    public sealed class InitialAssets : CustomScriptableObject, IListener<TestEvent>, VirtualMachine.IInitialData
    {
        [SerializeField]
        private AddressableInitialAssets? addressableInitialAssets;

        [SerializeField]
        private List<Object> assets = new();

        public IReadOnlyList<object> GetAllThatAre<T>()
        {
            List<object> throwawayList = new();
            if (addressableInitialAssets != null && typeof(T) == addressableInitialAssets.GetType())
            {
                throwawayList.Add(addressableInitialAssets);
            }

            foreach (Object obj in assets)
            {
                if (obj is T t)
                {
                    throwawayList.Add(t);
                }
            }

            return throwawayList;
        }

        public bool Add(Object asset)
        {
            if (!assets.Contains(asset))
            {
                assets.Add(asset);
                return true;
            }
            else return false;
        }

        public bool Remove(Object asset)
        {
            if (assets.Contains(asset))
            {
                assets.Remove(asset);
                return true;
            }
            else return false;
        }

        void IListener<TestEvent>.Receive(VirtualMachine vm, ref TestEvent ev)
        {
            for (int i = 0; i < assets.Count; i++)
            {
                Object asset = assets[i];
                Assert.IsNotNull(asset, $"Asset is null at index {i} on {this}");
                if (asset is IListener<TestEvent> listener)
                {
                    listener.Receive(vm, ref ev);
                }
            }
        }
    }
}
