namespace Assets.Scripts
{
    public abstract class Item
    {
        public static readonly Item[] Items = new Item[1024];

        public static readonly SimpleItem Stick = new SimpleItem(1, "stick");

        private readonly int _id;

        protected Item(int id)
        {
            _id = id;
            Items[id] = this;
        }

        public int Id
        {
            get { return _id; }
        }

        public abstract string TextureName(int value);
    }
}
