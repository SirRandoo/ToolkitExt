using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Verse;

namespace ToolkitNxt.Mod
{
    internal class WebSocketWrapper
    {
        public static WebSocketWrapper Instance { get; set; }

        ClientWebSocket _webSocket;

        static Queue<string> _messages = new Queue<string>();

        public static bool Subscribed = false;

        public WebSocketWrapper()
        {
            Instance = this;
            _webSocket = new ClientWebSocket();
            Initialize();
        }

        internal static void EnqueueMessage(string message)
        {
            _messages.Enqueue(message);
        }

        static void Initialize()
        {
            Task connect = ChatWithServer();
        }

        private static async Task ChatWithServer()
        {
            using (ClientWebSocket ws = new ClientWebSocket())
            {
                Uri serverUri = new Uri("wss://ws-us3.pusher.com/app/290b2ad8d139f7d58165?protocol=7&client=js&version=7.0.6&flash=false");
                await ws.ConnectAsync(serverUri, CancellationToken.None);
                while (true)
                {
                    if (_messages.Count > 0)
                    {
                        Log.Message("sending message");
                        ArraySegment<byte> bytestosend = new ArraySegment<byte>(
                            Encoding.UTF8.GetBytes(_messages.Dequeue()));
                        await ws.SendAsync(
                            bytestosend, WebSocketMessageType.Text,
                            true, CancellationToken.None);
                    }

                    ArraySegment<byte> bytesReceived = new ArraySegment<byte>(new byte[1024]);
                    WebSocketReceiveResult result = await ws.ReceiveAsync(
                        bytesReceived, CancellationToken.None);
                    PusherClient.ParseMessage(Encoding.UTF8.GetString(
                        bytesReceived.Array, 0, result.Count));
                    if (ws.State != WebSocketState.Open)
                    {
                        break;
                    }

                    Log.Message("Connected to Pusher Socket");
                }
            }
        }
    }
}
