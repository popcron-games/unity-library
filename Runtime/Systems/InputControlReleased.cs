#nullable enable
using System;
using UnityEngine.InputSystem;

namespace Library.Unity
{
    public readonly struct InputControlReleased
    {
        public readonly InputControl control;

        public InputControlReleased(InputControl control)
        {
            this.control = control;
        }

        public readonly bool IsPath(ReadOnlySpan<char> controlPath)
        {
            return controlPath.Equals(control.path.AsSpan(), StringComparison.OrdinalIgnoreCase);
        }
    }
}