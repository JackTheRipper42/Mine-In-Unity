using System.IO;
using UnityEngine;

namespace Assets.Scripts.Streaming
{
    public class WorldHeaderFormatter : HeaderFormatter
    {
        public void Serialize(Stream stream, WorldHeader header)
        {
            var headerString = JsonUtility.ToJson(header, false);
            WriteString(stream, headerString);
        }

        public WorldHeader Deserialize(Stream stream)
        {
            var headerString = ReadString(stream);
            var header = JsonUtility.FromJson<WorldHeader>(headerString);
            return header;
        }
    }
}
