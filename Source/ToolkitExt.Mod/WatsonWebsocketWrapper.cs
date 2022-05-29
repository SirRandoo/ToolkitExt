using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolkitNxt.Mod;
using Verse;
using WatsonWebsocket;

namespace ToolkitExt.Mod
{
    internal class WatsonWebsocketWrapper
    {
        static WatsonWsClient _client;

        public WatsonWebsocketWrapper()
        {
            _client = new WatsonWsClient(new Uri("wss://ws-us3.pusher.com/app/290b2ad8d139f7d58165?protocol=7&client=js&version=7.0.6&flash=false"));
            _client.ServerConnected += ServerConnected;
            _client.ServerDisconnected += ServerDisconnected;
            _client.MessageReceived += MessageReceived;
            _client.Start();
        }

        private static void MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            PusherClient.ParseMessage(Encoding.UTF8.GetString(e.Data));
        }

        private static void ServerDisconnected(object sender, EventArgs e)
        {
            Log.Message("Server Connected");
        }

        private static void ServerConnected(object sender, EventArgs e)
        {
            Log.Message("Server Connected");
        }

        internal static void SendMessage(string message)
        {
            _client.SendAsync(Encoding.UTF8.GetBytes(message));
        }
    }
}
