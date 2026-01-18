#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Diagnostics;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityLibrary
{
    /// <summary>
    /// Singleton asset accessible from anywhere, containing assets needed for the application to run.
    /// </summary>
    [ExecuteAlways, DefaultExecutionOrder(int.MaxValue)]
    public sealed class UnityApplicationSettings : ScriptableObject
    {
        private const string PathKey = nameof(UnityApplicationSettings) + ".path";
        internal const string EditorSystemsTypeNameKey = nameof(editorSystemsTypeName);
        internal const string RuntimeSystemsTypeNameKey = nameof(runtimeSystemsTypeName);
        internal const string AddBuiltInSystemsKey = nameof(addBuiltInSystems);

        internal static UnityApplicationSettings? singleton;

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
                    singleton = FindSingleton();
                    if (singleton == null)
                    {
                        throw new($"No {nameof(UnityApplicationSettings)} asset found in project, please create one");
                    }
                }
#endif
                ThrowIfSingletonIsMissing();
                return singleton!;
            }
        }

        [SerializeField]
        private string? runtimeSystemsTypeName;

        [SerializeField]
        private string? editorSystemsTypeName;

        [SerializeField]
        private bool addBuiltInSystems;

        /// <summary>
        /// Systems added to the virtual machine at runtime and in the editor.
        /// </summary>
        public Type? RuntimeSystemsType
        {
            get
            {
                if (string.IsNullOrEmpty(runtimeSystemsTypeName))
                {
                    return null;
                }

                return Type.GetType(runtimeSystemsTypeName);
            }
        }

        /// <summary>
        /// Systems added to the virtual machine only in the editor.
        /// </summary>
        public Type? EditorSystemsType
        {
            get
            {
                if (string.IsNullOrEmpty(editorSystemsTypeName))
                {
                    return null;
                }

                return Type.GetType(editorSystemsTypeName);
            }
        }

        /// <summary>
        /// Whether or not <see cref="UnityLibrarySystems"/> should be added automatically.
        /// </summary>
        public bool AddBuiltInSystems => addBuiltInSystems;

        private void OnEnable()
        {
            singleton = this;
            UnityApplication.TryStart(this);
        }

        private void OnDisable()
        {
            UnityApplication.TryStop();
#if UNITY_EDITOR
            EditorPrefs.SetString(PathKey, AssetDatabase.GetAssetPath(this));
#endif
            singleton = null;
        }

        [Conditional("DEBUG")]
        private static void ThrowIfSingletonIsMissing()
        {
            if (singleton == null)
            {
                throw new($"{nameof(UnityApplicationSettings)} singleton is missing");
            }
        }

#if UNITY_EDITOR
        public static UnityApplicationSettings? FindSingleton()
        {
            if (EditorPrefs.HasKey(PathKey))
            {
                string path = EditorPrefs.GetString(PathKey);
                EditorPrefs.DeleteKey(PathKey);
                UnityApplicationSettings? existingSettings = AssetDatabase.LoadAssetAtPath<UnityApplicationSettings>(path);
                if (existingSettings != null)
                {
                    return existingSettings;
                }
                else
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        UnityEngine.Debug.LogError($"Cached {nameof(UnityApplicationSettings)} asset not found at `{path}`, has it been deleted?");
                    }
                    else
                    {
                        UnityEngine.Debug.LogError($"Cached {nameof(UnityApplicationSettings)} asset not found, has it been deleted?");
                    }
                }
            }

            // trim null entries first, then find an existing singleton asset
            List<Object> preloadedAssets = new(PlayerSettings.GetPreloadedAssets());
            foreach (Object obj in preloadedAssets)
            {
                if (obj is UnityApplicationSettings existingSettings)
                {
                    return existingSettings;
                }
            }

            // scan the project, and add it to preloaded assets
            GUID[] guids = AssetDatabase.FindAssetGUIDs($"t:{nameof(UnityApplicationSettings)}");
            if (guids.Length > 0)
            {
                foreach (GUID guid in guids)
                {
                    UnityApplicationSettings existingSettings = AssetDatabase.LoadAssetByGUID<UnityApplicationSettings>(guid);
                    preloadedAssets.Add(existingSettings);
                    PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
                    return existingSettings;
                }
            }

            return null;
        }

        internal static UnityApplicationSettings CreateSingleton()
        {
            string currentPath = "Assets";
            if (Selection.activeObject is Object selectedObject)
            {
                currentPath = AssetDatabase.GetAssetPath(selectedObject);
                if (selectedObject is not DefaultAsset)
                {
                    int lastSlash = currentPath.LastIndexOf('/');
                    currentPath = currentPath[..lastSlash];
                }
            }

            currentPath += "/Unity Application Settings.asset";

            // create new instance
            UnityApplicationSettings newInstance = CreateInstance<UnityApplicationSettings>();
            newInstance.addBuiltInSystems = true;
            AssetDatabase.CreateAsset(newInstance, currentPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log($"Created unity application settings asset, please assign a program type", newInstance);
            EditorGUIUtility.PingObject(newInstance);

            // add to preloaded assets
            List<Object> preloadedAssets = new(PlayerSettings.GetPreloadedAssets());
            preloadedAssets.Add(newInstance);
            PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
            return newInstance;
        }
#else
        [Obsolete("Not available in builds", true)]
        public static UnityApplicationSettings? FindSingleton()
        {
            return null;
        }
#endif
    }
}
