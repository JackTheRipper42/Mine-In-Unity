namespace Assets.Scripts
{
    public class Map
    {
        private readonly int[] _map = new int[Chunk.Width * Chunk.Width * Chunk.Height];

        private static int GetIndex(int x, int y, int z)
        {
            return z + y*Chunk.Height + x*Chunk.Height*Chunk.Width;
        }

        public int this[int x, int y, int z]
        {
            get { return _map[GetIndex(x, y, z)]; }
            set { _map[GetIndex(x, y, z)] = value; }
        }

        public int[] InternalData
        {
            get { return _map; }
        }
    }
}
