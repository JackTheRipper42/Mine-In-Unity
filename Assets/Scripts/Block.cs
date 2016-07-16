using Assets.Scripts.Blocks;
using System;

namespace Assets.Scripts
{
    public abstract class Block
    {
        public static readonly Block[] Blocks = new Block[4096];

        public static readonly SimpleBlock Air = new SimpleBlock(0, true, "");
        public static readonly SimpleBlock Dirt = new SimpleBlock(1, false, "dirt");
        public static readonly Grass Grass = new Grass(2);
        public static readonly SimpleBlock Sand = new SimpleBlock(3, false, "sand");
        public static readonly SimpleBlock Water = new SimpleBlock(4, false, "water");

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

        public virtual void OnRandomTick(int x, int y, int z, IWorld world)
        {
        }
    }
}
