using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolkitExt.Mod;
using Verse;

namespace ToolkitNxt.Mod
{
    public class ToolkitNxt : Verse.Mod
    {
        public ToolkitNxt(ModContentPack content) : base(content)
        {
            
        }
    }

    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            ToolkitExtSettings.SetupData();
            new WatsonWebsocketWrapper();
        }
    }
}
