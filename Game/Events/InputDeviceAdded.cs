#nullable enable
namespace UnityLibrary.Unity
{
    public readonly struct InputDeviceAdded
    {
        public readonly int deviceId;

        public InputDeviceAdded(int deviceId)
        {
            this.deviceId = deviceId;
        }
    }
}