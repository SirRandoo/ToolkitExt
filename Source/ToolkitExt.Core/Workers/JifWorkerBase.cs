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

using System.Threading;
using ToolkitExt.Api;
using UnityEngine;
using Verse;

namespace ToolkitExt.Core.Workers
{
    public abstract class JifWorkerBase
    {
        private readonly Texture[] _frames;

        public JifWorkerBase(params Texture[] frames)
        {
            _frames = frames;
        }
        public Timer Timer { get; set; }

        public Texture CurrentFrame => _frames[Index];

        public bool Running { get; private set; }
        
        public int TotalFrames => _frames.Length;
        
        public int Index { get; private set; }

        public void Start(int milliseconds)
        {
            ChangeTimer(milliseconds);
        }

        public void Stop()
        {
            ChangeTimer(Timeout.Infinite);
        }

        public void Draw(Rect region)
        {
            GUI.DrawTexture(region, CurrentFrame);
        }

        public void SetFrame(int frame)
        {
            Index = (frame - 1) % _frames.Length;

            ChangeTimer(Timeout.Infinite);
        }

        public void Advance()
        {
            Index = (Index + 1) % _frames.Length;
        }

        public void ToLastFrame()
        {
            SetFrame(_frames.Length);
        }

        public void ToFirstFrame()
        {
            SetFrame(1);
        }

        public void ChangeTimer(int period)
        {
            Running = Timer?.Change(0, period) == true && period != Timeout.Infinite;
        }
        
        public bool TryRestart(int period)
        {
            if (Running)
            {
                return false;
            }

            ChangeTimer(period);

            return Running;
        }
    }
}
