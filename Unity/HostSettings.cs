#nullable enable
using Library.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace Library.Unity
{
    /// <summary>
    /// Singleton asset accessible from anywhere, containing user assets.
    /// </summary>
    [DefaultExecutionOrder(int.MaxValue - 100)]
    public sealed class HostSettings : CustomScriptableObject, IListener<PlayValidationEvent>
    {
        private static HostSettings? singleton;

        /// <summary>
        /// Static reference to the singleton asset.
        /// </summary>
        public static HostSettings Singleton
        {
            get
            {
                if (singleton != null)
                {
                    return singleton;
                }

                return CreateInstance() ?? throw new Exception("HostSettings asset couldn't be retrieved");
            }
            private set => singleton = value;
        }

        [SerializeField]
        private string? stateTypeName;

        [SerializeField]
        private List<Object> assets = new();

        public IReadOnlyList<Object> Assets => assets;

        /// <summary>
        /// The <see cref="IState"/> type to use when <see cref="Host"/> creates its virtual machine.
        /// </summary>
        public Type? StateType
        {
            get
            {
                if (string.IsNullOrEmpty(stateTypeName))
                {
                    return null;
                }

                return Type.GetType(stateTypeName);
            }
            set
            {
                stateTypeName = value?.AssemblyQualifiedName;
            }
        }

        void IListener<PlayValidationEvent>.Receive(VirtualMachine vm, PlayValidationEvent e)
        {
            Assert.IsNotNull(StateType, "StateType is null, please set it in the HostSettings asset.");
            for (int i = 0; i < assets.Count; i++)
            {
                Object? obj = assets[i];
                Assert.IsNotNull(obj, $"Asset at index {i} is null in {this}");
                if (obj != null)
                {
                    obj.Tell(vm, e);
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Singleton = this;
        }

        protected override void OnDisable()
        {
            Singleton = null!;
            base.OnDisable();
        }

        /// <summary>
        /// Retrieves the first asset of type <typeparamref name="T"/>.
        /// </summary>
        public bool TryGetFirstAsset<T>([NotNullWhen(true)] out T? asset)
        {
            asset = default!;
            foreach (Object obj in assets)
            {
                if (obj is T t)
                {
                    asset = t;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Iterates over all assets that are assignable to <typeparamref name="T"/>.
        /// </summary>
        public IEnumerable<T> GetAllThatAre<T>()
        {
            foreach (Object obj in assets)
            {
                if (obj is T t)
                {
                    yield return t;
                }
            }
        }

        /// <summary>
        /// Adds the asset if not contained already.
        /// </summary>
        public bool TryAdd(Object asset)
        {
            if (!assets.Contains(asset))
            {
                assets.Add(asset);
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Removes a specific asset.
        /// </summary>
        public bool TryRemove(Object asset)
        {
            if (assets.Contains(asset))
            {
                assets.Remove(asset);
                return true;
            }
            else return false;
        }

        private static HostSettings? CreateInstance()
        {
#if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:" + nameof(HostSettings));
            HostSettings? found = null;
            foreach (string guid in guids)
            {
                HostSettings asset = UnityEditor.AssetDatabase.LoadAssetAtPath<HostSettings>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
                if (found == null)
                {
                    found = asset;
                }
                else
                {
                    Debug.LogError("Duplicate HostSettings asset found at " + UnityEditor.AssetDatabase.GetAssetPath(asset));
                    break;
                }
            }

            List<Object> preloadedAssets = UnityEditor.PlayerSettings.GetPreloadedAssets().ToList();
            foreach (Object obj in preloadedAssets)
            {
                found = obj as HostSettings;
                if (found != null)
                {
                    break;
                }
            }

            if (found == null)
            {
                found = CreateInstance<HostSettings>();
                UnityEditor.AssetDatabase.CreateAsset(found, "Assets/" + nameof(HostSettings) + ".asset");
                UnityEditor.AssetDatabase.SaveAssets();
                Debug.Log("Created HostSettings asset at Assets/" + nameof(HostSettings) + ".asset");
            }

            if (!preloadedAssets.Contains(found))
            {
                preloadedAssets.Add(found);
                UnityEditor.PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
            }

            return found;
#else
            return null;
#endif
        }
    }
}
