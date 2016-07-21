using System.IO;

namespace Assets.Scripts.Streaming
{
    public class ChunkFormatter
    {
        private readonly MapFormatter _mapFormatter = new MapFormatter();
        private readonly ChunkHeaderFormatter _headerFormatter = new ChunkHeaderFormatter();

        public void Serialize(Stream stream, ChunkData chunkData)
        {
            using (var mapStream = new MemoryStream())
            {
                _mapFormatter.Serialize(mapStream, chunkData.Map);
                var header = new ChunkHeader
                {
                    DataSize = (int) mapStream.Position,
                    X = chunkData.Position.X,
                    Y = chunkData.Position.Y,
                    Z = chunkData.Position.Z
                };
                _headerFormatter.Serialize(stream, header);
                var buffer = mapStream.GetBuffer();
                stream.Write(buffer, 0, header.DataSize);
            }
        }

        public ChunkData Deserialize(Stream stream)
        {
            var header = _headerFormatter.Deserialize(stream);
            var map = new Map();
            var buffer = new byte[header.DataSize];
            stream.Read(buffer, 0, buffer.Length);
            _mapFormatter.Deserialize(buffer, map);
            var chunkData = new ChunkData(
                new Position3(header.X, header.Y, header.Z), 
                map);
            return chunkData;
        }
    }
}
