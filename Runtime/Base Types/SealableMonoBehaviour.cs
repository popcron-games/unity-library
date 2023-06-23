#nullable enable
using System;
using UnityEngine;

namespace Popcron
{
    public abstract class SealableMonoBehaviour : MonoBehaviour, IBranch
    {
        protected virtual void Awake() { }
        protected virtual void Start() { }
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
        protected virtual void OnValidate() { }
        protected virtual void Reset() { }
        protected virtual void OnDestroy() { }

        bool IBranch.TryGetChild(ReadOnlySpan<char> id, out object? value)
        {
            int childrenCount = transform.childCount;
            int idHash = id.GetSpanHashCode();
            for (int i = 0; i < childrenCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.name.GetSpanHashCode() == idHash)
                {
                    value = child;
                    return true;
                }
            }

            value = null;
            return false;
        }
    }
}