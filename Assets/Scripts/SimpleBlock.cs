namespace Assets.Scripts
{
    public class SimpleBlock : Block
    {
        private readonly string _uvName;
        private readonly bool _transparent;

        public SimpleBlock(int id, bool transparent, string uvName) 
            : base(id)
        {
            _uvName = uvName;
            _transparent = transparent;
        }

        public override string GetUvName(Side side)
        {
            return _uvName;
        }

        public override bool IsTransparent(Side side)
        {
            return _transparent;
        }
    }
}
