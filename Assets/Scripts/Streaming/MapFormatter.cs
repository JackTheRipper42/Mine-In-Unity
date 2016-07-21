using System;
using System.IO;

namespace Assets.Scripts.Streaming
{
    public class MapFormatter
    {
        public void Serialize(Stream stream, Map map)
        {
            var data = map.InternalData;
            if (data.Length == 0)
            {
                return;
            }

            var lastValue = data[0];
            var count = 1;
            for (var index = 1; index < data.Length; index++)
            {
                var value = data[index];
                if (lastValue == value)
                {
                    count++;
                }
                else
                {
                    Save(stream, lastValue, count);
                    lastValue = value;
                    count = 1;
                }
            }
            Save(stream, lastValue, count);
        }

        public void Deserialize(byte[] stream, Map map)
        {
            var data = map.InternalData;
            var streamIndex = 0;
            var dataIndex = 0;
            do
            {
                var value = BitConverter.ToInt32(stream, streamIndex);
                streamIndex += sizeof (int);
                var count = BitConverter.ToInt32(stream, streamIndex);
                streamIndex += sizeof (int);

                for (var i = 0; i < count; i++)
                {
                    data[dataIndex++] = value;
                }

            } while (streamIndex < stream.Length);
        }

        private static void Save(Stream stream, int value, int count)
        {
            var valueBytes = BitConverter.GetBytes(value);
            stream.Write(valueBytes, 0, valueBytes.Length);
            var countBytes = BitConverter.GetBytes(count);
            stream.Write(countBytes, 0, countBytes.Length);
        }
    }
}
