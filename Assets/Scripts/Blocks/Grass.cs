using System;

namespace Assets.Scripts.Blocks
{
    public class Grass : Block
    {
        public Grass(int id) : base(id)
        {
        }

        public override string GetUvName(int x, int y, int z, World world, Side side)
        {
            switch (side)
            {
                case Side.Up:
                    return "grass";
                case Side.Down:
                    return "dirt";
                case Side.Right:
                    return "grass side";
                case Side.Left:
                    return "grass side";
                case Side.Front:
                    return "grass side";
                case Side.Back:
                    return "grass side";
                default:
                    throw new ArgumentOutOfRangeException("side", side, null);
            }
        }

        public override bool IsTransparent(int x, int y, int z, World world, Side side)
        {
            return false;
        }
    }
}
