﻿// MIT License
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

using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace ToolkitExt.Mod
{
    [StaticConstructorOnStartup]
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public static class Textures
    {
        public static readonly Texture2D Gear = ContentFinder<Texture2D>.Get("UI/Gear");
        public static readonly Texture2D Hidden = ContentFinder<Texture2D>.Get("UI/Hidden");
        public static readonly Texture2D Visible = ContentFinder<Texture2D>.Get("UI/Visible");
        public static readonly Texture2D Toolbox = ContentFinder<Texture2D>.Get("UI/Polls/Toolbox");
        public static readonly Texture2D WindowAtlas = ContentFinder<Texture2D>.Get("UI/Polls/Atlas");
        public static readonly Texture2D CurvedArrows = ContentFinder<Texture2D>.Get("UI/CurvedArrows");
        public static readonly Texture2D GradientOverlay = ContentFinder<Texture2D>.Get("UI/Polls/Gradient");
        public static readonly Texture2D ProgressLeftAtlas = ContentFinder<Texture2D>.Get("UI/Polls/ProgressLeft");
        public static readonly Texture2D ProgressRightAtlas = ContentFinder<Texture2D>.Get("UI/Polls/ProgressRight");
    }
}
