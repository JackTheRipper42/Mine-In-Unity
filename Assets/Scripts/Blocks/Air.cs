namespace Assets.Scripts.Blocks
{
    public class Air : Block
    {
        public Air(int id)
            : base(id, false)
        {
        }

        public override bool IsSolid
        {
            get { return false; }
        }

        public override string GetUvName(int x, int y, int z, IWorld world, Side side)
        {
            return null;
        }

        public override bool IsTransparent(int x, int y, int z, IWorld world, Side side)
        {
            return true;
        }
    }
}