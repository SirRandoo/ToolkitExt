using System.Linq;
using ToolkitExt.Api.Events.Responses;
using Verse;

namespace ToolkitNxt.Mod
{
    internal class AuthenticationController
    {
        static string SocketId { get; set; } 

        internal static async void ConnectionEstablished(ConnectionEstablished connectionEstablishedEvent)
        {
            SocketId = connectionEstablishedEvent.Data.SocketId;

            if (SocketId == null)
            {
                return;
            }

            string body = "{'socket_id':'" + SocketId + "', 'channel_name':'private-private." + ToolkitExtSettings.channel_id + "'}";
            body = body.Replace('\'', '"');

            Log.Message(body);
            await HttpClientWrapper.Post("api/broadcasting/auth", ToolkitExtSettings.token, body);
        }

        internal static void PusherAuthTokenRecieved(AuthResponse data)
        {
            PusherEventController.Subscribe(data.Auth, "private-private." + ToolkitExtSettings.channel_id);
            WebSocketWrapper.Subscribed = true;
        }
    }
}
