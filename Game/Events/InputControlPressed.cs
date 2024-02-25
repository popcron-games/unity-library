#nullable enable
using System;

namespace UnityLibrary.Unity
{
    public readonly struct InputControlPressed
    {
        public readonly int deviceId;
        public readonly int controlPathHash;

        public InputControlPressed(int deviceId, ReadOnlySpan<char> controlPath)
        {
            this.deviceId = deviceId;
            controlPathHash = controlPath.GetDjb2HashCode();
        }

        public readonly bool IsPath(ReadOnlySpan<char> controlPath)
        {
            return controlPathHash == controlPath.GetDjb2HashCode();
        }
    }
}