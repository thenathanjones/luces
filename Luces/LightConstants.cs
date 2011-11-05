using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUSB.Interop;

namespace Luces
{
    public sealed class LightConstants
    {
        public static IList<LightConfig> KnownLightTypes = new List<LightConfig> 
        {
            new LightConfig() {Name = "DelcomV2", Guid = DeviceGuid.HID, VendorId = "0FC5", ProductId = "B080"},
        };
    }

    public struct LightConfig
    {
        public string Name;

        public Guid Guid;

        public string VendorId;

        public string ProductId;
    }
}
