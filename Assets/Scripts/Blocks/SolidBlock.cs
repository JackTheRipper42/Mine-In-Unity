namespace Assets.Scripts.Blocks
{
    public class SolidBlock : Block
    {
        private readonly string _uvName;

        public SolidBlock(int id, string uvName)
            : base(id, false)
        {
            _uvName = uvName;
        }

        public override bool IsSolid
        {
            get { return true; }
        }

        public override string GetUvName(int x, int y, int z, IWorld world, Side side)
        {
            return _uvName;
        }

        public override bool IsTransparent(int x, int y, int z, IWorld world, Side side)
        {
            return false;
        }
    }
}
