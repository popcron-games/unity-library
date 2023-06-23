using UnityEngine;

namespace Popcron
{
    public readonly struct RuntimeInitializeOnLoad : IEvent
    {
        public readonly RuntimeInitializeLoadType loadType;

        public RuntimeInitializeOnLoad(RuntimeInitializeLoadType loadType)
        {
            this.loadType = loadType;
        }
    }
}