using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUSB.Interop;
using NUSB.Manager;
using Burro;
using Ninject;

namespace Luces
{
    public class LucesCore
    {
        private IKernel _kernel;

        private IDeviceManager _deviceManager;

        private IBurroCore _parser;

        public IEnumerable<ILight> Lights { get; private set; }

        public LucesCore(IKernel kernel, IDeviceManager deviceManager, IBurroCore parser)
        {
            _kernel = kernel;
            _deviceManager = deviceManager;
            _parser = parser;
        }

        public void Initialise()
        {
            Initialise("luces.yml");
        }

        public void Initialise(string configFile)
        {
            InitialiseLights();

            foreach (var light in Lights)
            {
                light.Unknown();
            }

            InitialiseParser(configFile);
        }

        private void InitialiseParser(string configFile)
        {
            _parser.Initialise(configFile);
        }

        private void InitialiseLights()
        {
            var devicePaths = _deviceManager.FindDevices(DeviceGuid.HID, "0FC5", "B080"); 
            Lights = devicePaths.Select(dp => _kernel.Get<ILight>());
        }
    }
}
