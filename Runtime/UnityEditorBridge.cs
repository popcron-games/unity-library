#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using Object = UnityEngine.Object;
using UnityEngine;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Popcron
{
    /// <summary>
    /// Provides access to some of the AssetDatabase API at runtime without the need for compiler regions.
    /// </summary>
    public static class UnityEditorBridge
    {
        private static readonly StringBuilder builder = new StringBuilder();

#if UNITY_EDITOR
        public static Object selectedObject
        {
            get => Selection.activeObject;
            set => Selection.activeObject = value;
        }
#else
        public static Object selectedObject;
#endif

#if UNITY_EDITOR
        public static Object[] selectedObjects
        {
            get => Selection.objects;
            set => Selection.objects = value;
        }
#else
        public static Object[] selectedObjects = Array.Empty<Object>();
#endif

        /// <summary>
        /// Returns the first <typeparamref name="T"/> asset found.
        /// </summary>
        public static T? FindAsset<T>() where T : Object
        {
            foreach (T asset in FindAssets<T>())
            {
                return asset;
            }

            return null;
        }

        public static string[] FindAssets(string searchQuery)
        {
#if UNITY_EDITOR
            return AssetDatabase.FindAssets(searchQuery);
#else
            return Array.Empty<string>();
#endif
        }

        public static string[] FindAssets(string searchQuery, string[] searchInFolders)
        {
#if UNITY_EDITOR
            return AssetDatabase.FindAssets(searchQuery, searchInFolders);
#else
            return Array.Empty<string>();
#endif
        }

        public static string? GetAssetPath<T>(T asset) where T : Object
        {
#if UNITY_EDITOR
            return AssetDatabase.GetAssetPath(asset);
#else
            return null;
#endif
        }

        public static string AssetPathToGUID(string path)
        {
#if UNITY_EDITOR
            return AssetDatabase.AssetPathToGUID(path);
#else
            return "";
#endif
        }

        public static string GUIDToAssetPath(string guid)
        {
#if UNITY_EDITOR
            return AssetDatabase.GUIDToAssetPath(guid);
#else
            return "";
#endif
        }

        public static T? LoadAssetAtPath<T>(string path) where T : Object
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<T>(path);
#else
            return null;
#endif
        }

        public static Object? LoadAssetAtPath(string path, Type type)
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath(path, type);
#else
            return null;
#endif

        }

        public static Object? LoadAssetWithGUID(string guid, Type type)
        {
            return LoadAssetAtPath(GUIDToAssetPath(guid), type);
        }

        public static T? LoadAssetWithGUID<T>(string guid) where T : Object
        {
            return LoadAssetAtPath<T>(GUIDToAssetPath(guid));
        }

        public static void SetDirty(Object asset)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(asset);
#endif
        }

        public static string GenerateUniqueAssetPath(string path)
        {
#if UNITY_EDITOR
            return AssetDatabase.GenerateUniqueAssetPath(path);
#else
            return path;
#endif
        }

        public static void CreateAsset(Object asset, string path)
        {
#if UNITY_EDITOR
            AssetDatabase.CreateAsset(asset, path);
#endif
        }

        public static T CreateAssetInstance<T>(string path) where T : ScriptableObject
        {
#if UNITY_EDITOR
            T asset = ScriptableObject.CreateInstance<T>();
            CreateAsset(asset, path);
            return asset;
#else
            return ScriptableObject.CreateInstance<T>();
#endif
        }

        public static void SaveAssets()
        {
#if UNITY_EDITOR
            AssetDatabase.SaveAssets();
#endif
        }

        public static void CopySerialized(Object asset, Object existingAsset)
        {
#if UNITY_EDITOR
            EditorUtility.CopySerialized(asset, existingAsset);
#endif
        }

        public static void PingObject(Object asset)
        {
#if UNITY_EDITOR
            EditorGUIUtility.PingObject(asset);
#endif
        }

        public static void RepaintHierarchyWindow()
        {
#if UNITY_EDITOR
            EditorApplication.RepaintHierarchyWindow();
#endif
        }

        [Serializable]
        public class Container<T>
        {
            public T value;

            public Container(T value)
            {
                this.value = value;
            }
        }

        public static void SetEditorPref<T>(string key, T value)
        {
#if UNITY_EDITOR
            string json = JsonUtility.ToJson(new Container<T>(value));
            EditorPrefs.SetString(key, json);
#endif
        }

        public static T GetEditorPref<T>(string key, T defaultValue = default!)
        {
#if UNITY_EDITOR
            if (EditorPrefs.HasKey(key))
            {
                string json = EditorPrefs.GetString(key);
                try
                {
                    return JsonUtility.FromJson<Container<T>>(json).value!;
                }
                catch { }
            }
#endif
            return defaultValue;
        }

        public static bool TryGetEditorPref<T>(string key, out T value)
        {
#if UNITY_EDITOR
            if (EditorPrefs.HasKey(key))
            {
                string json = EditorPrefs.GetString(key);
                try
                {
                    Container<T> container = JsonUtility.FromJson<Container<T>>(json);
                    value = container.value!;
                    return true;
                }
                catch { }
            }
#endif
            value = default!;
            return false;
        }

        public static List<T> FindAssets<T>() where T : Object
        {
            builder.Clear();
            builder.Append("t:");
            builder.Append(typeof(T).FullName);
            string[] guids = FindAssets(builder.ToString());
            List<T> list = new List<T>(guids.Length);
            foreach (var guid in guids)
            {
                string path = GUIDToAssetPath(guid);
                if (LoadAssetAtPath<T>(path) is T asset)
                {
                    list.Add(asset);
                }
            }

            return list;
        }

        public static List<Object> FindAssets(Type type)
        {
            builder.Clear();
            builder.Append("t:");
            builder.Append(type.FullName);
            string[] guids = FindAssets(builder.ToString());
            List<Object> list = new List<Object>(guids.Length);
            foreach (string guid in guids)
            {
                string path = GUIDToAssetPath(guid);
                if (LoadAssetAtPath(path, type) is Object asset)
                {
                    list.Add(asset);
                }
            }

            return list;
        }

        public static Object? FindAssetWithID(ReadOnlySpan<char> id)
        {
            return FindAssetWithID<Object>(id);
        }

        public static T? FindAssetWithID<T>(ReadOnlySpan<char> id) where T : Object
        {
            builder.Clear();
            builder.Append("t:");
            builder.Append(typeof(T).FullName);
            string[] guids = FindAssets(builder.ToString());
            foreach (var guid in guids)
            {
                string path = GUIDToAssetPath(guid)!;
                Object asset = LoadAssetAtPath<T>(path)!;
                if (asset is IIdentifiable identifiable && identifiable.ID == id)
                {
                    return (T)asset;
                }
            }

            return null;
        }

        public static T GetCorrespondingObjectFromOriginalSource<T>(T component) where T : Object
        {
#if UNITY_EDITOR
            return PrefabUtility.GetCorrespondingObjectFromOriginalSource(component);
#else
            return component;
#endif
        }

        public static void CreateAsset(Object asset)
        {
            builder.Clear();
            builder.Append("Assets/");
            builder.Append(asset.name);
            builder.Append(".asset");
            CreateAsset(asset, GenerateUniqueAssetPath(builder.ToString()));
        }

        public static void CreateOrOverwriteAsset(Object asset, string assetPath)
        {
            Object? existingAsset = LoadAssetAtPath(assetPath, asset.GetType());
            if (existingAsset != null)
            {
                CopySerialized(asset, existingAsset);
                Object.DestroyImmediate(asset);
            }
            else
            {
                string directoryPath = Path.GetDirectoryName(assetPath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                CreateAsset(asset, assetPath);
            }
        }

        public static void RecordObject(Object objectToUndo, string name)
        {
#if UNITY_EDITOR
            Undo.RecordObject(objectToUndo, name);
#endif
        }

        public static string NicifyVariableName(string? v)
        {
#if UNITY_EDITOR
            return ObjectNames.NicifyVariableName(v) ?? string.Empty;
#else
            return v ?? string.Empty;
#endif
        }
    }
}