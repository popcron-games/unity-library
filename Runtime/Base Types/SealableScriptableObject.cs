using UnityEngine;

namespace Popcron.Sealable
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
