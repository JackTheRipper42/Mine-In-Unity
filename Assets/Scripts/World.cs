using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public class World : MonoBehaviour
    {
        public int Seed;
        public float ViewRange = 30;
        public GameObject ChunkPrefab;

        private List<Chunk> _chunks;
        private Queue<Position2> _chunkCreationQueue; 

        protected virtual void Awake()
        {
            Cursor.visible = false;
            if (Seed == 0)
            {
                Seed = Random.Range(0, int.MaxValue);
            }
            _chunks = new List<Chunk>();
            _chunkCreationQueue = new Queue<Position2>();
            StartCoroutine(ChunkCreator());
        }

        protected virtual void Update()
        {
            for (var x = transform.position.x - ViewRange; x < transform.position.x + ViewRange; x += Chunk.Width)
            {
                for (var z = transform.position.z - ViewRange; z < transform.position.z + ViewRange; z += Chunk.Width)
                {
                    var chunkPosition = new Position2(
                        Mathf.FloorToInt(x/Chunk.Width)*Chunk.Width,
                        Mathf.FloorToInt(z/Chunk.Width)*Chunk.Width);
                    var chunk = FindChunk(new Vector3(chunkPosition.X, 0, chunkPosition.Z));
                    if (chunk == null && !_chunkCreationQueue.Contains(chunkPosition))
                    {
                        _chunkCreationQueue.Enqueue(chunkPosition);
                    }
                }
            }
        }

        public Chunk FindChunk(Vector3 position)
        {
            for (var a = 0; a < _chunks.Count; a++)
            {
                var chunkPosition = _chunks[a].transform.position;

                if ((position.x < chunkPosition.x) ||
                    (position.z < chunkPosition.z) ||
                    (position.x >= chunkPosition.x + Chunk.Width) ||
                    (position.z >= chunkPosition.z + Chunk.Width))
                {
                    continue;
                }
                return _chunks[a];
            }
            return null;
        }

        public void SetBlockId(int x, int y, int z, int id)
        {
            var position = new Vector3(x, y, z);
            var chunk = FindChunk(position) ?? CreateChunk(position);
            chunk.SetBlockIdGlobal(x, y, z, id);
        }

        public int GetBlockId(int x, int y, int z)
        {
            var position = new Vector3(x, y, z);
            var chunk = FindChunk(position);
            return chunk != null
                ? chunk.GetBlockIdGlobal(x, y, z)
                : WorldGenerator.GetTheoreticalId(new Vector3(x, y, z), this);
        }

        private Chunk CreateChunk(Vector3 position)
        {
            var obj = Instantiate(ChunkPrefab);
            obj.transform.position = position;
            obj.transform.rotation = Quaternion.identity;
            var chunk = obj.GetComponent<Chunk>();
            chunk.Initialize();
            _chunks.Add(chunk);
            return chunk;
        }

        private IEnumerator ChunkCreator()
        {
            while (isActiveAndEnabled)
            {
                if (_chunkCreationQueue.Any())
                {
                    var chunkPosition = _chunkCreationQueue.Dequeue();
                    CreateChunk(new Vector3(chunkPosition.X, 0, chunkPosition.Z));
                }
                yield return new WaitForSeconds(0.01f);
            }
        }
    }
}