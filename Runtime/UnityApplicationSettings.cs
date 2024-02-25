#nullable enable
using Game;
using Game.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace UnityLibrary
{
    /// <summary>
    /// Singleton asset accessible from anywhere, containing user assets.
    /// </summary>
    /// <remarks>It's base type is not meant to be a <see cref="CustomScriptableObject"/>, as this would cause a circular reference problem.
    /// during the <see cref="OnEnable"/> event.
    /// <para></para>
    /// Despite it being a <see cref="ScriptableObject"/> and thus not having the ability to receive events by default, it implements
    /// <see cref="IListener{TestEvent}"/> anyway as its injected into the tested manually to avoid the circular reference issue.
    /// </remarks>
    [DefaultExecutionOrder(int.MinValue + 20)]
    public sealed class UnityApplicationSettings : ScriptableObject, VirtualMachine.IInitialData, IListener<TestEvent>
    {
        private static UnityApplicationSettings? singleton;

        /// <summary>
        /// Static reference to the singleton asset.
        /// </summary>
        public static UnityApplicationSettings Singleton
        {
            get
            {
#if UNITY_EDITOR
                if (singleton == null)
                {
                    singleton = CreateInstance();
                }
#endif
                if (singleton == null)
                {
                    throw new Exception("Program is executing in a state where the unity application settings aren't available");
                }

                return singleton;
            }
            private set => singleton = value;
        }

        [SerializeField]
        private string? stateTypeName;

        [SerializeField]
        private InitialAssets initialData;

        /// <summary>
        /// The <see cref="VirtualMachine.IState"/> type to use when <see cref="UnityApplication"/> creates its virtual machine.
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
        }

        public InitialAssets? InitialData => initialData != null ? initialData : null;

        void IListener<TestEvent>.Receive(VirtualMachine vm, ref TestEvent e)
        {
            Assert.IsNotNull(StateType, "StateType is null, please set it in the unity application settings asset.");
            Assert.IsNotNull(initialData, "InitialData is null, please set it in the unity application settings asset.");
        }

        private void OnEnable()
        {
            Singleton = this;
        }

        /// <summary>
        /// Iterates over all assets that are assignable to <typeparamref name="T"/>.
        /// </summary>
        public IReadOnlyList<object> GetAllThatAre<T>()
        {
            return initialData.GetAllThatAre<T>();
        }

        /// <summary>
        /// Adds the asset if not contained already.
        /// </summary>
        public bool TryAdd(Object asset)
        {
            return initialData.Add(asset);
        }

        /// <summary>
        /// Removes a specific asset.
        /// </summary>
        public bool TryRemove(Object asset)
        {
            return initialData.Remove(asset);
        }

        public bool AssignStateType(Type type)
        {
            if (stateTypeName != type.AssemblyQualifiedName)
            {
                stateTypeName = type.AssemblyQualifiedName;
                return true;
            }
            else return false;
        }

        public bool AssignInitialData(InitialAssets assets)
        {
            if (initialData != assets)
            {
                initialData = assets;
                return true;
            }
            else return false;
        }

#if UNITY_EDITOR
        private static UnityApplicationSettings CreateInstance()
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets($"t:{nameof(UnityApplicationSettings)}");
            UnityApplicationSettings? found = null;
            foreach (string guid in guids)
            {
                string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                UnityApplicationSettings asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityApplicationSettings>(assetPath);
                if (found == null)
                {
                    found = asset;
                }
                else
                {
                    Debug.LogWarningFormat(asset, "Duplicate unity application settings asset found at {0}, choosing first found {1}", UnityEditor.AssetDatabase.GetAssetPath(asset), found);
                    break;
                }
            }

            List<Object> preloadedAssets = UnityEditor.PlayerSettings.GetPreloadedAssets().ToList();
            foreach (Object obj in preloadedAssets)
            {
                found = obj as UnityApplicationSettings;
                if (found != null)
                {
                    break;
                }
            }

            if (found == null)
            {
                found = CreateInstance<UnityApplicationSettings>();
                found.stateTypeName = "Library.Unity.UnityApplication+DefaultState, Library.Runtime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
                string path = "Assets/Settings.asset";
                UnityEditor.AssetDatabase.CreateAsset(found, path);
                UnityEditor.AssetDatabase.SaveAssets();
                Debug.LogFormat("Created unity application settings asset at {1}", path);
            }

            if (!preloadedAssets.Contains(found))
            {
                preloadedAssets.Add(found);
                UnityEditor.PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
            }

            return found;
        }
#endif
    }
}
