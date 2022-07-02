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

namespace ToolkitExt.Api.Enums
{
    /// <summary>
    ///     The various types of fields a pawn option can be.
    /// </summary>
    public enum FieldType
    {
        /// <summary>
        ///     Represents a <see cref="string"/>.
        /// </summary>
        [EnumMember(Value = "string")]
        String,
        
        /// <summary>
        ///     Represents a <see cref="int"/>.
        /// </summary>
        [EnumMember(Value = "integer")]
        Integer,
        
        /// <summary>
        ///     Represents a <see cref="float"/>.
        /// </summary>
        [EnumMember(Value = "float")]
        Float,
        
        /// <summary>
        ///     Represents a percentage.
        /// </summary>
        /// <remarks>
        ///     A percentage is a <see cref="float"/> between <c>0f</c> and <c>1f</c>
        /// </remarks>
        [EnumMember(Value = "percentage")]
        Percentage,
        
        /// <summary>
        ///     Represents a <see cref="bool"/>.
        /// </summary>
        [EnumMember(Value = "boolean")]
        Boolean,
        
        /// <summary>
        ///     Represents an <see cref="Enum"/>.
        /// </summary>
        [EnumMember(Value = "enum")]
        Enum
    }
}
