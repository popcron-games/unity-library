#nullable enable
using UnityEngine;

namespace Popcron.Incomplete
{
    public abstract class SealableScriptableObject : ScriptableObject
    {
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
        protected virtual void OnValidate() { }
        protected virtual void OnDestroy() { }
        protected virtual void Awake() { }
        protected virtual void Reset() { }
    }
}
