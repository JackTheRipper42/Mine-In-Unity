namespace Assets.Scripts.Blocks
{
    public class Air : Block
    {
        public Air(int id)
            : base(id, false)
        {
        }

        public override string GetUvName(int x, int y, int z, IWorld world, Side side)
        {
            return null;
        }

        public override bool IsTransparent(int x, int y, int z, IWorld world, Side side)
        {
            return true;
        }

        public override bool IsSolid(int x, int y, int z, IWorld world)
        {
            return false;
        }
    }
}