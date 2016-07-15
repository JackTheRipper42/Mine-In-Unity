namespace Assets.Scripts
{
    public abstract class Block
    {
        public static readonly Block[] Blocks = new Block[4096];

        public static readonly SimpleBlock Air = new SimpleBlock(0, true, "");
        public static readonly SimpleBlock Dirt = new SimpleBlock(1, false, "dirt");
        public static readonly SimpleBlock Grass = new SimpleBlock(2, false, "grass");
        public static readonly SimpleBlock Sand = new SimpleBlock(3, false, "sand");
        public static readonly SimpleBlock Water = new SimpleBlock(4, false, "water");

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

        public abstract bool IsTransparent(Side side);
    }
}
