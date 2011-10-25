using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUSB.Interop;
using NUSB.Manager;

namespace Luces
{
    public class LucesCore
    {
        private IDeviceManager _deviceManager;

        public IEnumerable<ILight> Lights { get; set; }

        public LucesCore(IDeviceManager deviceManager)
        {
            _deviceManager = deviceManager;
        }

        public void Initialise()
        {
            var devicePaths = _deviceManager.FindDevices(DeviceGuid.HID, "0FC5", "B080");
        }
    }
}
