using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ToolkitExt.Api.Events
{
    public class PusherEvent : IPusherEvent
    {
        [JsonProperty("event")]
        public string Event { get; set; }
    }
}
