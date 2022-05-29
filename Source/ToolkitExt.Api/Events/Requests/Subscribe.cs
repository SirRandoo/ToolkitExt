using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ToolkitExt.Api.Events.Requests
{
    public class Subscribe : IPusherEvent
    {
        [JsonProperty("event")]
        public string Event { get; set; }

        [JsonProperty("data")]
        public SubscribeData Data { get; set; }
    }

    public class SubscribeData
    {
        [JsonProperty("auth")]
        public string Auth { get; set; }

        [JsonProperty("channel")]
        public string Channel { get; set; }
    }
}
