using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ToolkitExt.Api.Events.Requests;
using ToolkitExt.Mod;
using Verse;

namespace ToolkitNxt.Mod
{
    [Serializable]
    public class PusherEventController
    {
        internal static void Subscribe(string authKey, string channelName)
        {
            //WebSocketWrapper.EnqueueMessage("{'event':'pusher:subscribe','data':{'auth':'" + authKey + "', 'channel':'" + channelName + "'}}");
            Subscribe @event = SubscribeEvent(authKey, channelName);
            string output = JsonConvert.SerializeObject(@event);
            Log.Message(output);
            WatsonWebsocketWrapper.SendMessage(output);
        }

        static Subscribe SubscribeEvent(string authKey, string channelName)
        {
            Subscribe subscribeEvent = new Subscribe();
            subscribeEvent.Event = "pusher:subscribe";
            subscribeEvent.Data = new SubscribeData();
            subscribeEvent.Data.Auth = authKey;
            subscribeEvent.Data.Channel = channelName;

            return subscribeEvent;
        }
    }
}
