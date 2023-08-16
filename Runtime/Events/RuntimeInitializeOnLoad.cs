using Popcron;

namespace UnityEngine
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