using System.Text.RegularExpressions;
using Verse;

namespace ToolkitNxt.Mod
{
    public class ToolkitExtSettings : Verse.ModSettings
    {
        // Insert broadcaster key from extension page here
        public static string broadcasterKey = "";
        public static string token = "";
        public static string channel_id = "";

        public static void SetupData()
        {
            token = Regex.Match(broadcasterKey, "\\A\\d+.([a-zA-Z]|\\d)+").Value;
            channel_id = Regex.Match(broadcasterKey, "\\d+$").Value;
            Log.Message("Matches: " + token + " ----- " + channel_id);
        }
    }
}
