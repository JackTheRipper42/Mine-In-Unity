namespace Assets.Scripts
{
    public interface IWorld
    {
        int GetBlockId(int x, int y, int z);
        void SetBlockId(int x, int y, int z, int id);
    }
}