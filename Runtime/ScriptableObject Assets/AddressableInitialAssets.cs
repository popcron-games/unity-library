#nullable enable
using Game;
using Game.Events;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;

namespace UnityLibrary
{
    [CreateAssetMenu(menuName = "Unity Library/Addressable Initial Assets")]
    public sealed class AddressableInitialAssets : CustomScriptableObject, IListener<TestEvent>
    {
        [Header("Created always")]
        [SerializeField]
        private List<AssetReferenceGameObject> systemPrefabs = new();

        [SerializeField]
        private List<AssetReference> systemScenes = new();

        [Header("Created on play")]
        [SerializeField]
        private List<AssetReferenceGameObject> playPrefabs = new();

        [SerializeField]
        private List<AssetReference> playScenes = new();

        /// <summary>
        /// Prefabs created when build initializes or when playing from current scene, or the custom play button.
        /// </summary>
        public IReadOnlyCollection<AssetReferenceGameObject> SystemPrefabs => systemPrefabs;

        /// <summary>
        /// Scenes loaded when build initializes or when playing from current scene, or the custom play button.
        /// </summary>
        public IReadOnlyCollection<AssetReference> SystemScenes => systemScenes;

        /// <summary>
        /// Prefabs created when build initializes, or when custom play button is used. Not loaded when playing from current scene.
        /// </summary>
        public IReadOnlyCollection<AssetReferenceGameObject> PlayPrefabs => playPrefabs;

        /// <summary>
        /// Scenes loaded when build initializes or when custom play button is used. Not loaded when playing from current scene.
        /// </summary>
        public IReadOnlyCollection<AssetReference> PlayScenes => playScenes;

        private void Reset()
        {
            UnityApplicationSettings.Singleton.TryAdd(this);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(UnityApplicationSettings.Singleton);
#endif
        }

        public bool TryAddSystemPrefab(string address)
        {
            AssetReferenceGameObject asset = new(address);
            if (!systemPrefabs.Contains(asset))
            {
                systemPrefabs.Add(asset);
                return true;
            }

            return false;
        }

        public bool TryRemoveSystemPrefab(string address)
        {
            AssetReferenceGameObject asset = new(address);
            if (systemPrefabs.Contains(asset))
            {
                systemPrefabs.Remove(asset);
                return true;
            }

            return false;
        }

        void IListener<TestEvent>.Receive(VirtualMachine vm, ref TestEvent ev)
        {
#if UNITY_EDITOR
            for (int i = 0; i < systemPrefabs.Count; i++)
            {
                AssetReferenceGameObject systemPrefab = systemPrefabs[i];
                Assert.IsNotNull(systemPrefab.editorAsset, $"Prefab {systemPrefab} is not assigned at index {i} on {this}");
                GameObject editorAsset = systemPrefab.editorAsset;
                foreach (IListener<TestEvent> component in editorAsset.GetComponentsInChildren<IListener<TestEvent>>())
                {
                    component.Receive(vm, ref ev);
                }
            }

            for (int i = 0; i < systemScenes.Count; i++)
            {
                AssetReference scene = systemScenes[i];
                Assert.IsNotNull(scene.editorAsset, $"Scene {scene} is not assigned");
                Assert.IsTrue(scene.editorAsset.GetType().Name.Contains("SceneAsset"), $"Scene address {scene} is not a scene");
            }

            for (int i = 0; i < playPrefabs.Count; i++)
            {
                AssetReferenceGameObject playPrefab = playPrefabs[i];
                Assert.IsNotNull(playPrefab.editorAsset, $"Prefab {playPrefab} is not assigned at index {i} on {this}");
                GameObject editorAsset = playPrefab.editorAsset;
                foreach (IListener<TestEvent> component in editorAsset.GetComponentsInChildren<IListener<TestEvent>>())
                {
                    component.Receive(vm, ref ev);
                }
            }

            for (int i = 0; i < playScenes.Count; i++)
            {
                AssetReference scene = playScenes[i];
                Assert.IsNotNull(scene.editorAsset, $"Scene {scene} is not assigned");
                Assert.IsTrue(scene.editorAsset.GetType().Name.Contains("SceneAsset"), $"Scene address {scene} is not a scene");
            }
#endif
        }
    }
}
