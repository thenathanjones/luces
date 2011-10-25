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
            _deviceManager = new Mock<IDeviceManager>();
            _kernel = new StandardKernel();
        }

        [Test]
        public void InitialisationDetectsLights()
        {
            _kernel.Bind<IDeviceManager>().ToConstant(_deviceManager.Object);
            var core = _kernel.Get<LucesCore>();
            core.Initialise();
            _deviceManager.Verify(dm => dm.FindDevices(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }
    }
}
