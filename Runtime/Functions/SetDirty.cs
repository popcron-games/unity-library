#nullable enable
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Platformer.Functions
{
    public readonly struct SetDirty
    {
        public SetDirty(Object obj)
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(obj);
#endif
        }
    }
}