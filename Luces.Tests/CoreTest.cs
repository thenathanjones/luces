using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Ninject;
using NUSB.Manager;
using Luces;
using Luces.Lights;
using Burro;
using Burro.Util;
using Burro.BuildServers;
using Burro.Parsers;
using System.Collections;
using System.IO;

namespace Luces.Tests
{
    [TestFixture]
    public class CoreTest
    {
        private IKernel _kernel;
        private Mock<IDeviceManager> _deviceManager;
        private Mock<IBurroCore> _burro;
        private Mock<ILight> _light;

        private PipelineReport SUCCESSFUL_IDLE_PIPELINE = new PipelineReport() {BuildState = BuildState.Success, Activity = Activity.Idle};
        private PipelineReport FAILED_IDLE_PIPELINE = new PipelineReport() {BuildState = BuildState.Failure, Activity = Activity.Idle};
        private PipelineReport SUCCESSFUL_BUILDING_PIPELINE = new PipelineReport() {BuildState = BuildState.Success, Activity = Activity.Busy};
        private PipelineReport FAILED_BUILDING_PIPELINE = new PipelineReport() {BuildState = BuildState.Failure, Activity = Activity.Busy};

        [SetUp]
        public void Setup()
        {
            _kernel = new StandardKernel();
            _deviceManager = new Mock<IDeviceManager>();
            _kernel.Bind<IDeviceManager>().ToConstant(_deviceManager.Object);
            _burro = new Mock<IBurroCore>();
            _kernel.Bind<IBurroCore>().ToConstant(_burro.Object);
            _light = new Mock<ILight>();
            _kernel.Bind<ILight>().ToConstant(_light.Object);

            LightConstants.KnownLightTypes.Clear();
            LightConstants.KnownLightTypes.Add(new LightConfig() { Name = "I" });
        }

        [Test]
        public void InitialisationDetectsLights()
        {
            var core = _kernel.Get<LucesCore>();
            core.Initialise();
            _deviceManager.Verify(dm => dm.FindDevices(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void TurnsPathsIntoLights()
        {
            _deviceManager.Setup(dm => dm.FindDevices(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new List<string> {"blah", "blah1"});

            var core = _kernel.Get<LucesCore>();
            Assert.IsEmpty((ICollection)core.Lights);
            core.Initialise();
            Assert.AreEqual(2, core.Lights.Count());
            Assert.IsInstanceOf<ILight>(core.Lights.First());
        }
        
        [Test]
        public void CreatesConfigFileIfNotPresent()
        {
            var fileName = "./luces.yml";

            // make sure file doesn't exist
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            var core = _kernel.Get<LucesCore>();
            core.Initialise();
            
            // make sure the file is there
            Assert.IsTrue(File.Exists(fileName));
        }

        [Test]
        public void InitialisationStartsParserWithDefaultConfig()
        {
            var core = _kernel.Get<LucesCore>();
            core.Initialise();
            _burro.Verify(b => b.Initialise("./luces.yml"), Times.Once());
        }

        [Test]
        public void InitialisationAllowsConfigPassedIn()
        {
            var core = _kernel.Get<LucesCore>();
            core.Initialise("file2.yml");
            _burro.Verify(b => b.Initialise("file2.yml"), Times.Once());
        }

        [Test]
        public void InitialisesLightsToUnknown()
        {
            var core = _kernel.Get<LucesCore>();
            
            _deviceManager.Setup(dm => dm.FindDevices(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new List<string> { "blah", "blah1" });

            core.Initialise();

            _light.Verify(l => l.Unknown(), Times.Exactly(2));
        }

        [Test]
        public void InitialiseStartsMonitoring()
        {
            var core = _kernel.Get<LucesCore>();

            core.Initialise();

            _burro.Verify(b => b.StartMonitoring(), Times.Once());
        }

        [Test]
        public void ChangesToAllBuildPipelinesAlterLights()
        {
            var core = _kernel.Get<LucesCore>();

            _deviceManager.Setup(dm => dm.FindDevices(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new List<string> { "blah" });

            var bs1 = new Mock<IBuildServer>();
            var bs2 = new Mock<IBuildServer>();
            var buildServers = new List<Mock<IBuildServer>>() { bs1, bs2 };

            _burro.Setup(b => b.BuildServers).Returns(new List<IBuildServer>(buildServers.Select(bs => bs.Object)));

            core.Initialise();

            bs1.Raise(bs => bs.PipelinesUpdated += null, new List<PipelineReport>() { SUCCESSFUL_IDLE_PIPELINE });
            _light.Verify(l => l.Success(), Times.Once());
            bs2.Raise(bs => bs.PipelinesUpdated += null, new List<PipelineReport>() { SUCCESSFUL_IDLE_PIPELINE });
            _light.Verify(l => l.Success(), Times.Exactly(2));
        }

        [Test]
        public void SuccessfulBuilds()
        {
            var core = _kernel.Get<LucesCore>();

            _deviceManager.Setup(dm => dm.FindDevices(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new List<string> { "blah" });

            var bs1 = new Mock<IBuildServer>();
            var bs2 = new Mock<IBuildServer>();
            var buildServers = new List<Mock<IBuildServer>>() { bs1, bs2 };

            _burro.Setup(b => b.BuildServers).Returns(new List<IBuildServer>(buildServers.Select(bs => bs.Object)));

            core.Initialise();

            bs1.Raise(bs => bs.PipelinesUpdated += null, new List<PipelineReport>() { SUCCESSFUL_BUILDING_PIPELINE });
            _light.Verify(l => l.Success(), Times.Never());
            _light.Verify(l => l.Building(), Times.Once());
            bs1.Raise(bs => bs.PipelinesUpdated += null, new List<PipelineReport>() { SUCCESSFUL_IDLE_PIPELINE });
            _light.Verify(l => l.Building(), Times.Once());
            _light.Verify(l => l.Success(), Times.Once());
        }

        [Test]
        public void FailedBuilds()
        {
            var core = _kernel.Get<LucesCore>();

            _deviceManager.Setup(dm => dm.FindDevices(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new List<string> { "blah" });

            var bs1 = new Mock<IBuildServer>();
            var bs2 = new Mock<IBuildServer>();
            var buildServers = new List<Mock<IBuildServer>>() { bs1, bs2 };

            _burro.Setup(b => b.BuildServers).Returns(new List<IBuildServer>(buildServers.Select(bs => bs.Object)));

            core.Initialise();

            bs1.Raise(bs => bs.PipelinesUpdated += null, new List<PipelineReport>() { FAILED_IDLE_PIPELINE });
            _light.Verify(l => l.Success(), Times.Never());
            _light.Verify(l => l.Building(), Times.Never());
            _light.Verify(l => l.Failure(), Times.Once());
            bs1.Raise(bs => bs.PipelinesUpdated += null, new List<PipelineReport>() { FAILED_BUILDING_PIPELINE });
            _light.Verify(l => l.Building(), Times.Never());
            _light.Verify(l => l.Success(), Times.Never());
            _light.Verify(l => l.Failure(), Times.Exactly(2));
        }

        [Test]
        public void LoadsLightsFromConfig()
        {
            LightConstants.KnownLightTypes.Clear();
            LightConstants.KnownLightTypes.Add(new LightConfig() { Name = "Test" });
            _deviceManager.Setup(dm => dm.FindDevices(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new List<string> { "blah" });

            var core = _kernel.Get<LucesCore>();
            core.Initialise();

            Assert.AreEqual(1, core.Lights.Count());
            Assert.IsInstanceOf<TestLight>(core.Lights.First());
        }

        [Test]
        public void ShutdownDisablesAndDisconnectsLights()
        {
            _deviceManager.Setup(dm => dm.FindDevices(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new List<string> { "blah" });

            var core = _kernel.Get<LucesCore>();
            core.Initialise();

            _light.Verify(l => l.Off(), Times.Never());
            _light.Verify(l => l.Disconnect(), Times.Never());
            core.Shutdown();
            _light.Verify(l => l.Off(), Times.Once());
            _light.Verify(l => l.Disconnect(), Times.Once());
        }
    }
}
