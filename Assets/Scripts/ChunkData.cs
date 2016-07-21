namespace Assets.Scripts
{
    public class ChunkData
    {
        private readonly Position3 _position;
        private readonly int[] _map;

        public ChunkData(Position3 position, int[] map)
        {
            _position = position;
            _map = map;
        }


        public Position3 Position
        {
            get { return _position; }
        }

        public int[] Map
        {
            get { return _map; }
        }
    }
}
