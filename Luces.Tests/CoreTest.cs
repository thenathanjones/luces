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

namespace Luces.Tests
{
    [TestFixture]
    public class CoreTest
    {
        private IKernel _kernel;
        private Mock<IDeviceManager> _deviceManager;
        private Mock<IBurroCore> _burro;

        [SetUp]
        public void Setup()
        {
            _kernel = new StandardKernel();
            _deviceManager = new Mock<IDeviceManager>();
            _kernel.Bind<IDeviceManager>().ToConstant(_deviceManager.Object);
            _burro = new Mock<IBurroCore>();
            _kernel.Bind<IBurroCore>().ToConstant(_burro.Object);
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
        public void InitialisationStartsParserWithDefaultFile()
        {
            var core = _kernel.Get<LucesCore>();
            core.Initialise();
            _burro.Verify(b => b.Initialise("luces.yml"), Times.Once());
        }

        [Test]
        public void InitialisationAllowsFilePassedIn()
        {
            var core = _kernel.Get<LucesCore>();
            core.Initialise("file2.yml");
            _burro.Verify(b => b.Initialise("file2.yml"), Times.Once());
        }
    }
}
