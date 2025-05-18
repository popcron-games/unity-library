#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
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
        public const string ProgramTypeName = nameof(programTypeName);
        public const string EditorSystemsTypeName = nameof(editorSystemsTypeName);
        private const string PathKey = nameof(UnityApplicationSettings) + ".path";

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
                    singleton = GetOrCreateInstance();
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

        void IListener<Validate>.Receive(VirtualMachine vm, ref Validate e)
        {
            Assert.IsNotNull(ProgramType, $"Program type is null, please set it in the unity application settings asset");
        }

        private void OnEnable()
        {
            if (singleton == null)
            {
                singleton = this;
                if (!UnityApplication.started)
                {
                    UnityApplication.started = true;
                    UnityApplication.Start(this);
                }
            }
        }

        private void OnDisable()
        {
            if (singleton == this)
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
        }

        public bool TryAssignProgramType(Type newProgramType)
        {
            if (programTypeName != newProgramType.AssemblyQualifiedName)
            {
                programTypeName = newProgramType.AssemblyQualifiedName;
                return true;
            }
            else return false;
        }

        public bool TryAssignEditorSystemsType(Type newEditorSystemsType)
        {
            if (editorSystemsTypeName != newEditorSystemsType.AssemblyQualifiedName)
            {
                editorSystemsTypeName = newEditorSystemsType.AssemblyQualifiedName;
                return true;
            }
            else return false;
        }

        [Conditional("DEBUG")]
        private static void ThrowIfSingletonIsMissing()
        {
            if (singleton == null)
            {
                throw new($"UnityApplicationSettings singleton is missing");
            }
        }

#if UNITY_EDITOR
        private static UnityApplicationSettings GetOrCreateInstance()
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
                    UnityEngine.Debug.LogError($"UnityApplicationSettings asset not found at {path}, creating a new one");
                }
            }

            List<Object> preloadedAssets = PlayerSettings.GetPreloadedAssets().ToList();
            foreach (Object obj in preloadedAssets)
            {
                if (obj != null && obj is UnityApplicationSettings existingSettings)
                {
                    return existingSettings;
                }
            }

            string[] guids = AssetDatabase.FindAssets($"t:{typeof(UnityApplicationSettings).Name}");
            if (guids.Length > 0)
            {
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    UnityApplicationSettings existingSettings = AssetDatabase.LoadAssetAtPath<UnityApplicationSettings>(path);
                    if (existingSettings != null)
                    {
                        preloadedAssets.Add(existingSettings);
                        PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
                        return existingSettings;
                    }
                }
            }

            UnityApplicationSettings newInstance = CreateInstance<UnityApplicationSettings>();
            AssetDatabase.CreateAsset(newInstance, "Assets/Settings.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log($"Created unity application settings asset", newInstance);
            preloadedAssets.Add(newInstance);
            PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
            return newInstance;
        }
#endif
    }
}
