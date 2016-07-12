using UnityEngine;

namespace Assets.Scripts
{
    public class World : MonoBehaviour
    {

        public static World CurrentWorld;

        public int ChunkWidth = 20;
        public int ChunkHeight = 20;
        public int Seed;
        public float ViewRange = 30;
        public Chunk ChunkFab;

        protected virtual void Awake()
        {
            Cursor.visible = false;
            CurrentWorld = this;
            if (Seed == 0)
            {
                Seed = Random.Range(0, int.MaxValue);
            }
        }

        protected virtual void Update()
        {
            for (var x = transform.position.x - ViewRange; x < transform.position.x + ViewRange; x += ChunkWidth)
            {
                for (var z = transform.position.z - ViewRange; z < transform.position.z + ViewRange; z += ChunkWidth)
                {

                    var pos = new Vector3(x, 0, z);
                    pos.x = Mathf.Floor(pos.x/ChunkWidth)*ChunkWidth;
                    pos.z = Mathf.Floor(pos.z/ChunkWidth)*ChunkWidth;

                    var chunk = Chunk.FindChunk(pos);
                    if (chunk == null)
                    {
                        Instantiate(ChunkFab, pos, Quaternion.identity);
                    }
                }
            }
        }
    }
}