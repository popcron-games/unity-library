#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace UnityLibrary
{
    public static class SingletonScriptableObjects
    {
        internal static readonly List<ScriptableObject> scriptableObjects = new();

        public static IReadOnlyList<ScriptableObject> All => scriptableObjects;
    }
}