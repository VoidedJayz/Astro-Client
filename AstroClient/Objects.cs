using System;
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
        public class WebSocketObjects
        {
            public string Message { get; set; } = "???";
            public bool GodMode { get; set; } = false;
            public bool InfiniteStamina { get; set; } = false;
            public int daysUntilDeadline { get; set; } = 0;
            public int profitQuota { get; set; } = 0;
            public int totalScrapValue { get; set; } = 0;
            public bool inMenus { get; set; } = true;
        }
        public class MenuOptionV2
        {
            public string FeatureName { get; set; } = "???";
            public string FeatureDescription { get; set; } = "???";
            public Color FeatureColor { get; set; } = Color.White;
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
