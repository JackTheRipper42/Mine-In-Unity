using UnityEngine;

namespace Assets.Scripts
{
    public class World : MonoBehaviour
    {

        public static World CurrentWorld;

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
            for (var x = transform.position.x - ViewRange; x < transform.position.x + ViewRange; x += Chunk.Width)
            {
                for (var z = transform.position.z - ViewRange; z < transform.position.z + ViewRange; z += Chunk.Width)
                {

                    var position = new Vector3(x, 0, z);
                    position.x = Mathf.Floor(position.x/Chunk.Width)*Chunk.Width;
                    position.z = Mathf.Floor(position.z/Chunk.Width)*Chunk.Width;

                    var chunk = Chunk.FindChunk(position);
                    if (chunk == null)
                    {
                        Instantiate(ChunkFab, position, Quaternion.identity);
                    }
                }
            }
        }
    }
}