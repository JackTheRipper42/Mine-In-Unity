namespace Assets.Scripts
{
    public class SimpleItem : Item
    {
        private readonly string _textureName;

        public SimpleItem(int id, string textureName) 
            : base(id)
        {
            _textureName = textureName;
        }

        public override string TextureName(int value)
        {
            return _textureName;
        }
    }
}
