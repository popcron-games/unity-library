#nullable enable
using Game;
using Game.Functions;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityLibrary.Events;
using UnityLibrary.Unity;

namespace UnityLibrary.Systems
{
    /// <summary>
    /// Loads and releases addressable assets assigned on the initial assets.
    /// </summary>
    public class ManageAddressableInitialAssets : IDisposable, IListener<ApplicationStarted>, IListener<ApplicationStopped>
    {
        private readonly HashSet<GameObject> instances = new();
        private readonly CancellationTokenSource cts = new();
        private readonly VirtualMachine vm;

        public ManageAddressableInitialAssets(VirtualMachine vm)
        {
            this.vm = vm;
            Addressables.InitializeAsync();
        }

        public void Dispose()
        {
            cts.Cancel();
            cts.Dispose();
        }

        void IListener<ApplicationStarted>.Receive(VirtualMachine vm, ref ApplicationStarted e)
        {
            UnityApplicationSettings settings = UnityApplicationSettings.Singleton;
            Transform parent = new GameObject(nameof(AddressableInitialAssets)).transform;
            UnityEngine.Object.DontDestroyOnLoad(parent.gameObject);
            bool initializedAny = false;
            foreach (AddressableInitialAssets initialAssets in settings.GetAllThatAre<AddressableInitialAssets>())
            {
                initializedAny = true;
                CreateObjects(initialAssets.SystemPrefabs, parent);
                LoadScenes(initialAssets.SystemScenes);
                if (UnityApplication.IsUnityPlayer)
                {
                    CreateObjects(initialAssets.PlayPrefabs, parent);
                    LoadScenes(initialAssets.PlayScenes);
                }
            }

            if (!initializedAny)
            {
                Debug.LogWarning("No addressable initial assets were found in the assigned Unity application initial data, skipping.");
            }
        }

        void IListener<ApplicationStopped>.Receive(VirtualMachine vm, ref ApplicationStopped e)
        {
            DestroyObjects();
        }

        private void LoadScenes(IEnumerable<AssetReference> sceneAddresses)
        {
            foreach (AssetReference sceneAddress in sceneAddresses)
            {
                InvokeFunctionRequest req = new(new LoadScene(sceneAddress));
                vm.Broadcast(ref req);
            }
        }

        private void CreateObjects(IEnumerable<AssetReferenceGameObject> gameObjects, Transform parent)
        {
            foreach (AssetReferenceGameObject prefabAddress in gameObjects)
            {
                InvokeFunctionRequest req = new(new Instantiate(prefabAddress, parent, (instance) =>
                {
                    if (instance is GameObject gameObject)
                    {
                        instances.Add(gameObject);
                    }
                    else
                    {
                        throw new NotImplementedException($"Type {instance.GetType()} is not supported");
                    }
                }));
                vm.Broadcast(ref req);
            }
        }

        private void DestroyObjects()
        {
            foreach (GameObject instance in instances)
            {
                InvokeFunctionRequest req = new(new Destroy(instance));
                vm.Broadcast(ref req);
            }

            instances.Clear();
        }
    }
}
