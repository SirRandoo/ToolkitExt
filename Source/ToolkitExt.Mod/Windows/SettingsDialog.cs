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
using ToolkitExt.Api;
using ToolkitExt.Core;
using ToolkitExt.Core.Entities;
using UnityEngine;
using Verse;

namespace ToolkitExt.Mod.Windows
{
    public class SettingsDialog : ProxySettingsWindow
    {
        private bool _displayingKey;
        private readonly string _lastToken;
        private string _settingsHeader;
        private string _keyLabel;
        private string _keyDescription;
        private string _generalGroup;
        private string _largeTextLabel;
        private string _largeTextDescription;
        private string _showKeyTooltip;
        private string _hideKeyTooltip;
        private string _pasteKeyTooltip;
        private string _pollGroup;
        private CompositeLabel _headerLabel;

        /// <inheritdoc/>
        public SettingsDialog() : base(ExtensionMod.Instance)
        {
            _lastToken = ExtensionMod.Settings.Auth.Token;
        }

        /// <inheritdoc/>
        public override void DoWindowContents(Rect region)
        {
            GUI.BeginGroup(region);

            var headerRect = new Rect(0f, 0f, region.width, Text.LineHeight * 2f);
            var separatorRect = new Rect(10f, headerRect.height + 2f, region.width - 20f, Text.LineHeight);

            float settingsY = separatorRect.y + separatorRect.height + 2f;
            var settingsRect = new Rect(0f, settingsY, region.width, region.height - settingsY);

            GUI.BeginGroup(headerRect);
            DrawSettingsHeader(headerRect);
            GUI.EndGroup();

            Widgets.DrawLineHorizontal(separatorRect.x, separatorRect.y, separatorRect.width);

            GUI.BeginGroup(settingsRect);
            var listing = new Listing_Standard();
            listing.Begin(settingsRect.AtZero());
            
            DrawGeneralSettings(listing);
            DrawPollSettings(listing);

            listing.End();
            GUI.EndGroup();

            GUI.EndGroup();
        }

        private void DrawSettingsHeader(Rect region)
        {
            _headerLabel ??= CompositeLabel.Compile(region, _settingsHeader);

            _headerLabel.Draw();
        }

        private void DrawGeneralSettings([NotNull] Listing listing)
        {
            listing.GroupHeader(_generalGroup, false);

            (Rect keyLabel, Rect keyField) = listing.Split();
            UiHelper.Label(keyLabel, _keyLabel);

            (Rect trueKeyField, Rect buttonRects) = keyField.Split(0.7f);
            (Rect visibilityButton, Rect pasteButton) = buttonRects.Split(0.5f);

            if (_displayingKey)
            {
                if (UiHelper.TextField(trueKeyField, ExtensionMod.Settings.Auth.BroadcasterKey, out string key))
                {
                    ExtensionMod.Settings.Auth.BroadcasterKey = key;
                }
            }
            else
            {
                ExtensionMod.Settings.Auth.BroadcasterKey = GUI.PasswordField(trueKeyField, ExtensionMod.Settings.Auth.BroadcasterKey, '*');
            }

            UiHelper.Icon(visibilityButton, _displayingKey ? Textures.Hidden : Textures.Visible, Color.white);
            TooltipHandler.TipRegion(visibilityButton, _displayingKey ? _hideKeyTooltip : _showKeyTooltip);

            if (Widgets.ButtonInvisible(visibilityButton))
            {
                _displayingKey = !_displayingKey;
            }

            UiHelper.Icon(pasteButton, TexButton.Paste, Color.white);
            TooltipHandler.TipRegion(pasteButton, _pasteKeyTooltip);

            if (Widgets.ButtonInvisible(pasteButton))
            {
                ExtensionMod.Settings.Auth.BroadcasterKey = GUIUtility.systemCopyBuffer;
            }
            
            listing.DrawDescription(_keyDescription);
        }

        private void DrawPollSettings([NotNull] Listing listing)
        {
            listing.GroupHeader(_pollGroup);

            Widgets.CheckboxLabeled(listing.GetRect(Text.LineHeight), _largeTextLabel, ref ExtensionMod.Settings.Polls.LargeText);
            listing.DrawDescription(_largeTextDescription);
        }

        /// <inheritdoc />
        protected override void GetTranslations()
        {
            _generalGroup = "TExt.SettingGroups.General".TranslateSimple();
            _pollGroup = "TExt.SettingGroups.Poll".TranslateSimple();
            
            _settingsHeader = string.Format("TExt.Headers.ClientSideSettings".TranslateSimple(), SiteMap.Dashboard.AbsoluteUri);
            
            _keyLabel = "TExt.Settings.BroadcasterKey.Label".TranslateSimple();
            _keyDescription = string.Format("TExt.Settings.BroadcasterKey.Description".TranslateSimple(), SiteMap.Dashboard.AbsoluteUri);

            _largeTextLabel = "TExt.Settings.LargeText.Label".TranslateSimple();
            _largeTextDescription = "TExt.Settings.LargeText.Description".TranslateSimple();

            _pasteKeyTooltip = "TExt.Tooltips.PasteBroadcasterKey".TranslateSimple();
            _showKeyTooltip = "TExt.Tooltips.ToggleBroadcasterKey.Hidden".TranslateSimple();
            _hideKeyTooltip = "TExt.Tooltips.ToggleBroadcasterKey.Visible".TranslateSimple();
        }

        /// <inheritdoc/>
        public override void PostClose()
        {
            if (!string.Equals(_lastToken, ExtensionMod.Settings.Auth.Token))
            {
                Task.Run(async () => await BackendClient.Instance.SetCredentials(ExtensionMod.Settings.Auth.ChannelId, ExtensionMod.Settings.Auth.Token));
            }
        }
    }
}
