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

using System.Runtime.Serialization;
using RimWorld;

namespace ToolkitExt.Api.Enums
{
    /// <summary>
    ///     The different types of options available for customization to
    ///     users.
    /// </summary>
    public enum OptionType
    {
        /// <summary>
        ///     Represents a <see cref="SkillDef"/> for a given pawn.
        /// </summary>
        [EnumMember(Value = "skill")]
        Skill,

        /// <summary>
        ///     Represents a <see cref="RimWorld.Backstory"/> for a given pawn.
        /// </summary>
        [EnumMember(Value = "backstory")]
        Backstory,

        /// <summary>
        ///     Represents a <see cref="TraitDef"/> for a given pawn.
        /// </summary>
        [EnumMember(Value = "trait")]
        Trait,

        /// <summary>
        ///     Represents a name for a given pawn.
        /// </summary>
        [EnumMember(Value = "name")]
        Name,

        /// <summary>
        ///     Represents a favorite color for a given pawn.
        /// </summary>
        [EnumMember(Value = "favorite_color")]
        FavoriteColor,

        /// <summary>
        ///     Represents the skin color for a given pawn.
        /// </summary>
        [EnumMember(Value = "melanin")]
        Melanin,

        /// <summary>
        ///     Represents the sexuality of a given pawn.
        /// </summary>
        [EnumMember(Value = "sexuality")]
        Sexuality,

        /// <summary>
        ///     Represents the <see cref="Gender"/> of a given pawn.
        /// </summary>
        [EnumMember(Value = "gender")]
        Gender
    }
}
