using System.IO;
using UnityEngine;

namespace Assets.Scripts.Streaming
{
    public class ChunkHeaderFormatter : HeaderFormatter
    {
        public void Serialize(Stream stream, ChunkHeader header)
        {
            var headerString = JsonUtility.ToJson(header, false);
            WriteString(stream, headerString);
        }

        public ChunkHeader Deserialize(Stream stream)
        {
            var headerString = ReadString(stream);
            var header = JsonUtility.FromJson<ChunkHeader>(headerString);
            return header;
        }
    }
}
