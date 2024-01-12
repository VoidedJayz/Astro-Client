using BepInEx;
using GameNetcodeStuff;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AstroLibrary
{
    [BepInPlugin("AstroLibrary", "Astro Client Handler", "2.0.0")]
    public class Main : BaseUnityPlugin
    {
        public static bool godMode = false;
        public static bool infiniteStamina = false;
        public static bool globalMenu = true;
        public static int TotalScrapValue = 0;
        public static bool connected = false;
        public static string prevLevel = "";
        public readonly Harmony hsec = new Harmony("AstroLibrary");
        public static ClientWebSocket webSocket;
        public static Uri serverUri = new Uri("ws://localhost:4565");
        public static TipQueue tipQueue = new TipQueue();

        void Start()
        {
            File.Create("AstroLibrary.log").Close();
            hsec.PatchAll(typeof(Main));
            Task.Run(async () =>
            {
                await AstroLobbyManagerFunction();
            });
            Task.Run(async () =>
            {
                await EnsureWebSocketConnection();
            });
        }
        [HarmonyPatch(typeof(PlayerControllerB), "AllowPlayerDeath")]
        [HarmonyPrefix]
        static bool GodMode()
        {
            if (godMode == true)
            {
                return false;
            }
            else { return true; }
        }
        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        static void InfiniteStamina(ref float ___sprintMeter)
        {

            if (infiniteStamina) { Mathf.Clamp(___sprintMeter += 0.02f, 0f, 1f); }
        }
        private bool IsWebSocketHostAccessible(string host)
        {
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(host);
                IPAddress[] addresses = hostEntry.AddressList;
                return addresses.Length > 0;
            }
            catch (SocketException)
            {
                return false;
            }
        }
        private async Task EnsureWebSocketConnection()
        {
            string websocketHost = "localhost"; // Replace with your actual WebSocket host
            Uri serverUri = new Uri($"ws://{websocketHost}:4565"); // WebSocket server URI

            while (true)
            {
                LogToFile("Checking WebSocket connection accessibility...");
                if (!IsWebSocketHostAccessible(websocketHost))
                {
                    LogToFile($"WebSocket host '{websocketHost}' is not accessible. Retrying in 2 seconds.");
                    await Task.Delay(2000);
                    continue;
                }
                LogToFile($"WebSocket host '{websocketHost}' is accessible.");

                // Proceed with WebSocket connection initialization here if needed
                // Example: Create a new WebSocket instance and attempt to connect
                using (ClientWebSocket ws = new ClientWebSocket())
                {
                    try
                    {
                        webSocket = ws;
                        await webSocket.ConnectAsync(serverUri, CancellationToken.None);
                        LogToFile("WebSocket connected.");
                        await tipQueue.EnqueueTip("Astro Client V3.0.0", "Astro's pipeline has successfully connected!");
                        await ReceiveMessagesAsync();
                    }
                    catch (WebSocketException ex)
                    {
                        LogToFile($"WebSocket connection failed: {ex.Message}. Retrying in 2 seconds.");
                    }
                }

                await Task.Delay(2000); // Add a delay before the next connection attempt
            }
        }
        public static bool IsWebSocketHostReachable(string host)
        {
            try
            {
                System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
                PingReply reply = ping.Send(host);

                if (reply.Status == IPStatus.Success)
                {
                    return true; // Host is reachable
                }
            }
            catch (PingException)
            {
                // Host is not reachable
            }

            return false;
        }
        public async Task ReceiveMessagesAsync()
        {
            var buffer = new byte[1024];
            while (webSocket.State == WebSocketState.Open)
            {
                try
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        var json = JsonConvert.DeserializeObject<WebSocketObjects>(receivedMessage);
                        godMode = json.GodMode;
                        infiniteStamina = json.InfiniteStamina;
                        LogToFile("Received: " + receivedMessage);
                        UpdateStuff($"Commands received!");
                    }
                }
                catch (Exception e)
                {
                    connected = false;
                    LogToFile("WebSocket receive error: " + e.Message);
                }
            }
            if (webSocket != null)
            {
                webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Requesting Exit", CancellationToken.None).Wait();
                webSocket.Dispose();
                webSocket = new ClientWebSocket();
                LogToFile("WebSocket closed, and regenerated.");
            }
            LogToFile("Message Receiver closed.");
        }
        public async Task SendAsync(string data)
        {
            try
            {
                if (webSocket != null && webSocket.State == WebSocketState.Open)
                {
                    try
                    {
                        var messageBuffer = Encoding.UTF8.GetBytes(data);
                        await webSocket.SendAsync(new ArraySegment<byte>(messageBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    catch (Exception e)
                    {
                        LogToFile("WebSocket send error: " + e.Message);
                    }
                }
                else
                {
                    LogToFile("WebSocket is not open. Attempting to reconnect.");
                }
            }
            catch (Exception e)
            {
                LogToFile("Error sending data to WebSocket! " + e.Message);
            }
        }
        public async void UpdateStuff(string msg, bool dc = false)
        {
            if (webSocket == null || webSocket.State != WebSocketState.Open) { return; }
            try
            {
                TotalScrapValue = 0;
                string data = "";
                if (!dc)
                {
                    if (globalMenu == false)
                    {
                        var grabbableObjects = GameObject.Find("/Environment/HangarShip").GetComponentsInChildren<GrabbableObject>();
                        foreach (var obj in grabbableObjects)
                        {
                            if (obj.name != "ClipboardManual" && obj.name != "StickyNoteItem")
                            {
                                TotalScrapValue += obj.scrapValue;
                            }
                        }
                    }
                    data = JsonConvert.SerializeObject(new WebSocketObjects
                    {
                        Message = msg,
                        GodMode = godMode,
                        InfiniteStamina = infiniteStamina,
                        daysUntilDeadline = TimeOfDay.Instance.daysUntilDeadline,
                        profitQuota = TimeOfDay.Instance.profitQuota,
                        totalScrapValue = TotalScrapValue,
                        inMenus = globalMenu,
                    });
                }
                else
                {
                    data = JsonConvert.SerializeObject(new WebSocketObjects
                    {
                        Message = msg,
                        GodMode = godMode,
                        InfiniteStamina = infiniteStamina,
                        inMenus = globalMenu,
                    });
                }
                LogToFile("Sending: " + data);
                await SendAsync(data);
            }
            catch (Exception e)
            {
                LogToFile("Error sending data to WebSocket! " + e);
            }
        }
        public async Task LogToFile(string msg)
        {
            Logger.LogInfo(msg);
        }
        public async Task CustomTip(string title, string message, bool isWarn = false)
        {
            if (HUDManager.Instance != null)
            {
                HUDManager.Instance.DisplayTip(title, message, isWarn);
            }
        }
        private void OnApplicationQuit()
        {
            if (webSocket != null)
            {
                webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Requesting Exit", CancellationToken.None).Wait();
                webSocket.Dispose();
                LogToFile("WebSocket closed.");
            }
        }
        public async Task AstroLobbyManagerFunction()
        {
            bool menu = true; // You may initialize this as needed

            LC_API.GameInterfaceAPI.GameState.WentIntoOrbit += () => UpdateStuff("Orbit Update triggered new data.");
            LC_API.GameInterfaceAPI.GameState.PlayerDied += () => UpdateStuff("Player Death triggered new data.");
            LC_API.GameInterfaceAPI.GameState.LandOnMoon += () => UpdateStuff("Land On Moon triggered new data.");
            LC_API.GameInterfaceAPI.GameState.ShipStartedLeaving += () => UpdateStuff("Ship leaving triggered new data.");
            SceneManager.sceneLoaded += (s, m) => 
            {
                Task.Delay(4000).Wait();
                if (s.name == "SampleSceneRelay")
                {
                    menu = false; globalMenu = menu;
                    UpdateStuff("Lobby Connected!");

                }
                else if (s.name == "MainMenu")
                {
                    menu = true; globalMenu = menu;
                    UpdateStuff("Lobby Disconnected!");
                }
            };

        }
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

    public class TipQueue
    {
        private readonly Queue<(string title, string message, bool isWarn)> tipQueue = new Queue<(string, string, bool)>();
        private readonly int maxQueueSize = 5;
        private bool processingQueue = false;

        public async Task EnqueueTip(string title, string message, bool isWarn = false)
        {
            lock (tipQueue)
            {
                if (tipQueue.Count < maxQueueSize)
                {
                    tipQueue.Enqueue((title, message, isWarn));
                }
                // If the queue is full, additional requests are ignored.
            }

            if (!processingQueue)
            {
                await ProcessQueue();
            }
        }

        private async Task ProcessQueue()
        {
            while (true)
            {
                (string title, string message, bool isWarn) tip;

                lock (tipQueue)
                {
                    if (tipQueue.Count == 0)
                    {
                        processingQueue = false;
                        return;
                    }

                    tip = tipQueue.Dequeue();
                }

                if (HUDManager.Instance != null)
                {
                    HUDManager.Instance.DisplayTip(tip.title, tip.message, tip.isWarn);
                }

                // Add a delay or await here if you want to control the pace of tip display.
                await Task.Delay(100); // Example delay, you can adjust it as needed.
            }
        }
    }

}
