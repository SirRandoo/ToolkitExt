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

using System;
using System.IO;
using JetBrains.Annotations;
using ToolkitExt.Api;
using ToolkitExt.Core;
using UnityEngine;
using Verse;

namespace ToolkitExt.Mod
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature, ImplicitUseTargetFlags.WithMembers)]
    public class ExtensionSettings : ModSettings
    {
        internal AuthSettings Auth;
        internal WindowSettings Window;

        /// <inheritdoc/>
        public override void ExposeData()
        {
            Scribe_Deep.Look(ref Window, "windowSettings");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                Window ??= WindowSettings.CreateDefault();
            }
        }

        public void LoadAuthSettings()
        {
            if (!File.Exists(FilePaths.AuthSettings))
            {
                return;
            }

            try
            {
                Json.Load<AuthSettings>(FilePaths.AuthSettings);
            }
            catch (Exception e)
            {
                // TODO: Log that the auth settings couldn't be loaded.
            }
        }

        public void SaveAuthSettings()
        {
            try
            {
                Json.Save(FilePaths.AuthSettings, Auth);
            }
            catch (Exception e)
            {
                // TODO: Log that the auth settings couldn't be saved.
            }
        }

        internal class WindowSettings : IExposable
        {
            internal Vector2 PollPosition;

            /// <inheritdoc/>
            public void ExposeData()
            {
                Scribe_Values.Look(ref PollPosition, "pollDisplayPosition");
            }

            [NotNull]
            public static WindowSettings CreateDefault() => new WindowSettings { PollPosition = new Vector2(Screen.width, Mathf.FloorToInt(Screen.height * 0.333f)) };
        }
    }
}
