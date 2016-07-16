using System;
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

        private ResourceManager _resourceManager;

        private int[,,] _map;
        private Mesh _visualMesh;
        private MeshCollider _meshCollider;
        private MeshFilter _meshFilter;
        private World _world;

        public bool IsDirty { get; private set; }

        public void Initialize()
        {
            _world = FindObjectOfType<World>();

            _meshCollider = GetComponent<MeshCollider>();
            _meshFilter = GetComponent<MeshFilter>();
            var meshRenderer = GetComponent<MeshRenderer>();

            _resourceManager = FindObjectOfType<ResourceManager>();
            meshRenderer.material = _resourceManager.ChunkMaterial;

            CalculateMapFromScratch();
            IsDirty = true;
        }

        protected virtual void Update()
        {
            if (IsDirty)
            {
                CreateVisualMesh();
                IsDirty = false;
            }
        }

        private void CalculateMapFromScratch()
        {
            _map = new int[Width, Height, Width];

            Random.seed = _world.Seed;

            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    for (var z = 0; z < Width; z++)
                    {
                        _map[x, y, z] = WorldGenerator.GetTheoreticalId(
                            new Vector3(x, y, z) + transform.position,
                            _world);
                    }
                }
            }
        }

        private void CreateVisualMesh()
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

        private int GetId(int x, int y, int z)
        {

            if ((y < 0) || (y >= Height))
            {
                return 0;
            }

            if ((x < 0) || (z < 0) || (x >= Width) || (z >= Width))
            {
                var worldPosition = new Vector3(x, y, z) + transform.position;
                var chunk = _world.FindChunk(worldPosition);
                if (chunk == this)
                {
                    return 0;
                }
                if (chunk == null)
                {
                    return WorldGenerator.GetTheoreticalId(worldPosition, _world);
                }
                return chunk.GetId(worldPosition);
            }
            return _map[x, y, z];
        }

        private int GetId(Vector3 worldPosition)
        {
            worldPosition -= transform.position;
            var x = Mathf.FloorToInt(worldPosition.x);
            var y = Mathf.FloorToInt(worldPosition.y);
            var z = Mathf.FloorToInt(worldPosition.z);
            return GetId(x, y, z);
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
            IsDirty = true;

            if (x == 0)
            {
                var chunk = _world.FindChunk(new Vector3(x - 2, y, z) + transform.position);
                if (chunk != null)
                {
                    chunk.IsDirty = true;
                }
            }
            if (x == Width - 1)
            {
                var chunk = _world.FindChunk(new Vector3(x + 2, y, z) + transform.position);
                if (chunk != null)
                {
                    chunk.IsDirty = true;
                }
            }
            if (z == 0)
            {
                var chunk = _world.FindChunk(new Vector3(x, y, z - 2) + transform.position);
                if (chunk != null)
                {
                    chunk.IsDirty = true;
                }
            }
            if (z == Width - 1)
            {
                var chunk = _world.FindChunk(new Vector3(x, y, z + 2) + transform.position);
                if (chunk != null)
                {
                    chunk.IsDirty = true;
                }
            }
            return true;
        }

        public int GetBlockIdGlobal(Vector3 worldPosition)
        {
            var localPosition = worldPosition - transform.position;
            var x = Mathf.FloorToInt(localPosition.x);
            var y = Mathf.FloorToInt(localPosition.y);
            var z = Mathf.FloorToInt(localPosition.z);

            return GetBlockIdLocal(x, y, z);
        }

        private int GetBlockIdLocal(int x, int y, int z)
        {
            if (x < 0 || x >= Width)
            {
                throw new ArgumentOutOfRangeException("x", x, "The x coordinate is invalid.");
            }
            if (z < 0 || z >= Width)
            {
                throw new ArgumentOutOfRangeException("z", z, "The z coordinate is invalid.");
            }

            if ((y < 0) || (y >= Height))
            {
                return Block.Air.Id;
            }

            return _map[x, y, z];
        }
    }
}