#nullable enable
using UnityEngine.InputSystem;

namespace Library.Unity
{
    public readonly struct InputDeviceRemoved
    {
        public readonly InputDevice device;

        public InputDeviceRemoved(InputDevice device)
        {
            this.device = device;
        }
    }
}