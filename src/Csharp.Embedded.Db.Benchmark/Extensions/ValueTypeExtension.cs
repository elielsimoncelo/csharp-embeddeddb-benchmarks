using System;
namespace Csharp.Embedded.Db.Benchmark.Extensions
{
    internal static class ValueTypeExtension
    {
        public static byte[] ToBinary(this long type)
        {
            var result = BitConverter.GetBytes(type);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(result);

            return result;
        }

        public static long ToLong(this byte[] content)
        {
            return content == null ? default : BitConverter.ToInt64(content, 0);
        }
    }
}
