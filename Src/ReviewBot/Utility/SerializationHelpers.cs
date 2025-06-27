#region using

using System.IO;
using ProtoBuf;

#endregion

namespace ReviewBot.Utility
{
    public static class SerializationHelpers
    {
        public static MemoryStream ToMemoryStream(this object obj)
        {
            var ms = new MemoryStream();
            Serializer.Serialize(ms, obj);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        public static T Deserialize<T>(this Stream s)
        {
            return Serializer.Deserialize<T>(s);
        }
    }
}