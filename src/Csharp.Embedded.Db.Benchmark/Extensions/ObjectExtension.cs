using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ZeroFormatter;

namespace Csharp.Embedded.Db.Benchmark.Extensions
{
    internal static class ObjectExtension
    {
        private static object _locked = new object();

        public static byte[] Serialize<T>(this T obj)
        {
            return obj.Equals(default(T)) ? new byte[] { } : ZeroFormatterSerializer.Serialize(obj);
        }

        public static T Deserialize<T>(this byte[] content)
        {
            return content == null ? default : ZeroFormatterSerializer.Deserialize<T>(content);
        }

        public static byte[] ToByteArray(this object obj)
        {
            if (obj == null) return new byte[] { };

            var result = new byte[] { };

            lock (_locked)
            { 
                using (var stream = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, obj);

                    result = stream.ToArray();
                }
            }

            return result;
        }

        public static T ToObect<T>(this byte[] content)
        {
            if (content == null) return default;

            T result = default;

            lock (_locked)
            {
                using (var stream = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();

                    stream.Write(content, 0, content.Length);
                    stream.Seek(0, SeekOrigin.Begin);

                    result = (T)formatter.Deserialize(stream);
                }
            }

            return result;
        }
    }
}
