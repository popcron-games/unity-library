#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;
using UnityLibrary.Events;
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
    public sealed class UnityApplicationSettings : ScriptableObject, IListener<Validate>
    {
        private const string PathKey = nameof(UnityApplicationSettings) + ".path";
        internal const string EditorSystemsTypeNameKey = nameof(editorSystemsTypeName);
        internal const string ProgramTypeNameKey = nameof(programTypeName);
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
        private string? programTypeName;

        [SerializeField]
        private string? editorSystemsTypeName;

        [SerializeField]
        private bool addBuiltInSystems;

        /// <summary>
        /// The <see cref="IProgram"/> type to use when <see cref="UnityApplication"/> creates its virtual machine.
        /// </summary>
        public Type? ProgramType
        {
            get
            {
                if (string.IsNullOrEmpty(programTypeName))
                {
                    return null;
                }

                return Type.GetType(programTypeName);
            }
        }

        /// <summary>
        /// The editor only seems type.
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
        /// Controls whether the <see cref="UnityLibrarySystems"/> should be added by default.
        /// </summary>
        public bool AddBuiltInSystems => addBuiltInSystems;

        void IListener<Validate>.Receive(VirtualMachine vm, ref Validate e)
        {
            Assert.IsNotNull(ProgramType, $"Program type is null, please set it in the singleton {nameof(UnityApplicationSettings)} asset");
        }

        private void OnEnable()
        {
            singleton = this;
            if (!UnityApplication.started)
            {
                UnityApplication.started = true;
                UnityApplication.Start(this);
            }
        }

        private void OnDisable()
        {
            if (UnityApplication.started)
            {
                UnityApplication.started = false;
                UnityApplication.Stop();
            }

#if UNITY_EDITOR
            EditorPrefs.SetString(PathKey, AssetDatabase.GetAssetPath(this));
#endif
            singleton = null;
        }

        public bool TryAssignProgramType(Type newProgramType)
        {
            if (programTypeName != newProgramType.AssemblyQualifiedName)
            {
                programTypeName = newProgramType.AssemblyQualifiedName;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryAssignEditorSystemsType(Type newEditorSystemsType)
        {
            if (editorSystemsTypeName != newEditorSystemsType.AssemblyQualifiedName)
            {
                editorSystemsTypeName = newEditorSystemsType.AssemblyQualifiedName;
                return true;
            }
            else
            {
                return false;
            }
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
        internal static UnityApplicationSettings? FindSingleton()
        {
            if (EditorPrefs.HasKey(PathKey))
            {
                string path = EditorPrefs.GetString(PathKey);
                EditorPrefs.DeleteKey(PathKey);
                UnityApplicationSettings existingSettings = AssetDatabase.LoadAssetAtPath<UnityApplicationSettings>(path);
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
            bool trimmed = false;
            for (int i = preloadedAssets.Count - 1; i >= 0; i--)
            {
                Object obj = preloadedAssets[i];
                if (obj == null)
                {
                    preloadedAssets.RemoveAt(i);
                    trimmed = true;
                }
            }

            if (trimmed)
            {
                PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
            }

            foreach (Object obj in preloadedAssets)
            {
                if (obj is UnityApplicationSettings existingSettings)
                {
                    return existingSettings;
                }
            }

            // scan the project, and add it to preloaded assets
            GUID[] guids = AssetDatabase.FindAssetGUIDs($"t:{typeof(UnityApplicationSettings).Name}");
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
#endif
    }
}
