using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstroClient.Systems
{
    internal class ConfigSystem
    {
        public static ConfigSystem? loadedConfig;

        // Config Values
        public bool backgroundMusic { get; set; } = true;
        public bool autoUpdateAstro { get; set; } = true;
        public string? prioritySong { get; set; } = null;
        public string? customPath { get; set; } = null;

        public void Save()
        {
            string json = JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
            FileSystem.WriteAllText("config.json", json);
        }
        public static ConfigSystem Load()
        {
            if (!File.Exists("config.json"))
            {
                return new ConfigSystem();
            }
            else
            {
                string json = FileSystem.ReadAllText("config.json");
                return JsonConvert.DeserializeObject<ConfigSystem>(json);
            }
        }

        public static async Task GetConfig()
        {
            try
            {
                LogSystem.Log("Attempting to load configuration.");

                loadedConfig = Load();
                await Task.Delay(1000);

                if (loadedConfig != null)
                {
                    LogSystem.Log("Configuration loaded successfully.");
                }
                else
                {
                    LogSystem.Log("Failed to load configuration: currentConfig is null.");
                }
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"Error in GetConfig: {ex.Message}");
            }
        }
        public static void ResetCurrentConfigPath()
        {
            loadedConfig.customPath = null;
            loadedConfig.Save();
            LogSystem.Log("Current custom path reset and configuration saved.");
        }
    }
}
