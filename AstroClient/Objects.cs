﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstroClient
{
    internal class Objects
    {
        public class MenuOption
        {
            public string option { get; set; }
            public string warning { get; set; }
            public string identity { get; set; }
            public Color? color { get; set; }
            public Color? warningColor { get; set; }
            public bool? matchMenu { get; set; }
            public bool? newLine { get; set; }
        }
        public class ServerData
        {
            public string version { get; set; }
            public string changelogs { get; set; }
            public string status { get; set; }
            public string modpackVersion { get; set; }
            public string modpackChangelogs { get; set; }
        }
    }
}
