using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ToolkitExt.Api.Events
{
    public interface IPusherEvent
    {
        string Event { get; set; }
    }
}
