﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Luces.Lights
{
    public interface ILight
    {
        void Unknown();

        void Success();

        void Building();

        void Failure();

        void Off();

        void Initialise(string devicePath);

        void Disconnect();
    }
}
