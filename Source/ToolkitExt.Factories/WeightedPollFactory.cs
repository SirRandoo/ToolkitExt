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

using System.Collections.Generic;
using JetBrains.Annotations;
using ToolkitExt.Api.Interfaces;

namespace ToolkitExt.Factories
{
    /// <summary>
    ///     An abstract class for managing the weights of a given factory's
    ///     options.
    /// </summary>
    public abstract class WeightedPollFactory : IPollFactory
    {
        private readonly Dictionary<string, float> _weights = new Dictionary<string, float>();

        /// <inheritdoc/>
        public abstract IPoll Create();

        /// <summary>
        ///     Retrieves the current weight for a given option.
        /// </summary>
        /// <param name="identifier">The id of the option to get the weight for</param>
        /// <returns>The stored weight of the option</returns>
        protected float GetWeightFor([NotNull] string identifier)
        {
            if (_weights.TryGetValue(identifier, out float weight))
            {
                return weight;
            }

            _weights[identifier] = weight;

            return 1f;
        }

        /// <summary>
        ///     Saves a new weight for the given option.
        /// </summary>
        /// <param name="identifier">
        ///     The id of the option the weight should be
        ///     saved to
        /// </param>
        /// <param name="weight">The new weight the save for the given option</param>
        protected void SetWeightFor([NotNull] string identifier, float weight)
        {
            _weights[identifier] = weight;
        }
    }
}
