﻿using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class World : MonoBehaviour
    {
        public int Seed;
        public float ViewRange = 30;
        public GameObject ChunkPrefab;

        private List<Chunk> _chunks;

        protected virtual void Awake()
        {
            Cursor.visible = false;
            if (Seed == 0)
            {
                Seed = Random.Range(0, int.MaxValue);
            }
            _chunks = new List<Chunk>();
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

                    var chunk = FindChunk(position);
                    if (chunk == null)
                    {
                        var obj = Instantiate(ChunkPrefab);
                        obj.transform.position = position;
                        obj.transform.rotation = Quaternion.identity;
                        chunk = obj.GetComponent<Chunk>();
                        chunk.Initialize();
                        _chunks.Add(chunk);
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
    }
}