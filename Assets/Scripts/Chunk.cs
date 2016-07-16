using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    [RequireComponent(typeof(MeshFilter))]
    public class Chunk : MonoBehaviour
    {
        public const int Width = 20;
        public const int Height = 20;

        public static List<Chunk> Chunks = new List<Chunk>();

        private ResourceManager _resourceManager;

        private int[,,] _map;
        private Mesh _visualMesh;
        private MeshCollider _meshCollider;
        private MeshFilter _meshFilter;

        protected virtual void Start()
        {
            Chunks.Add(this);

            _meshCollider = GetComponent<MeshCollider>();
            _meshFilter = GetComponent<MeshFilter>();
            var meshRenderer = GetComponent<MeshRenderer>();

            _resourceManager = FindObjectOfType<ResourceManager>();
            meshRenderer.material = _resourceManager.ChunkMaterial;

            CalculateMapFromScratch();
            StartCoroutine(CreateVisualMesh());
        }

        private static int GetTheoreticalId(Vector3 position)
        {
            Random.seed = World.CurrentWorld.Seed;

            var grain0Offset = new Vector3(Random.value*10000, Random.value*10000, Random.value*10000);
            var grain1Offset = new Vector3(Random.value*10000, Random.value*10000, Random.value*10000);

            return GetTheoreticalId(position, grain0Offset, grain1Offset);
        }

        private static int GetTheoreticalId(Vector3 position, Vector3 offset0, Vector3 offset1)
        {
            const float heightBase = 10f;
            var maxHeight = Height - 10f;
            var heightSwing = maxHeight - heightBase;

            int id = Block.Grass.Id;

            var clusterValue = CalculateNoiseValue(position, offset1, 0.02f);
            var blobValue = CalculateNoiseValue(position, offset1, 0.05f);
            var mountainValue = CalculateNoiseValue(position, offset0, 0.009f);
            if ((Math.Abs(mountainValue) < 0.001f) && (blobValue < 0.2f))
            {
                id = Block.Dirt.Id;
            }
            else if (clusterValue > 0.9f)
            {
                id = Block.Grass.Id;
            }
            else if (clusterValue > 0.8f)
            {
                id = Block.Sand.Id;
            }

            mountainValue = Mathf.Sqrt(mountainValue);
            mountainValue *= heightSwing;
            mountainValue += heightBase;
            mountainValue += (blobValue*10) - 5f;

            return mountainValue >= position.y ? id : 0;
        }

        private void CalculateMapFromScratch()
        {
            _map = new int[Width, Height, Width];

            Random.seed = World.CurrentWorld.Seed;

            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    for (var z = 0; z < Width; z++)
                    {
                        _map[x, y, z] = GetTheoreticalId(new Vector3(x, y, z) + transform.position);
                    }
                }
            }
        }

        private static float CalculateNoiseValue(Vector3 pos, Vector3 offset, float scale)
        {
            var noiseX = Mathf.Abs((pos.x + offset.x)*scale);
            var noiseY = Mathf.Abs((pos.y + offset.y)*scale);
            var noiseZ = Mathf.Abs((pos.z + offset.z)*scale);

            return Mathf.Max(0, Noise.Generate(noiseX, noiseY, noiseZ));
        }
        
        private IEnumerator CreateVisualMesh()
        {
            _visualMesh = new Mesh();
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    for (var z = 0; z < Width; z++)
                    {
                        if (_map[x, y, z] == 0)
                        {
                            continue;
                        }

                        var id = _map[x, y, z];

                        // Left wall
                        if (IsTransparent(x - 1, y, z))
                        {
                            BuildFace(
                                id, 
                                new Vector3(x, y, z), 
                                Vector3.up, 
                                Vector3.forward, 
                                false, 
                                vertices, 
                                uvs,
                                triangles);
                        }

                        // Right wall
                        if (IsTransparent(x + 1, y, z))
                        {
                            BuildFace(
                                id, 
                                new Vector3(x + 1, y, z), 
                                Vector3.up, 
                                Vector3.forward, 
                                true, 
                                vertices, 
                                uvs,
                                triangles);
                        }

                        // Bottom wall
                        if (IsTransparent(x, y - 1, z))
                        {
                            BuildFace(
                                id, 
                                new Vector3(x, y, z), 
                                Vector3.forward, 
                                Vector3.right, 
                                false, 
                                vertices, 
                                uvs,
                                triangles);
                        }

                        // Top wall
                        if (IsTransparent(x, y + 1, z))
                        {
                            BuildFace(
                                id,
                                new Vector3(x, y + 1, z),
                                Vector3.forward,
                                Vector3.right,
                                true,
                                vertices,
                                uvs,
                                triangles);
                        }

                        // Back
                        if (IsTransparent(x, y, z - 1))
                        {
                            BuildFace(
                                id, 
                                new Vector3(x, y, z), 
                                Vector3.up, 
                                Vector3.right, 
                                true, 
                                vertices, 
                                uvs, 
                                triangles);
                        }

                        // Front
                        if (IsTransparent(x, y, z + 1))
                        {
                            BuildFace(
                                id, 
                                new Vector3(x, y, z + 1), 
                                Vector3.up, 
                                Vector3.right, 
                                false, 
                                vertices, 
                                uvs,
                                triangles);
                        }
                    }
                }
            }

            _visualMesh.vertices = vertices.ToArray();
            _visualMesh.uv = uvs.ToArray();
            _visualMesh.triangles = triangles.ToArray();
            _visualMesh.RecalculateBounds();
            _visualMesh.RecalculateNormals();

            _meshFilter.mesh = _visualMesh;

            _meshCollider.sharedMesh = null;
            _meshCollider.sharedMesh = _visualMesh;

            yield return 0;
        }

        private void BuildFace(
            int id, 
            Vector3 corner, 
            Vector3 up, 
            Vector3 right, 
            bool reversed,
            ICollection<Vector3> vertices, 
            ICollection<Vector2> uvs, 
            ICollection<int> triangles)
        {
            var index = vertices.Count;

            vertices.Add(corner);
            vertices.Add(corner + up);
            vertices.Add(corner + up + right);
            vertices.Add(corner + right);

            var uvWidth = _resourceManager.BlockUvSize;
            var uvCorner = _resourceManager.BlockUvPositions[Block.Blocks[id].GetUvName(Side.Up)];

            uvs.Add(uvCorner);
            uvs.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
            uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
            uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));

            if (reversed)
            {
                triangles.Add(index + 0);
                triangles.Add(index + 1);
                triangles.Add(index + 2);
                triangles.Add(index + 2);
                triangles.Add(index + 3);
                triangles.Add(index + 0);
            }
            else
            {
                triangles.Add(index + 1);
                triangles.Add(index + 0);
                triangles.Add(index + 2);
                triangles.Add(index + 3);
                triangles.Add(index + 2);
                triangles.Add(index + 0);
            }

        }

        private bool IsTransparent(int x, int y, int z)
        {
            if (y < 0)
            {
                return false;
            }
            var id = GetId(x, y, z);
            return Block.Blocks[id].IsTransparent(Side.Up);
        }

        public int GetId(int x, int y, int z)
        {

            if ((y < 0) || (y >= Height))
            {
                return 0;
            }

            if ((x < 0) || (z < 0) || (x >= Width) || (z >= Width))
            {
                var worldPosition = new Vector3(x, y, z) + transform.position;
                var chunk = FindChunk(worldPosition);
                if (chunk == this)
                {
                    return 0;
                }
                if (chunk == null)
                {
                    return GetTheoreticalId(worldPosition);
                }
                return chunk.GetId(worldPosition);
            }
            return _map[x, y, z];
        }

        public int GetId(Vector3 worldPosition)
        {
            worldPosition -= transform.position;
            var x = Mathf.FloorToInt(worldPosition.x);
            var y = Mathf.FloorToInt(worldPosition.y);
            var z = Mathf.FloorToInt(worldPosition.z);
            return GetId(x, y, z);
        }

        public static Chunk FindChunk(Vector3 position)
        {

            for (var a = 0; a < Chunks.Count; a++)
            {
                var chunkPosition = Chunks[a].transform.position;

                if ((position.x < chunkPosition.x) ||
                    (position.z < chunkPosition.z) ||
                    (position.x >= chunkPosition.x + Width) ||
                    (position.z >= chunkPosition.z + Width))
                {
                    continue;
                }
                return Chunks[a];

            }
            return null;
        }

        public bool SetId(int id, Vector3 worldPosition)
        {
            worldPosition -= transform.position;
            return SetId(
                id,
                Mathf.FloorToInt(worldPosition.x),
                Mathf.FloorToInt(worldPosition.y),
                Mathf.FloorToInt(worldPosition.z));
        }

        public bool SetId(int id, int x, int y, int z)
        {
            if ((x < 0) || (y < 0) || (z < 0) || (x >= Width) || (y >= Height || (z >= Width)))
            {
                return false;
            }
            if (_map[x, y, z] == id)
            {
                return false;
            }
            _map[x, y, z] = id;
            StartCoroutine(CreateVisualMesh());

            if (x == 0)
            {
                var chunk = FindChunk(new Vector3(x - 2, y, z) + transform.position);
                if (chunk != null)
                {
                    StartCoroutine(chunk.CreateVisualMesh());
                }
            }
            if (x == Width - 1)
            {
                var chunk = FindChunk(new Vector3(x + 2, y, z) + transform.position);
                if (chunk != null)
                {
                    StartCoroutine(chunk.CreateVisualMesh());
                }
            }
            if (z == 0)
            {
                var chunk = FindChunk(new Vector3(x, y, z - 2) + transform.position);
                if (chunk != null)
                {
                    StartCoroutine(chunk.CreateVisualMesh());
                }
            }
            if (z == Width - 1)
            {
                var chunk = FindChunk(new Vector3(x, y, z + 2) + transform.position);
                if (chunk != null)
                {
                    StartCoroutine(chunk.CreateVisualMesh());
                }
            }
            return true;
        }
    }
}