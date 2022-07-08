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

using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace ToolkitExt.Api
{
    public static class Json
    {
        private static readonly JsonSerializer Serializer = JsonSerializer.CreateDefault();

        [CanBeNull]
        public static T Load<T>(string filePath) where T : new()
        {
            if (!File.Exists(filePath))
            {
                return new T();
            }

            using (FileStream file = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(file))
                {
                    using (var jsonReader = new JsonTextReader(reader))
                    {
                        return Serializer.Deserialize<T>(jsonReader);
                    }
                }
            }
        }

        public static void Save<T>([NotNull] string filePath, [NotNull] T obj)
        {
            DirectoryInfo directory = Directory.GetParent(filePath);

            if (directory is { Exists: false })
            {
                directory.Create();
            }

            using (FileStream file = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (var writer = new StreamWriter(file))
                {
                    using (var jsonWriter = new JsonTextWriter(writer))
                    {
                        Serializer.Serialize(jsonWriter, obj);
                    }
                }
            }
        }

        [CanBeNull]
        public static async Task<T> LoadAsync<T>(string filePath) where T : new()
        {
            if (!File.Exists(filePath))
            {
                return new T();
            }

            using (FileStream file = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new StreamReader(file))
                {
                    using (var jsonReader = new JsonTextReader(reader))
                    {
                        return await Serializer.DeserializeAsync<T>(jsonReader);
                    }
                }
            }
        }

        public static async Task SaveAsync<T>([NotNull] string filePath, [NotNull] T obj)
        {
            DirectoryInfo directory = Directory.GetParent(filePath);

            if (directory is { Exists: false })
            {
                directory.Create();
            }

            using (FileStream file = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (var writer = new StreamWriter(file))
                {
                    using (var jsonWriter = new JsonTextWriter(writer))
                    {
                        await Serializer.SerializeAsync(jsonWriter, obj);
                    }
                }
            }
        }

        [CanBeNull]
        public static T Deserialize<T>([NotNull] Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                using (var jsonReader = new JsonTextReader(reader))
                {
                    return Serializer.Deserialize<T>(jsonReader);
                }
            }
        }

        [CanBeNull]
        public static T Deserialize<T>([NotNull] string content)
        {
            using (var reader = new StringReader(content))
            {
                using (var jsonReader = new JsonTextReader(reader))
                {
                    return Serializer.Deserialize<T>(jsonReader);
                }
            }
        }

        public static void Serialize<T>([NotNull] Stream stream, [NotNull] T obj)
        {
            using (var writer = new StreamWriter(stream))
            {
                using (var jsonWriter = new JsonTextWriter(writer))
                {
                    Serializer.Serialize(jsonWriter, obj);
                }
            }
        }

        [NotNull]
        public static string Serialize<T>([NotNull] T obj)
        {
            using (var writer = new StringWriter())
            {
                using (var jsonWriter = new JsonTextWriter(writer))
                {
                    Serializer.Serialize(jsonWriter, obj);
                }

                return writer.ToString();
            }
        }

        [CanBeNull]
        public static async Task<T> DeserializeAsync<T>([NotNull] Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                using (var jsonReader = new JsonTextReader(reader))
                {
                    return await Serializer.DeserializeAsync<T>(jsonReader);
                }
            }
        }

        [ItemCanBeNull]
        public static async Task<T> DeserializeAsync<T>([NotNull] string content)
        {
            using (var reader = new StringReader(content))
            {
                using (var jsonReader = new JsonTextReader(reader))
                {
                    return await Serializer.DeserializeAsync<T>(jsonReader);
                }
            }
        }

        public static async Task SerializeAsync<T>([NotNull] Stream stream, [NotNull] T obj)
        {
            using (var writer = new StreamWriter(stream))
            {
                using (var jsonWriter = new JsonTextWriter(writer))
                {
                    await Serializer.SerializeAsync(jsonWriter, obj);
                }
            }
        }

        [ItemNotNull]
        public static async Task<string> SerializeAsync<T>([NotNull] T obj)
        {
            using (var writer = new StringWriter())
            {
                using (var jsonWriter = new JsonTextWriter(writer))
                {
                    await Serializer.SerializeAsync(jsonWriter, obj);
                }

                return writer.ToString();
            }
        }

        [ContractAnnotation("=> true, result:notnull; => false, result:null")]
        public static bool TrySerialize<T>(T obj, out string result)
        {
            try
            {
                result = Serialize(obj);

                return true;
            }
            catch (JsonException)
            {
                result = default;
            }

            return false;
        }

        [ContractAnnotation("=> true, result:notnull; => false, result:null")]
        public static bool TryDeserialize<T>([NotNull] string content, out T result)
        {
            try
            {
                result = Deserialize<T>(content);

                return true;
            }
            catch (JsonException)
            {
                result = default;
            }

            return false;
        }
    }
}
