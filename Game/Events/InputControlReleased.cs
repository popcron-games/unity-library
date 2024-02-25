#nullable enable
using System;

namespace UnityLibrary.Unity
{
    public readonly struct InputControlReleased
    {
        public readonly int deviceId;
        private readonly int hash;

        public InputControlReleased(int deviceId, ReadOnlySpan<char> controlPath)
        {
            this.deviceId = deviceId;
            hash = controlPath.GetDjb2HashCode();
        }

        public readonly bool IsPath(ReadOnlySpan<char> controlPath)
        {
            return hash == controlPath.GetDjb2HashCode();
        }
    }
}