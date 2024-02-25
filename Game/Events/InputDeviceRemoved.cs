#nullable enable
namespace UnityLibrary.Unity
{
    public readonly struct InputDeviceRemoved
    {
        public readonly int deviceId;

        public InputDeviceRemoved(int deviceId)
        {
            this.deviceId = deviceId;
        }
    }
}