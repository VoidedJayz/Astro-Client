using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AstroClient.Client;
using static AstroClient.Objects;
using Newtonsoft.Json;
using DiscordRPC;
using System.Diagnostics;

namespace AstroClient.Systems
{
    public class GameClient
    {
        private HttpListener _httpListener;
        private const string ServerUri = "http://localhost:4565/";
        private List<WebSocket> connectedClients = new List<WebSocket>();
        private WebSocketObjects currentData = new WebSocketObjects();

        public GameClient()
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(ServerUri);
        }

        // Socket
        public async Task Start()
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            _httpListener.Start();
            LogSystem.Log($"WebSocket Server started at {ServerUri}", "Pipeline");
            Task.Run(UpdateManager);
            while (true)
            {
                var context = await _httpListener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    await HandleWebSocketClient(context);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }
        private async void OnProcessExit(object sender, EventArgs e)
        {
            Shutdown().Wait();
        }
        private async Task HandleWebSocketClient(HttpListenerContext context)
        {
            WebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
            WebSocket webSocket = webSocketContext.WebSocket;

            // Add the connected client to the list
            connectedClients.Add(webSocket);
            LogSystem.Log($"Client connected: {context.Request.RemoteEndPoint}", "Pipeline");

            try
            {
                byte[] buffer = new byte[1024];
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    }
                    else if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        currentData = JsonConvert.DeserializeObject<WebSocketObjects>(receivedMessage);
                        LogSystem.Log($"Received: {receivedMessage}", "Pipeline");
                    }
                }
            }
            catch (WebSocketException wse)
            {
                // Handle WebSocket exceptions (e.g., abrupt closure)
                LogSystem.Log($"WebSocket Exception: {wse.Message}", "Pipeline");
            }
            catch (Exception e)
            {
                // Handle other exceptions
                LogSystem.Log($"Exception: {e.Message}", "Pipeline");
            }
            finally
            {
                if (webSocket != null)
                {
                    LogSystem.Log($"Client disconnected: {context.Request.RemoteEndPoint}", "Pipeline");
                    webSocket.Dispose();
                    connectedClients.Remove(webSocket); // Remove the client if it closes
                }
            }
        }

        public async Task SendAsync(string message)
        {
            var messageBuffer = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(messageBuffer);

            foreach (var webSocket in connectedClients)
            {
                if (webSocket.State == WebSocketState.Open)
                {
                    await webSocket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

        public async Task Shutdown()
        {
            // Close and dispose of all connected WebSocket clients
            foreach (var webSocket in connectedClients)
            {
                if (webSocket.State == WebSocketState.Open)
                {
                    LogSystem.Log($"Client Closed: {webSocket.CloseStatusDescription}", "Pipeline");
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server shutting down", CancellationToken.None);
                    webSocket.Dispose();
                }
            }

            // Stop the HttpListener
            if (_httpListener != null && _httpListener.IsListening)
            {
                _httpListener.Stop();
                _httpListener.Close();
            }
            LogSystem.Log("WebSocket Server shut down.", "Pipeline");
        }


        // In-Game
        public async Task UpdateManager()
        {
            while (true)
            {
                await Task.Delay(1000);
                if (connectedClients.Count > 0)
                {
                    await UpdatePresence();
                }
            }
        }
        public async Task UpdatePresence()
        {
            if (currentData.inMenus == true)
            {
                DiscordManager.UpdatePresence("company", "In-Game Menus..");
            }
            else
            {
                if (currentData.profitQuota > currentData.totalScrapValue)
                {
                    DiscordManager.UpdatePresence("company", $"Not makin quota.. {currentData.totalScrapValue}/{currentData.profitQuota}");
                }
                else
                {
                    DiscordManager.UpdatePresence("company", $"Makin quota! {currentData.totalScrapValue}/{currentData.profitQuota}");
                }
            }
        }


    }
}
