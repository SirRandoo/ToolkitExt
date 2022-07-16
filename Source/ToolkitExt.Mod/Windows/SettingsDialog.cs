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
using System.Threading.Tasks;
using JetBrains.Annotations;
using SirRandoo.CommonLib.Helpers;
using SirRandoo.CommonLib.Windows;
using ToolkitExt.Core;
using UnityEngine;
using Verse;

namespace ToolkitExt.Mod.Windows
{
    public class SettingsDialog : ProxySettingsWindow
    {
        private bool _displayingKey;
        private bool _tokenChanged;

        /// <inheritdoc/>
        public SettingsDialog() : base(ExtensionMod.Instance)
        {
        }

        /// <inheritdoc/>
        public override void DoWindowContents(Rect region)
        {
            GUI.BeginGroup(region);

            var listing = new Listing_Standard();

            listing.Begin(region.AtZero());

            DrawGeneralSettings(listing);
            DrawPollSettings(listing);

            listing.End();

            GUI.EndGroup();
        }

        private void DrawGeneralSettings([NotNull] Listing listing)
        {
            listing.GroupHeader("General", false);

            (Rect keyLabel, Rect keyField) = listing.Split();
            UiHelper.Label(keyLabel, "Broadcaster key");

            (Rect trueKeyField, Rect buttonRects) = keyField.Split(0.7f);
            (Rect visibilityButton, Rect pasteButton) = buttonRects.Split(0.5f);

            if (_displayingKey)
            {
                if (UiHelper.TextField(trueKeyField, ExtensionMod.Settings.Auth.BroadcasterKey, out string token))
                {
                    _tokenChanged = true;
                    ExtensionMod.Settings.Auth.BroadcasterKey = token;
                }
            }
            else
            {
                ExtensionMod.Settings.Auth.BroadcasterKey = GUI.PasswordField(trueKeyField, ExtensionMod.Settings.Auth.BroadcasterKey, '*');
            }

            UiHelper.Icon(visibilityButton, _displayingKey ? Textures.Hidden : Textures.Visible, Color.white);
            TooltipHandler.TipRegion(visibilityButton, "Click to toggle the visibility of the broadcaster key.");

            if (Widgets.ButtonInvisible(visibilityButton))
            {
                _displayingKey = !_displayingKey;
            }

            UiHelper.Icon(pasteButton, TexButton.Paste, Color.white);
            TooltipHandler.TipRegion(pasteButton, "Click to paste the broadcaster key from your clipboard.");

            if (Widgets.ButtonInvisible(pasteButton))
            {
                ExtensionMod.Settings.Auth.BroadcasterKey = GUIUtility.systemCopyBuffer;
            }
        }

        private void DrawPollSettings([NotNull] Listing listing)
        {
            listing.GroupHeader("Poll");

            Widgets.CheckboxLabeled(listing.GetRect(Text.LineHeight), "Use larger text on the poll display.", ref ExtensionMod.Settings.Polls.LargeText);
        }

        /// <inheritdoc/>
        public override void PostClose()
        {
            if (_tokenChanged)
            {
                Task.Run(async () => await BackendClient.Instance.SetCredentials(ExtensionMod.Settings.Auth.ChannelId, ExtensionMod.Settings.Auth.Token));
            }
        }
    }
}
