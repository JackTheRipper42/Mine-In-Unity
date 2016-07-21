using Assets.Scripts.Blocks;
using System;

namespace Assets.Scripts
{
    public abstract class Block
    {
        public static readonly Block[] Blocks = new Block[4096];

        public static readonly Air Air = new Air(0);
        public static readonly SolidBlock Dirt = new SolidBlock(1, "dirt");
        public static readonly Grass Grass = new Grass(2);
        public static readonly SolidBlock Sand = new SolidBlock(3, "sand");
        public static readonly SolidBlock Water = new SolidBlock(4, "water");

        private readonly int _id;
        private readonly bool _requiresRandomTickUpdate;

        protected Block(int id, bool requiresRandomTickUpdate)
        {
            _id = id;
            _requiresRandomTickUpdate = requiresRandomTickUpdate;
            if (Blocks[id] != null)
            {
                throw new ArgumentException(string.Format("The id {0} is already ised.", id), "id");
            }
            Blocks[id] = this;
        }

        public int Id
        {
            get { return _id; }
        }

        public bool RequiresRandomTickUpdate
        {
            get { return _requiresRandomTickUpdate; }
        }

        public abstract string GetUvName(int x, int y, int z, IWorld world, Side side);

        public abstract bool IsTransparent(int x, int y, int z, IWorld world, Side side);

        public abstract bool IsSolid(int x, int y, int z, IWorld world);

        public virtual void OnRandomTick(int x, int y, int z, IWorld world)
        {
        }
    }
}