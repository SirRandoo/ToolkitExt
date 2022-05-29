using System.Text.RegularExpressions;
using Verse;

namespace ToolkitNxt.Mod
{
    public class ToolkitExtSettings : Verse.ModSettings
    {
        public static string broadcasterKey = "1|fB37MPyzpu0aliQbtshblyRpCNiuf9dNRkuShLei.124055459";
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
