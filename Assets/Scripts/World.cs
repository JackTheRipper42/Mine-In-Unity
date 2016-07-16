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
        private Queue<Position3> _chunkCreationQueue; 

        protected virtual void Awake()
        {
            Cursor.visible = false;
            if (Seed == 0)
            {
                Seed = Random.Range(0, int.MaxValue);
            }
            _chunks = new List<Chunk>();
            _chunkCreationQueue = new Queue<Position3>();
            StartCoroutine(ChunkCreator());
            StartCoroutine(OnTickUpdater());
        }

        protected virtual void Update()
        {
            for (var x = transform.position.x - ViewRange; x < transform.position.x + ViewRange; x += Chunk.Width)
            {
                for (var z = transform.position.z - ViewRange; z < transform.position.z + ViewRange; z += Chunk.Width)
                {
                    var chunkPosition = new Position3(
                        Mathf.FloorToInt(x/Chunk.Width)*Chunk.Width,
                        0,
                        Mathf.FloorToInt(z/Chunk.Width)*Chunk.Width);
                    var chunk = FindChunk(chunkPosition);
                    if (chunk == null && !_chunkCreationQueue.Contains(chunkPosition))
                    {
                        _chunkCreationQueue.Enqueue(chunkPosition);
                    }
                }
            }
        }

        public Chunk FindChunk(Position3 position)
        {
            for (var a = 0; a < _chunks.Count; a++)
            {
                var chunkPosition = _chunks[a].transform.position;

                if ((position.X < chunkPosition.x) ||
                    (position.Z < chunkPosition.z) ||
                    (position.X >= chunkPosition.x + Chunk.Width) ||
                    (position.Z >= chunkPosition.z + Chunk.Width))
                {
                    continue;
                }
                return _chunks[a];
            }
            return null;
        }

        public void SetBlockId(int x, int y, int z, int id)
        {
            var position = new Position3(x, y, z);
            var chunk = FindChunk(position) ?? CreateChunk(new Position3(x, 0, z));
            chunk.SetBlockIdGlobal(x, y, z, id);
        }

        public int GetBlockId(int x, int y, int z)
        {
            var position = new Position3(x, y, z);
            var chunk = FindChunk(position);
            return chunk != null
                ? chunk.GetBlockIdGlobal(x, y, z)
                : WorldGenerator.GetTheoreticalId(position, this);
        }

        private Chunk CreateChunk(Position3 position)
        {
            var obj = Instantiate(ChunkPrefab);
            var chunk = obj.GetComponent<Chunk>();
            chunk.Initialize(position);
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
                    CreateChunk(chunkPosition);
                }
                yield return new WaitForSeconds(0.01f);
            }
        }

        private IEnumerator OnTickUpdater()
        {
            while (isActiveAndEnabled)
            {
                var freeBlocks = 65000;
                foreach (var chunk in _chunks.OrderBy(item => Random.value))
                {
                    if (Random.Range(0, 10) > 6)
                    {
                        freeBlocks -= chunk.TickUpdateBlocks;
                        if (freeBlocks >= 0)
                        {
                            chunk.TickUpdate();
                        }
                    }
                    yield return new WaitForEndOfFrame();
                }
                yield return new WaitForSeconds(0.02f);
            }
        }
    }
}