#nullable enable
using UnityEngine;

namespace Popcron
{
    /// <summary>
    /// Will occur automatically when OnValidate/OnReset get called on a <see cref="CustomMonoBehaviour"/> or <see cref="CustomScriptableObject"/> instances.
    /// </summary>
    public readonly struct ValidationEvent : IEvent
    {
        public readonly MonoBehaviourFlags flags;

        public ValidationEvent(MonoBehaviourFlags flags)
        {
            this.flags = flags;
        }
    }
}