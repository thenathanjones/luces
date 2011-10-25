using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Ninject;
using NUSB.Manager;
using Luces;

namespace Luces.Tests
{
    [TestFixture]
    public class CoreTest
    {
        private IKernel _kernel;
        private Mock<IDeviceManager> _deviceManager;

        [SetUp]
        public void Setup()
        {
            _kernel = new StandardKernel();
            _deviceManager = new Mock<IDeviceManager>();
            _kernel.Bind<IDeviceManager>().ToConstant(_deviceManager.Object);
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
    }
}
