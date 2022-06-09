// MIT License
// 
// Copyright (c) 2022 SirRandoo
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Threading.Tasks;
using ToolkitExt.Core;
using Verse;

namespace ToolkitExt.Mod
{
    public class ExtensionMod : Verse.Mod
    {
        /// <inheritdoc/>
        public ExtensionMod(ModContentPack content) : base(content)
        {
            Instance = this;
            PollManager = new PollManager();
            Settings = GetSettings<ExtensionSettings>();
            Settings.LoadAuthSettings();
            
            if (Settings.Auth != null && !string.IsNullOrEmpty(Settings.Auth.ChannelId))
            {
                Task.Run(async () => await BackendClient.Instance.SetCredentials(Settings.Auth.ChannelId, Settings.Auth.Token));
            }
        }

        public static ExtensionSettings Settings { get; private set; }
        public static ExtensionMod Instance { get; private set; }
        public PollManager PollManager { get; }

        /// <inheritdoc/>
        public override void WriteSettings()
        {
            Settings.Write();
            Settings.SaveAuthSettings();
        }
    }
}
