using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Haukcode.UsefulDotNet
{
    public static partial class SerializeHelper
    {
        public static void SerializeToStream(Stream stream, object input)
        {
            using (var sw = new StreamWriter(stream))
            using (var tw = new JsonTextWriter(sw))
            {
                var serializer = new JsonSerializer();
                serializer.Converters.Add(new StringEnumConverter());
                serializer.Serialize(tw, input);
                tw.Flush();
            }
        }

        public static byte[] SerializeAndCompress(object input)
        {
            using (var ms = new MemoryStream())
            {
                using (var compressor = new DeflateStream(ms, CompressionLevel.Optimal, false))
                {
                    SerializeToStream(compressor, input);
                }

                return ms.ToArray();
            }
        }
    }
}
