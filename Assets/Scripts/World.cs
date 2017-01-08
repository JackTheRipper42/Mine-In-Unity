using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.Streaming;
using UnityEngine;

namespace Assets.Scripts
{
    public class World : MonoBehaviour, IWorld
    {
        public int Seed;
        public float ViewRange = 30;
        public GameObject ChunkPrefab;

        private List<Chunk> _chunks;
        private Queue<ChunkData> _chunkCreationQueue;

        protected virtual void Awake()
        {
            Cursor.visible = false;
            if (Seed == 0)
            {
                Seed = Random.Range(0, int.MaxValue);
            }
            _chunks = new List<Chunk>();
            _chunkCreationQueue = new Queue<ChunkData>();
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
                    if (chunk == null && _chunkCreationQueue.All(data => data.Position != chunkPosition))
                    {
                        _chunkCreationQueue.Enqueue(new ChunkData(chunkPosition, CalculateMapFromScratch(chunkPosition)));
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
            var worldPosition = new Position3(x, y, z);
            var chunkPosition = new Position3(x, 0, z);
            var chunk = FindChunk(worldPosition) ?? CreateChunk(
                new ChunkData(
                    chunkPosition,
                    CalculateMapFromScratch(chunkPosition)));
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

        public void Save()
        {
            var headerFormatter = new WorldHeaderFormatter();
            var chunkFormatter = new ChunkFormatter();
            using (var stream = new FileStream("World", FileMode.Create, FileAccess.Write))
            {
                headerFormatter.Serialize(stream, new WorldHeader
                {
                    Seed = Seed,
                    Chunks = _chunks.Count
                });
                foreach (var chunk in _chunks)
                {
                    chunkFormatter.Serialize(stream, chunk.Data);
                }
            }
        }

        public void Load()
        {
            foreach (var chunk in _chunks)
            {
                Destroy(chunk.gameObject);
            }
            _chunkCreationQueue.Clear();
            _chunks.Clear();

            var headerFormatter = new WorldHeaderFormatter();
            var chunkFormatter = new ChunkFormatter();
            using (var stream = new FileStream("World", FileMode.Open, FileAccess.Read))
            {
                var header = headerFormatter.Deserialize(stream);
                Seed = header.Seed;
                for (var i = 0; i < header.Chunks; i++)
                {
                    var chunkData = chunkFormatter.Deserialize(stream);
                    _chunkCreationQueue.Enqueue(chunkData);
                }
            }
        }

        private Chunk CreateChunk(ChunkData chunkData)
        {
            var obj = Instantiate(ChunkPrefab);
            var chunk = obj.GetComponent<Chunk>();
            chunk.Initialize(chunkData);
            _chunks.Add(chunk);
            return chunk;
        }

        private Map CalculateMapFromScratch(Position3 position)
        {
            Random.InitState(Seed);
            var map = new Map();

            for (var x = 0; x < Chunk.Width; x++)
            {
                for (var y = 0; y < Chunk.Height; y++)
                {
                    for (var z = 0; z < Chunk.Width; z++)
                    {
                        var id = WorldGenerator.GetTheoreticalId(
                            new Position3(x, y, z) + position,
                            this);
                        map[x, y, z] = id;
                    }
                }
            }
            return map;
        }

        private IEnumerator ChunkCreator()
        {
            while (isActiveAndEnabled)
            {
                if (_chunkCreationQueue.Any())
                {
                    var chunkData = _chunkCreationQueue.Dequeue();
                    CreateChunk(chunkData);
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