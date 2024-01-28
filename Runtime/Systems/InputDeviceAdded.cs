#nullable enable
using UnityEngine.InputSystem;

namespace Library.Unity
{
    public readonly struct InputDeviceAdded
    {
        public readonly InputDevice device;

        public InputDeviceAdded(InputDevice device)
        {
            this.device = device;
        }
    }
}