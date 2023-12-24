using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Objects

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
}
