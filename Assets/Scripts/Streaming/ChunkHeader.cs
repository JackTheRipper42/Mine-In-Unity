using System;

namespace Assets.Scripts.Streaming
{
    [Serializable]
    public struct ChunkHeader
    {
        public int X;
        public int Y;
        public int Z;
        public int DataSize;
    }
}
