using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Assets.Scripts.Streaming
{
    public abstract class HeaderFormatter
    {
        protected string ReadString(Stream stream)
        {
            var data = new List<byte>();
            var end = false;
            while (!end)
            {
                var rawData = stream.ReadByte();
                if (rawData > 0)
                {
                    data.Add((byte) rawData);
                }
                else
                {
                    end = true;
                }
            }

            return Encoding.ASCII.GetString(data.ToArray());
        }

        protected void WriteString(Stream stream, string value)
        {
            var buffer = Encoding.ASCII.GetBytes(value);
            stream.Write(buffer, 0, buffer.Length);
            stream.WriteByte(0);
        }
    }
}
