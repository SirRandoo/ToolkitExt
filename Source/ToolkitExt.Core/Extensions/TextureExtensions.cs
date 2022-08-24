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

using JetBrains.Annotations;
using UnityEngine;

namespace ToolkitExt.Core.Extensions
{
    public static class TextureExtensions
    {
        [NotNull]
        public static Texture2D CopyFromReadOnly([NotNull] this Texture2D original)
        {
            RenderTexture tmp = RenderTexture.GetTemporary(original.width, original.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            Graphics.Blit(original, tmp);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = tmp;
            var tex = new Texture2D(original.width, original.height);
            tex.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            tex.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(tmp);

            return tex;
        }

        [NotNull]
        public static Texture2D GetFrameFromSheet([NotNull] this Texture2D sheet, int index)
        {
            var texture = new Texture2D(sheet.height, sheet.height);
            texture.SetPixels(sheet.GetPixels(index * texture.height, 0, texture.height, texture.height));
            texture.Apply();

            return texture;
        }

        [NotNull]
        public static Texture[] GetFramesFromSheet([NotNull] this Texture2D sheet)
        {
            int totalFrames = sheet.width / sheet.height;
            var container = new Texture[totalFrames];
            
            for (var i = 0; i < totalFrames; i++)
            {
                var texture = new Texture2D(sheet.height, sheet.height);
                texture.SetPixels(sheet.GetPixels(i * texture.height, 0, texture.height, texture.height));
                texture.Apply();

                container[i] = texture;
            }

            return container;
        }
    }
}
