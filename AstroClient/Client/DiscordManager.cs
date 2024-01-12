using AstroClient.Systems;
using DiscordRPC;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstroClient.Client
{
    internal class DiscordManager
    {
        private static DiscordRpcClient? client;

        public static void Start()
        {
            try
            {
                client = new DiscordRpcClient("1187549516199628850");
                client.Initialize();
                client.SetPresence(new RichPresence()
                {
                    Details = "In Menus",
                    State = UpdateManager.Version.ToString(),
                    Assets = new Assets()
                    {
                        LargeImageKey = "astrov2",

                    },
                });
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"Error in DiscordManager: {ex}");
            }
        }

        public static void UpdatePresence(string icon, string state)
        {
            try
            {
                client?.SetPresence(new RichPresence()
                {
                    Details = state,
                    State = UpdateManager.Version.ToString(),
                    Assets = new Assets()
                    {
                        LargeImageKey = "astrov2",
                    },
                });
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"Error in UpdatePresence: {ex}");
            }
        }
    }
}
