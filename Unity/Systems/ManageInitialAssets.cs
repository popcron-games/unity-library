#nullable enable
using Library.Events;
using Library.Functions;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Library.Unity
{
    /// <summary>
    /// Loads and releases addressable assets assigned on the initial assets.
    /// </summary>
    public class ManageInitialAssets : IDisposable, IListener<ApplicationStarted>, IListener<ApplicationStopped>
    {
        private readonly HashSet<GameObject> instances = new();
        private readonly CancellationTokenSource cts = new();

        public ManageInitialAssets()
        {
            Addressables.InitializeAsync();
        }

        public void Dispose()
        {
            cts.Cancel();
            cts.Dispose();
        }

        void IListener<ApplicationStarted>.Receive(VirtualMachine vm, ApplicationStarted e)
        {
            HostSettings settings = HostSettings.Singleton;
            Transform parent = new GameObject(nameof(InitialAssets)).transform;
            GameObject.DontDestroyOnLoad(parent.gameObject);
            foreach (InitialAssets initialAssets in settings.GetAllThatAre<InitialAssets>())
            {
                CreateObjects(initialAssets.SystemPrefabs, parent);
                LoadScenes(initialAssets.SystemScenes);
                if (Host.IsUnityPlayer)
                {
                    CreateObjects(initialAssets.PlayPrefabs, parent);
                    LoadScenes(initialAssets.PlayScenes);
                }
            }
        }

        void IListener<ApplicationStopped>.Receive(VirtualMachine vm, ApplicationStopped e)
        {
            ReleaseObjects();
        }

        private void LoadScenes(IEnumerable<AssetReference> sceneAddresses)
        {
            foreach (AssetReference sceneAddress in sceneAddresses)
            {
                new LoadScene(sceneAddress.RuntimeKey.ToString());
            }
        }

        private void CreateObjects(IEnumerable<AssetReferenceGameObject> gameObjects, Transform parent)
        {
            foreach (AssetReferenceGameObject prefabAddress in gameObjects)
            {
                new Instantiate(prefabAddress.RuntimeKey.ToString(), parent, (instance) =>
                {
                    instances.Add(instance);
                });
            }
        }

        private void ReleaseObjects()
        {
            foreach (GameObject instance in instances)
            {
                new Destroy(instance);
            }

            instances.Clear();
        }
    }
}
