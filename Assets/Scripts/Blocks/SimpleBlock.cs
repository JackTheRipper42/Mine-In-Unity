namespace Assets.Scripts.Blocks
{
    public class SimpleBlock : Block
    {
        private readonly string _uvName;
        private readonly bool _transparent;

        public SimpleBlock(int id, bool transparent, string uvName) 
            : base(id, false)
        {
            _uvName = uvName;
            _transparent = transparent;
        }

        public override string GetUvName(int x, int y, int z, IWorld world, Side side)
        {
            return _uvName;
        }

        public override bool IsTransparent(int x, int y, int z, IWorld world, Side side)
        {
            return _transparent;
        }
    }
}
