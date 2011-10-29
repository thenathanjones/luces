using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Ninject;
using NUSB.Manager;
using Luces;
using Burro;
using Burro.Util;
using Burro.BuildServers;

namespace Luces.Tests
{
    [TestFixture]
    public class CoreTest
    {
        private IKernel _kernel;
        private Mock<IDeviceManager> _deviceManager;
        private Mock<IBurroCore> _burro;
        private Mock<ILight> _light;

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
            Assert.IsNull(core.Lights);
            core.Initialise();
            Assert.AreEqual(2, core.Lights.Count());
            Assert.IsInstanceOf<ILight>(core.Lights.First());
        }

        [Test]
        public void InitialisationStartsParserWithDefaultConfig()
        {
            var core = _kernel.Get<LucesCore>();
            core.Initialise();
            _burro.Verify(b => b.Initialise("luces.yml"), Times.Once());
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
        // TODO
        public void ChangesToBuildPipelinesRaiseEvents()
        {
            var core = _kernel.Get<LucesCore>();

            var bs1 = new Mock<IBuildServer>();
            var bs2 = new Mock<IBuildServer>();
            var buildServers = new List<Mock<IBuildServer>>() { bs1, bs2 };

            _burro.Setup(b => b.BuildServers).Returns(new List<IBuildServer>(buildServers.Select(bs => bs.Object)));

            core.Initialise();


        }
    }
}
