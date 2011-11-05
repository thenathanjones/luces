using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUSB.Controller;
using System.Threading;

namespace Luces.Lights
{
    internal class DelcomV2Light : ILight
    {
        private IUSBController _controller;

        private LightColor _lastColor = LightColor.Off;

        private const int FLASH_LENGTH = 300;

        public DelcomV2Light(IUSBController controller)
        {
            _controller = controller;
        }

        public void Initialise(string devicePath)
        {
            _controller.Initialise(devicePath, false);
        }

        public void Disconnect()
        {
            _controller.Disconnect();
        }

        public void Unknown()
        {
            ChangeColor(LightColor.White);
        }

        public void Success()
        {
            ChangeColor(LightColor.Green);
        }

        public void Building()
        {
            ChangeColor(LightColor.Blue);
        }

        public void Failure()
        {
            ChangeColor(LightColor.Red);
        }

        public void Off()
        {
            SetColor(LightColor.Off);
        }

        public void ChangeColor(LightColor color)
        {
            if (color != _lastColor)
            {
                SetColor(color);
                Thread.Sleep(FLASH_LENGTH);
                SetColor(_lastColor);
                Thread.Sleep(FLASH_LENGTH);
                SetColor(color);
                _lastColor = color;
            }
        }

        private void SetColor(LightColor color)
        {
            var controlBytes = new byte[8];
            controlBytes[0] = 0x65;
            controlBytes[1] = 0x0C;
            controlBytes[2] = (byte)color; // This is the LED byte
            controlBytes[3] = 0xFF;

            _controller.HidSetFeature(controlBytes);
        }

        public enum LightColor : byte
        {
            Red = 0x02,
            Green = 0x01,
            Blue = 0x04,
            Yellow = 0x03,
            Aqua = 0x05,
            Magenta = 0x06,
            White = 0x07,
            Off = 0x00
        }
    }
}
