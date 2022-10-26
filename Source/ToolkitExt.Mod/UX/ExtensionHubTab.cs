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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using RimWorld;
using SirRandoo.CommonLib.Helpers;
using ToolkitExt.Api;
using ToolkitExt.Api.Enums;
using ToolkitExt.Core;
using ToolkitExt.Core.Workers;
using ToolkitExt.Mod.Entities;
using ToolkitExt.Mod.Windows;
using UnityEngine;
using Verse;

namespace ToolkitExt.Mod.UX
{
    [UsedImplicitly]
    [StaticConstructorOnStartup]
    internal class ExtensionHubTab : MainTabWindow
    {
        private const int PingPeriod = 150;
        private const int SpinnerPeriod = 150;
        private static readonly RimLogger Logger = new RimLogger("UX.HubTab");
        private static readonly JifWorker PingWorker = JifWorker.Load("UI/PingSheet");
        private static readonly JifWorker SpinnerWorker = JifWorker.Load("UI/SpinnerSheet");
        private readonly QuickSearchWidget _searchWidget = new QuickSearchWidget();

        private string _connectedLabel;

        private string _connectedTooltip;
        private string _connectingLabel;
        private string _connectingTooltip;
        private string _disconnectedLabel;
        private string _disconnectedTooltip;
        private string _disconnectingLabel;
        private string _disconnectingTooltip;

        private Vector2 _logScrollPos = Vector2.zero;
        private string _reconnectingLabel;
        private string _reconnectingTooltip;
        private string _reconnectTooltip;
        private string _settingsTooltip;
        private string _subscribedLabel;
        private string _subscribedTooltip;
        private string _subscribingLabel;
        private string _subscribingTooltip;
        private float _searchResultsHeight;
        private Vector2 _searchScrollPos = Vector2.zero;
        private ReadOnlyCollection<SearchResult> _searchResults;

        /// <inheritdoc/>
        public override MainTabWindowAnchor Anchor => MainTabWindowAnchor.Left;

        /// <inheritdoc/>
        public override Vector2 RequestedTabSize => new Vector2(Mathf.FloorToInt(UI.screenWidth * 0.4f), Mathf.FloorToInt(UI.screenHeight * 0.4f));

        /// <inheritdoc/>
        public override void PreClose()
        {
            base.PreClose();

            PingWorker.Stop();
            SpinnerWorker.Stop();
        }

        /// <inheritdoc/>
        public override void PreOpen()
        {
            base.PreOpen();

            FetchTranslations();
            SpinnerWorker.Start(SpinnerPeriod);
            _searchWidget?.Reset();
        }

        /// <inheritdoc/>
        public override void DoWindowContents(Rect inRect)
        {
            GUI.BeginGroup(inRect);

            var headerRegion = new Rect(0f, 0f, inRect.width, Text.LineHeight);
            var footerRegion = new Rect(0f, inRect.height - Text.LineHeight, inRect.width, Text.LineHeight);
            Rect contentRegion = new Rect(0f, headerRegion.height, inRect.width, inRect.height - headerRegion.height - footerRegion.height).ContractedBy(10f);

            GUI.BeginGroup(headerRegion);
            DrawHeader(headerRegion);
            GUI.EndGroup();

            GUI.BeginGroup(contentRegion);

            if (_searchResults != null && _searchResults.Count > 0)
            {
                DrawSearchResults(contentRegion.AtZero());
            }
            else
            {
                DrawContent(contentRegion.AtZero());
            }

            GUI.EndGroup();

            GUI.BeginGroup(footerRegion);
            DrawFooter(footerRegion.AtZero());
            GUI.EndGroup();

            GUI.EndGroup();
        }

        private void DrawHeader(Rect region)
        {
            Rect indicatorRegion = LayoutHelper.IconRect(0f, 0f, Text.LineHeight, Text.LineHeight, 0f);
            var stateRegion = new Rect(indicatorRegion.x + indicatorRegion.width + 4f, 0f, Mathf.FloorToInt(region.width * 0.47f), Text.LineHeight);
            var conStatusRegion = new Rect(0f, 0f, stateRegion.x + stateRegion.width, stateRegion.height);

            float searchWidth = Mathf.FloorToInt(region.width * 0.35f);
            var searchRegion = new Rect(region.width - searchWidth, 0f, searchWidth, Text.LineHeight);

            DrawConnectionIcon(indicatorRegion);
            DrawConnectionState(stateRegion);
            TipConnectionStatus(conStatusRegion);

            _searchWidget.OnGUI(searchRegion, ProcessSearchRequest);
        }

        private static void DrawConnectionIcon(Rect region)
        {
            ConnectionState state = BackendClient.Instance.WsState;

            switch (state)
            {
                case ConnectionState.Connecting:
                    PingWorker.TryRestart(PingPeriod);
                    PingWorker.Draw(region);

                    break;
                case ConnectionState.Connected:
                    PingWorker.ToLastFrame();
                    PingWorker.Draw(region);

                    break;
                case ConnectionState.Reconnecting:
                case ConnectionState.Disconnecting:
                    PingWorker.TryRestart(PingPeriod);
                    DrawColoredPing(region, Color.yellow);

                    break;
                case ConnectionState.Disconnected:
                    PingWorker.ToLastFrame();
                    DrawColoredPing(region, Color.red);

                    break;
                case ConnectionState.Subscribing:
                    PingWorker.TryRestart(PingPeriod);
                    DrawColoredPing(region, ColorLibrary.Turquoise);

                    break;
                case ConnectionState.Subscribed:
                    PingWorker.ToLastFrame();
                    DrawColoredPing(region, ColorLibrary.Turquoise);

                    break;
                default:
                    Logger.Warn($@"Cannot draw indicator for unknown connection state ""{state.ToStringFast()}""");

                    break;
            }
        }

        private void DrawConnectionState(Rect region)
        {
            ConnectionState state = BackendClient.Instance.WsState;

            switch (state)
            {
                case ConnectionState.Connecting:
                    UiHelper.Label(region, _connectingLabel);

                    break;
                case ConnectionState.Connected:
                    UiHelper.Label(region, _connectedLabel);

                    break;
                case ConnectionState.Disconnecting:
                    UiHelper.Label(region, _disconnectingLabel);

                    break;
                case ConnectionState.Disconnected:
                    UiHelper.Label(region, _disconnectedLabel);

                    break;
                case ConnectionState.Reconnecting:
                    UiHelper.Label(region, _reconnectingLabel);

                    break;
                case ConnectionState.Subscribing:
                    UiHelper.Label(region, _subscribingLabel);

                    break;
                case ConnectionState.Subscribed:
                    UiHelper.Label(region, _subscribedLabel);

                    break;
                default:
                    Logger.Warn($@"Cannot label region of unknown connection state ""{state.ToStringFast()}""");

                    break;
            }
        }

        private void TipConnectionStatus(Rect region)
        {
            ConnectionState state = BackendClient.Instance.WsState;

            switch (state)
            {
                case ConnectionState.Connecting:
                    TooltipHandler.TipRegion(region, _connectingTooltip);

                    break;
                case ConnectionState.Connected:
                    TooltipHandler.TipRegion(region, _connectedTooltip);

                    break;
                case ConnectionState.Disconnecting:
                    TooltipHandler.TipRegion(region, _disconnectingTooltip);

                    break;
                case ConnectionState.Disconnected:
                    TooltipHandler.TipRegion(region, _disconnectedTooltip);

                    break;
                case ConnectionState.Reconnecting:
                    TooltipHandler.TipRegion(region, _reconnectingTooltip);

                    break;
                case ConnectionState.Subscribing:
                    TooltipHandler.TipRegion(region, _subscribingTooltip);

                    break;
                case ConnectionState.Subscribed:
                    TooltipHandler.TipRegion(region, _subscribedTooltip);

                    break;
                default:
                    Logger.Warn($@"Cannot tip region of unknown connection state ""{state.ToStringFast()}""");

                    break;
            }
        }

        private void DrawContent(Rect region)
        {
            var logGroupRegion = new Rect(0f, 0f, Mathf.FloorToInt(region.width * 0.5f) - 2f, region.height);
            var logHeaderRegion = new Rect(0f, 0f, logGroupRegion.width, Text.LineHeight);
            var logRegion = new Rect(0f, Text.LineHeight, logGroupRegion.width, region.height - logHeaderRegion.height);

            List<string> messages = HubMessageLog.AllMessages.ToList();
            var logViewRegion = new Rect(0f, 0f, logRegion.width - 16f, messages.Count * Text.LineHeight);

            GUI.BeginGroup(logGroupRegion);
            UiHelper.Label(logHeaderRegion, "Event Log", ColorLibrary.LightBlue, TextAnchor.MiddleCenter, GameFont.Small);

            _logScrollPos = GUI.BeginScrollView(logRegion, _logScrollPos, logViewRegion);

            for (var index = 0; index < messages.Count; index++)
            {
                string message = messages[index];

                var lineRect = new Rect(0f, index * Text.LineHeight, logViewRegion.width, Text.LineHeight);

                if (!lineRect.IsVisible(logRegion, _logScrollPos))
                {
                    continue;
                }

                UiHelper.Label(lineRect, message);
            }

            GUI.EndScrollView();
            GUI.EndGroup();
        }

        private void DrawSearchResults(Rect region)
        {
            var searchView = new Rect(0f, 0f, region.width - 16f, _searchResultsHeight);
            _searchScrollPos = GUI.BeginScrollView(region, _searchScrollPos, searchView);

            var y = 0f;
            var alternate = false;
            foreach (SearchResult result in _searchResults)
            {
                float height = result.GetHeight(region.width);
                var searchRegion = new Rect(0f, y, region.width, height);
                alternate = !alternate;
                y += height;

                if (!searchRegion.IsVisible(region, _searchScrollPos))
                {
                    continue;
                }

                GUI.BeginGroup(searchRegion);
                result.Draw(searchRegion.AtZero());
                GUI.EndGroup();

                if (alternate)
                {
                    Widgets.DrawLightHighlight(searchRegion);
                }

                Widgets.DrawHighlightIfMouseover(searchRegion);
            }

            GUI.EndScrollView();
        }

        private void DrawFooter(Rect region)
        {
            Rect settingsBtnRegion = LayoutHelper.IconRect(region.width - Text.LineHeight, 0f, Text.LineHeight, Text.LineHeight);
            Rect reconnectBtnRegion = LayoutHelper.IconRect(settingsBtnRegion.x - Text.LineHeight + 2f, 0f, Text.LineHeight, Text.LineHeight, 4f);

            if (Widgets.ButtonImage(settingsBtnRegion, Textures.Gear))
            {
                Find.WindowStack.Add(new SettingsDialog());
            }

            if (Widgets.ButtonImage(reconnectBtnRegion, Textures.CurvedArrows))
            {
                Task.Run(async () => await BackendClient.Instance.ReconnectAsync());
            }

            TooltipHandler.TipRegion(settingsBtnRegion, _settingsTooltip);
            TooltipHandler.TipRegion(reconnectBtnRegion, _reconnectTooltip);
        }

        private void ProcessSearchRequest()
        {
            if (string.IsNullOrEmpty(_searchWidget.filter.Text))
            {
                _searchResults = null;

                return;
            }
        
            _searchResults = SearchIndex.Find(_searchWidget.filter.Text);
            _searchResultsHeight = 0f;
            
            foreach (SearchResult entry in _searchResults)
            {
                _searchResultsHeight += entry.GetHeight(windowRect.width - 36f);
            }
        }

        private static void DrawColoredPing(Rect region, Color color)
        {
            GUI.color = color;
            PingWorker.Draw(region);
            GUI.color = Color.white;
        }

        private void FetchTranslations()
        {
            _connectedLabel = "TExt.Labels.ConnectionStates.Connected".TranslateSimple();
            _connectedTooltip = "TExt.Tooltips.ConnectionStates.Connected".TranslateSimple();
            _connectingLabel = "TExt.Labels.ConnectionStates.Connecting".TranslateSimple();
            _connectingTooltip = "TExt.Tooltips.ConnectionStates.Connecting".TranslateSimple();
            _disconnectedLabel = "TExt.Labels.ConnectionStates.Disconnected".TranslateSimple();
            _disconnectedTooltip = "TExt.Tooltips.ConnectionStates.Disconnected".TranslateSimple();
            _disconnectingLabel = "TExt.Labels.ConnectionStates.Disconnecting".TranslateSimple();
            _disconnectingTooltip = "TExt.Tooltips.ConnectionStates.Disconnecting".TranslateSimple();
            _reconnectingLabel = "TExt.Labels.ConnectionStates.Reconnecting".TranslateSimple();
            _reconnectingTooltip = "TExt.Tooltips.ConnectionStates.Reconnecting".TranslateSimple();
            _subscribedLabel = "TExt.Labels.ConnectionStates.Subscribed".TranslateSimple();
            _subscribedTooltip = "TExt.Tooltips.ConnectionStates.Subscribed".TranslateSimple();
            _subscribingLabel = "TExt.Labels.ConnectionStates.Subscribing".TranslateSimple();
            _subscribingTooltip = "TExt.Tooltips.ConnectionStates.Subscribing".TranslateSimple();

            _reconnectTooltip = "TExt.Tooltips.Reconnect".TranslateSimple();
            _settingsTooltip = "TExt.Tooltips.Settings".TranslateSimple();
        }
    }
}
