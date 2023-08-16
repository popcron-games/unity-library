#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Popcron.Lib
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public static class ManageStaticObjects
    {
        private readonly static HashSet<Type> types = new();

        static ManageStaticObjects()
        {
            Call();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += (change) =>
            {
                if (change != UnityEditor.PlayModeStateChange.EnteredPlayMode)
                {
                    Call();
                }
            };
#endif
        }

        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeInitialize()
        {
            Call();
        }

        public static void Call()
        {
#if UNITY_EDITOR
            StaticData asset = ScriptableObject.CreateInstance<StaticData>();
            List<string> fullTypeNames = new();
            foreach (Type type in UnityEditor.TypeCache.GetTypesDerivedFrom<StaticObject>())
            {
                fullTypeNames.Add(type.AssemblyQualifiedName);
            }

            asset.staticObjectTypes = fullTypeNames;
            UnityEditorBridge.CreateOrOverwriteAsset(asset, "Assets/Resources/Static Data.asset");
            UnityEditor.AssetDatabase.Refresh();
#endif
            TryToCreate();
        }

        private static void TryToCreate()
        {
            StaticData staticData = Resources.Load<StaticData>("Static Data");
            foreach (var fullTypeName in staticData.staticObjectTypes)
            {
                Type? type = Type.GetType(fullTypeName);
                if (type != null && types.Add(type))
                {
                    StaticObject instance = (StaticObject)Activator.CreateInstance(type);
                    Everything.Add(instance);
                }
            }
        }
    }
}