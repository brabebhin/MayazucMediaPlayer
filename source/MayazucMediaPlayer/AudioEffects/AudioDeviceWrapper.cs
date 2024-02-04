using Windows.Devices.Enumeration;

namespace MayazucMediaPlayer.AudioEffects
{
    public sealed class AudioDeviceWrapper
    {

        public DeviceInformation Device
        {
            get;
            private set;
        }

        public AudioDeviceWrapper(DeviceInformation device)
        {
            Device = device;
        }


        public override string ToString()
        {
            if (Device == null)
                return "System default";
            return Device.Name;
        }
    }
}
