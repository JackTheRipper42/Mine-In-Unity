using System;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Blocks
{
    public class Grass : Block
    {
        private const string DirtUvName = "dirt";
        private const string GrassUvName = "grass";
        private const string GrassSideUvName = "grass side";

        public Grass(int id)
            : base(id, true)
        {
        }

        public override string GetUvName(int x, int y, int z, IWorld world, Side side)
        {
            switch (side)
            {
                case Side.Up:
                    return GrassUvName;
                case Side.Down:
                    return DirtUvName;
                case Side.Right:
                    return IsRightDirty(x, y, z, world) ? GrassSideUvName : GrassUvName;
                case Side.Left:
                    return IsLeftDirty(x, y, z, world) ? GrassSideUvName : GrassUvName;
                case Side.Front:
                    return IsFrontDirty(x, y, z, world) ? GrassSideUvName : GrassUvName;
                case Side.Back:
                    return IsBackDirty(x, y, z, world) ? GrassSideUvName : GrassUvName;
                default:
                    throw new ArgumentOutOfRangeException("side", side, null);
            }
        }

        public override bool IsTransparent(int x, int y, int z, IWorld world, Side side)
        {
            return false;
        }

        public override void OnRandomTick(int x, int y, int z, IWorld world)
        {
            if (world.GetBlockId(x, y + 1, z) != Air.Id)
            {
                if (Random.Range(0, 10) > 6)
                {
                    world.SetBlockId(x, y, z, Dirt.Id);
                }
            }
        }

        private bool IsRightDirty(int x, int y, int z, IWorld world)
        {
            if (Blocks[world.GetBlockId(x + 1, y - 1, z)].Id != Id)
            {
                return true;
            }
            if (!Blocks[world.GetBlockId(x, y + 1, z)].IsTransparent(x, y + 1, z, world, Side.Down))
            {
                return true;
            }
            if (!Blocks[world.GetBlockId(x + 1, y, z)].IsTransparent(x + 1, y, z, world, Side.Left))
            {
                return true;
            }
            return false;
        }

        private bool IsLeftDirty(int x, int y, int z, IWorld world)
        {
            if (Blocks[world.GetBlockId(x - 1, y - 1, z)].Id != Id)
            {
                return true;
            }
            if (!Blocks[world.GetBlockId(x, y + 1, z)].IsTransparent(x, y + 1, z, world, Side.Down))
            {
                return true;
            }
            if (!Blocks[world.GetBlockId(x - 1, y, z)].IsTransparent(x - 1, y, z, world, Side.Right))
            {
                return true;
            }
            return false;
        }

        private bool IsFrontDirty(int x, int y, int z, IWorld world)
        {
            if (Blocks[world.GetBlockId(x, y - 1, z + 1)].Id != Id)
            {
                return true;
            }
            if (!Blocks[world.GetBlockId(x, y + 1, z)].IsTransparent(x, y + 1, z, world, Side.Down))
            {
                return true;
            }
            if (!Blocks[world.GetBlockId(x, y, z + 1)].IsTransparent(x, y, z + 1, world, Side.Back))
            {
                return true;
            }
            return false;
        }

        private bool IsBackDirty(int x, int y, int z, IWorld world)
        {
            if (Blocks[world.GetBlockId(x, y - 1, z - 1)].Id != Id)
            {
                return true;
            }
            if (!Blocks[world.GetBlockId(x, y + 1, z)].IsTransparent(x, y + 1, z, world, Side.Down))
            {
                return true;
            }
            if (!Blocks[world.GetBlockId(x, y, z - 1)].IsTransparent(x, y, z - 1, world, Side.Front))
            {
                return true;
            }
            return false;
        }
    }
}
