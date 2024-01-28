#nullable enable
using System;
using UnityEngine.InputSystem;

namespace Library.Unity
{
    public readonly struct InputControlPressed
    {
        public readonly InputControl control;

        public InputControlPressed(InputControl control)
        {
            this.control = control;
        }

        public readonly bool IsPath(ReadOnlySpan<char> controlPath)
        {
            return controlPath.Equals(control.path.AsSpan(), StringComparison.OrdinalIgnoreCase);
        }
    }
}