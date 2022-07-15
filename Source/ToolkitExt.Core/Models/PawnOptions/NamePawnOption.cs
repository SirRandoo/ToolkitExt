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
using Newtonsoft.Json;
using ToolkitExt.Api.Enums;
using ToolkitExt.Api.Interfaces;
using Verse;

namespace ToolkitExt.Core.Models
{
    /// <summary>
    ///     Represents a name for a pawn.
    /// </summary>
    public class NamePawnOption : IPawnOption
    {
        /// <summary>
        ///     The first name of a given pawn.
        /// </summary>
        /// <remarks>
        ///     In the event a pawn only supports <see cref="NameSingle"/>,
        ///     <see cref="Nick"/> will be used instead.
        /// </remarks>
        [JsonProperty("first")]
        public List<IOptionRule> First { get; set; }

        /// <summary>
        ///     The nickname of a given pawn.
        /// </summary>
        /// <remarks>
        ///     In the event a pawn only supports <see cref="NameSingle"/>,
        ///     <see cref="Nick"/> will be used instead.
        /// </remarks>
        [JsonProperty("nick")]
        public List<IOptionRule> Nick { get; set; }

        /// <summary>
        ///     The last name of a given pawn.
        /// </summary>
        /// <remarks>
        ///     In the event a pawn only supports <see cref="NameSingle"/>,
        ///     <see cref="Nick"/> will be used instead.
        /// </remarks>
        [JsonProperty("last")]
        public List<IOptionRule> Last { get; set; }

        /// <inheritdoc/>
        [JsonProperty("type")]
        public OptionType Type => OptionType.Name;

        /// <inheritdoc/>
        [JsonProperty("requires_mods")]
        public IModRequirement[] RequiresMods { get; set; }
    }
}
