using System;

namespace UnityEngine
{
    [Flags]
    public enum MonoBehaviourFlags
    {
        None = 0,
        InOnValidate = 1 << 0,
        InReset = 1 << 1,
    }
}