using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ToolkitExt.Api.Events.Responses
{
    public class ConnectionEstablished : IPusherEvent
    {
        [JsonProperty("event")]
        public string Event { get; set; }

        [JsonProperty("data")]
        public ConnectionEstablishedData Data { get; set; }
    }

    public class ConnectionEstablishedData
    {
        [JsonProperty("socket_id")]
        public string SocketId { get; set; }

        [JsonProperty("activity_timeout")]
        public int ActivityTimeout { get; set; }
    }
}
