using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ToolkitExt.Api.Events.Responses
{
    public class SubscriptionSucceeded : IPusherEvent
    {
        [JsonProperty("event")]
        public string Event { get; set; }

        [JsonProperty("data")]
        public SubscriptionSucceededData Data { get; set; }

        [JsonProperty("channel")]
        public string Channel { get; set; }
    }

    public class SubscriptionSucceededData
    {

    }
}
