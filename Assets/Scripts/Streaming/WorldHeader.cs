using System;

namespace Assets.Scripts.Streaming
{
    [Serializable]
    public struct WorldHeader
    {
        public int Chunks;
        public int Seed;
    }
}
