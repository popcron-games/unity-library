#nullable enable
using System;
using UnityEngine.InputSystem;

namespace Library.Unity
{
    public readonly struct InputControlChanged
    {
        public readonly InputControl control;

        public InputControlChanged(InputControl control)
        {
            this.control = control;
        }

        public readonly bool IsPath(ReadOnlySpan<char> controlPath)
        {
            return controlPath.Equals(control.path.AsSpan(), StringComparison.OrdinalIgnoreCase);
        }
    }
}