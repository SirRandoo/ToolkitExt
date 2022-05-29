using Newtonsoft.Json;
using ToolkitExt.Api.Events;
using ToolkitExt.Api.Events.Responses;
using Verse;

namespace ToolkitNxt.Mod
{
    internal class PusherClient
    {
        public static void ParseMessage(string message)
        {
            message = message.Replace("\\\"", "'");
            message = message.Replace('"', '\'');
            message = message.Replace("'{", "{");
            message = message.Replace("}'", "}");

            var definition = new { Data = "" };
            Log.Message(message);
            Log.Message("Attempting to deserialize");
            PusherEvent pusherEvent = JsonConvert.DeserializeObject<PusherEvent>(message);

            switch (pusherEvent.Event)
            {
                case "pusher:connection_established":
                    ConnectionEstablished @event = JsonConvert.DeserializeObject<ConnectionEstablished>(message);
                    AuthenticationController.ConnectionEstablished(@event);
                    break;
                default:

                    break;
            }
        }
    }
}
