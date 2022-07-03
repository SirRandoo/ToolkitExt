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
using System.IO;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace ToolkitExt.Core.Serialization
{
    /// <summary>
    ///     A converter for deserializing JSON responses with embedded JSON.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the data into</typeparam>
    /// <remarks>
    ///     From https://stackoverflow.com/a/39154630
    /// </remarks>
    public class EmbeddedJsonConverter<T> : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override object ReadJson([NotNull] JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            JsonContract contract = serializer.ContractResolver.ResolveContract(objectType);

            if (contract is JsonPrimitiveContract)
            {
                throw new JsonSerializationException("Invalid type: " + objectType);
            }

            existingValue ??= contract.DefaultCreator();

            if (reader.TokenType == JsonToken.String)
            {
                var json = (string)JToken.Load(reader);

                using (var subReader = new JsonTextReader(new StringReader(json)))
                {
                    serializer.Populate(subReader, existingValue);
                }
            }
            else
            {
                serializer.Populate(reader, existingValue);
            }

            return existingValue;
        }

        /// <inheritdoc/>
        public override bool CanConvert(Type objectType) => typeof(T).IsAssignableFrom(objectType);
    }
}
