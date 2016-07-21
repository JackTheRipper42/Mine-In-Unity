namespace Assets.Scripts
{
    public class ChunkData
    {
        private readonly Position3 _position;
        private readonly Map _map;

        public ChunkData(Position3 position, Map map)
        {
            _position = position;
            _map = map;
        }


        public Position3 Position
        {
            get { return _position; }
        }

        public Map Map
        {
            get { return _map; }
        }
    }
}
