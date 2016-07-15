namespace Assets.Scripts
{
    public abstract class Block
    {
        public static readonly Block[] Blocks = new Block[4096];

        public static readonly SimpleBlock Dirt = new SimpleBlock(1, "dirt");
        public static readonly SimpleBlock Grass = new SimpleBlock(2, "grass");
        public static readonly SimpleBlock Sand = new SimpleBlock(3, "sand");
        public static readonly SimpleBlock Water = new SimpleBlock(4, "water");

        private readonly int _id;

        protected Block(int id)
        {
            _id = id;
            Blocks[id] = this;
        }

        public int Id
        {
            get { return _id; }
        }

        public abstract string GetUvName(Side side);
    }
}
