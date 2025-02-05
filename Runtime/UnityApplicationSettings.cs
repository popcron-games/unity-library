#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;
using UnityLibrary.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
    public sealed class UnityApplicationSettings : ScriptableObject, IInitialData, IListener<Validate>
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
        }

        [SerializeField]
        private string? stateTypeName;

        [SerializeField]
        private InitialAssets? initialData;

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

        void IListener<Validate>.Receive(VirtualMachine vm, ref Validate e)
        {
            Assert.IsNotNull(StateType, "StateType is null, please set it in the unity application settings asset");
        }

        private void OnEnable()
        {
            singleton = this;
#if UNITY_EDITOR
            EditorPrefs.SetString(nameof(UnityApplicationSettings) + ".path", AssetDatabase.GetAssetPath(this));
#endif
        }

        private void OnDisable()
        {
            singleton = null;
        }

        /// <summary>
        /// Iterates over all assets that are assignable to <typeparamref name="T"/>.
        /// </summary>
        public IReadOnlyList<T> GetAllThatAre<T>()
        {
            if (initialData is null)
            {
                return Array.Empty<T>();
            }

            return initialData.GetAllThatAre<T>();
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
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(UnityApplicationSettings)}");
            UnityApplicationSettings? found = null;
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                UnityApplicationSettings asset = AssetDatabase.LoadAssetAtPath<UnityApplicationSettings>(assetPath);
                if (found == null)
                {
                    found = asset;
                }
                else
                {
                    Debug.LogWarningFormat(asset, "Duplicate unity application settings asset found at {0}, choosing first found {1}", AssetDatabase.GetAssetPath(asset), found);
                    break;
                }
            }

            List<Object> preloadedAssets = PlayerSettings.GetPreloadedAssets().ToList();
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
                string path = "Assets/Settings.asset";
                AssetDatabase.CreateAsset(found, path);
                AssetDatabase.SaveAssets();
                Debug.LogFormat("Created unity application settings asset at {0}", path);
            }

            if (!preloadedAssets.Contains(found))
            {
                preloadedAssets.Add(found);
                PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
            }

            return found;
        }
#endif
    }
}
