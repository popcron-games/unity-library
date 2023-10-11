#nullable enable

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
#endif

namespace UnityEngine
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public static class PreloadedAssets
    {
        public static bool Contains<T>()
        {
            foreach (Object? asset in PlayerSettings.GetPreloadedAssets())
            {
                if (asset != null && asset is T)
                {
                    return true;
                }
            }

            return false;
        }

        public static void AddIfTypeNotPresent(Object asset)
        {
#if UNITY_EDITOR
            Type type = asset.GetType();
            List<Object?> preloadedAssets = new List<Object?>(PlayerSettings.GetPreloadedAssets());
            bool found = false;
            for (int i = 0; i < preloadedAssets.Count; i++)
            {
                Object? preloadedAsset = preloadedAssets[i];
                if (preloadedAsset != null && preloadedAsset.GetType() == type)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                preloadedAssets.Add(asset);
                PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
            }
#endif
        }
    }
}
