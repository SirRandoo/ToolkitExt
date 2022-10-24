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
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using JetBrains.Annotations;
using ToolkitExt.Api;
using ToolkitExt.Mod.Windows;
using UnityEngine;
using Verse;

namespace ToolkitExt.Mod
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public static class IndexExecutables
    {
        private static readonly Uri DiscordUrl = new Uri("https://discord.gg/qrtg224");
        
        public static void OpenToBroadcasterKey()
        {
            Find.WindowStack.Add(new SettingsDialog());
        }

        public static void OpenToLargeText()
        {
            Find.WindowStack.Add(new SettingsDialog());
        }

        public static void OpenStreamerSettings()
        {
            Application.OpenURL(SiteMap.Dashboard.AbsoluteUri);
        }

        public static void OpenDiscordLink()
        {
            Application.OpenURL(DiscordUrl.AbsoluteUri);
        }

        public static void OpenLiveChannels()
        {
            Application.OpenURL(SiteMap.Live.AbsoluteUri);
        }
    }
}
