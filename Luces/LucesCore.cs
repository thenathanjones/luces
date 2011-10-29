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

            InitialiseParser(configFile);

            RegisterForUpdates();
        }

        private void RegisterForUpdates()
        {
            foreach (var buildServer in _parser.BuildServers)
            {
                buildServer.PipelinesUpdated += HandlePipelineUpdate;
            }
        }

        private void HandlePipelineUpdate(IEnumerable<PipelineReport> update)
        {
            if (update.All(pr => pr.BuildState == BuildState.Success))
            {
                foreach (var light in Lights)
                {
                    light.Success();
                }
            }
        }

        private void InitialiseParser(string configFile)
        {
            _parser.Initialise(configFile);
        }

        private void InitialiseLights()
        {
            var devicePaths = _deviceManager.FindDevices(DeviceGuid.HID, "", ""); 
            Lights = devicePaths.Select(dp => _kernel.Get<ILight>());

            foreach (var light in Lights)
            {
                light.Unknown();
            }
        }
    }
}
