using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUSB.Interop;
using NUSB.Manager;
using Burro;
using Ninject;
using Burro.Parsers;
using Luces.Lights;
using NUSB.Controller;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;

namespace Luces
{
    public class LucesCore
    {
        private IKernel _kernel;

        private IDeviceManager _deviceManager;

        private IBurroCore _parser;

        private IList<ILight> _lights = new List<ILight>();

        public IEnumerable<ILight> Lights
        {
            get
            {
                return _lights;
            }
        }

        public LucesCore(IKernel kernel, IDeviceManager deviceManager, IBurroCore parser)
        {
            _kernel = kernel;
            _deviceManager = deviceManager;
            _parser = parser;
        }

        public void Initialise()
        {
            var assembly = System.Reflection.Assembly.GetEntryAssembly();
            var baseDir = ".";

            if (assembly != null)
            {
                baseDir = System.IO.Path.GetDirectoryName(assembly.Location);
            }
            var configPath = baseDir + "/luces.yml";

            EnsureConfigExists(configPath);

            Initialise(configPath);
        }

        private void EnsureConfigExists(string configPath)
        {
            if (!File.Exists(configPath))
            {
                var resourceAssembly = Assembly.GetExecutingAssembly();
                var defaultConfig = resourceAssembly.GetManifestResourceStream("Luces.Config.luces.yml");
                WriteStreamToFile(defaultConfig, configPath);

                GiveWriteAccessToUsers(configPath);

                throw new FileLoadException("No config file found.  Put default at " + configPath);
            }
        }

        private void GiveWriteAccessToUsers(string configPath)
        {
            var fileSecurity = File.GetAccessControl(configPath);
            fileSecurity.AddAccessRule(new FileSystemAccessRule("BUILTIN\\Users", FileSystemRights.Write, AccessControlType.Allow));
            File.SetAccessControl(configPath, fileSecurity);
        }

        private void WriteStreamToFile(Stream stream, string fileName)
        {
            var outputFile = new FileStream(fileName, FileMode.Create);

            try
            {
                var length = 256;
                var buffer = new Byte[length];

                var bytesRead = stream.Read(buffer, 0, length);
                while (bytesRead > 0)
                {
                    outputFile.Write(buffer, 0, bytesRead);
                    bytesRead = stream.Read(buffer, 0, length);
                }
            }
            finally
            {
                stream.Close();
                outputFile.Close();
            }
        }

        public void Initialise(string configFile)
        {
            InitialiseLights();

            InitialiseParser(configFile);

            RegisterForUpdates();
        }

        private void InitialiseParser(string configFile)
        {
            _parser.Initialise(configFile);
        }

        private void InitialiseLights()
        {
            foreach (var config in LightConstants.KnownLightTypes)
            {
                LoadLights(config);
            }

            SetInitialState();
        }

        private void LoadLights(LightConfig config)
        {
            var deviceClassName = "Luces.Lights." + config.Name + "Light";
            var deviceClassType = AppDomain.CurrentDomain.GetAssemblies().AsEnumerable().SelectMany(a => a.GetTypes()).Single(t => t.FullName == deviceClassName);

            var devicePaths = _deviceManager.FindDevices(config.Guid, config.VendorId, config.ProductId);
            foreach (var devicePath in devicePaths)
            {
                var light = (ILight)_kernel.Get(deviceClassType);
                light.Initialise(devicePath);
                _lights.Add(light);
            }
        }

        private void SetInitialState()
        {
            foreach (var light in Lights)
            {
                light.Unknown();
            }
        }

        private void RegisterForUpdates()
        {
            foreach (var buildServer in _parser.BuildServers)
            {
                buildServer.PipelinesUpdated += HandlePipelineUpdate;
            }

            _parser.StartMonitoring();
        }

        private void HandlePipelineUpdate(IEnumerable<PipelineReport> update)
        {
            if (update.All(pr => pr.BuildState == BuildState.Success))
            {
                BuildsSuccessful(update);
            }
            else
            {
                foreach (var light in Lights)
                {
                    light.Failure();
                }
            }
        }

        private void BuildsSuccessful(IEnumerable<PipelineReport> update)
        {
            if (update.Any(pr => pr.Activity == Activity.Busy))
            {
                foreach (var light in Lights)
                {
                    light.Building();
                }
            }
            else
            {
                foreach (var light in Lights)
                {
                    light.Success();
                }
            }
        }

        public void Shutdown()
        {
            foreach (var light in Lights)
            {
                light.Off();
                light.Disconnect();
            }
        }
    }
}
