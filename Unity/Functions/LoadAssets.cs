#nullable enable
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Platformer.Functions
{
    /// <summary>
    /// Loads all assets of a given type. 
    /// Editor only, in builds it will always return an empty list.
    /// </summary>
    public readonly struct LoadAssets<T> where T : Object
    {
        public static List<T> Invoke()
        {
#if UNITY_EDITOR
            List<T> result = new();
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).FullName}");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                {
                    result.Add(asset);
                }
            }

            return result;
#else
            return new List<T>();
#endif
        }
    }
}