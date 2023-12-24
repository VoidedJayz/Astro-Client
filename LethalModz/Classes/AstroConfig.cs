using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astro.Classes
{
    public class AstroConfig
    {
        public bool menuMusic { get; set; } = true;
        public bool autoUpdate { get; set; } = true;
        public string priotitySong { get; set; } = null;
        public string customPath { get; set; } = null;

        public void Save()
        {
            AstroFileSystem.WriteAllText("settings.json", JsonConvert.SerializeObject(this));
        }
        public static AstroConfig Load()
        {
            if (AstroFileSystem.FileExists("settings.json"))
            {
                return JsonConvert.DeserializeObject<AstroConfig>(AstroFileSystem.ReadAllText("settings.json"));
            }
            return new AstroConfig();
        }
    }
}
