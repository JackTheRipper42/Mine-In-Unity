namespace Assets.Scripts
{
    public class SimpleBlock : Block
    {
        private readonly string _uvName;

        public SimpleBlock(int id, string uvName) 
            : base(id)
        {
            _uvName = uvName;
        }

        public override string GetUvName(Side side)
        {
            return _uvName;
        }
    }
}
