using UnityEngine;

namespace Popcron.Sealable
{
    public abstract class SealableMonoBehaviour : MonoBehaviour
    {
        protected virtual void Awake() { }
        protected virtual void Start() { }
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
        protected virtual void OnValidate() { }
        protected virtual void Reset() { }
        protected virtual void OnDestroy() { }
    }
}