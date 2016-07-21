using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        private MeshCollider _meshCollider;
        private MeshFilter _meshFilter;
        private World _world;
        private HashSet<Position3> _randomTickBlocks;

        public bool IsDirty { get; private set; }

        public ChunkData Data { get; private set; }

        public int TickUpdateBlocks
        {
            get { return _randomTickBlocks.Count; }
        }

        public void Initialize(ChunkData chunkData)
        {
            transform.position = chunkData.Position.ToVector3();
            transform.rotation = Quaternion.identity;
            Data = chunkData;

            _world = FindObjectOfType<World>();

            _meshCollider = GetComponent<MeshCollider>();
            _meshFilter = GetComponent<MeshFilter>();
            var meshRenderer = GetComponent<MeshRenderer>();

            _resourceManager = FindObjectOfType<ResourceManager>();
            meshRenderer.material = _resourceManager.ChunkMaterial;

            _randomTickBlocks = new HashSet<Position3>();
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    for (var z = 0; z < Width; z++)
                    {
                        var id = Data.Map[x, y, z];
                        if (Block.Blocks[id].RequiresRandomTickUpdate)
                        {
                            _randomTickBlocks.Add(new Position3(x, y, z));
                        }
                    }
                }
            }

            IsDirty = true;
        }

        public int GetBlockIdGlobal(Position3 worldPosition)
        {
            var localPosition = worldPosition - Data.Position;
            return GetBlockIdLocal(localPosition.X, localPosition.Y, localPosition.Z);
        }

        public int GetBlockIdGlobal(int worldX, int worldY, int worldZ)
        {
            return GetBlockIdLocal(
                worldX - Data.Position.X,
                worldY - Data.Position.Y,
                worldZ - Data.Position.Z);
        }

        public void SetBlockIdGlobal(Position3 worldPosition, int id)
        {
            var localPosition = worldPosition - Data.Position;
            SetBlockIdLocal(localPosition.X, localPosition.Y, localPosition.Z, id);
        }

        public void SetBlockIdGlobal(int worldX, int worldY, int worldZ, int id)
        {
            SetBlockIdLocal(
                worldX - Data.Position.X,
                worldY - Data.Position.Y,
                worldZ - Data.Position.Z,
                id);
        }

        public void TickUpdate()
        {
            foreach (var position in _randomTickBlocks.ToList())
            {
                var id = Data.Map[position.X, position.Y, position.Z];
                var worldPosition = position + Data.Position;
                Block.Blocks[id].OnRandomTick(worldPosition.X, worldPosition.Y, worldPosition.Z, _world);
            }
        }

        public void EnableRandomTickUpdate(Position3 worldPosition)
        {
            var localPosition = worldPosition - Data.Position;
            _randomTickBlocks.Add(localPosition);
        }

        public void DisableRandomTickUpdate(Position3 worldPosition)
        {
            var localPosition = worldPosition - Data.Position;
            _randomTickBlocks.Remove(localPosition);
        }

        protected virtual void Update()
        {           
            if (IsDirty)
            {
                CreateMeshes();
                IsDirty = false;
            }
        }

        private void CreateMeshes()
        {
            var visualVertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var visualTriangles = new List<int>();
            var colliderVertices = new List<Vector3>();
            var colliderTriangles = new List<int>();

            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    for (var z = 0; z < Width; z++)
                    {
                        if (Data.Map[x, y, z] == 0)
                        {
                            continue;
                        }

                        var id = Data.Map[x, y, z];

                        var worldX = x + Data.Position.X;
                        var worldY = y + Data.Position.Y;
                        var worldZ = z + Data.Position.Z;

                        if (Block.Blocks[id].IsSolid(x, y, z, _world))
                        {
                            // Left wall
                            if (!IsSolid(x - 1, y, z))
                            {
                                BuildFace(
                                    new Vector3(x, y, z),
                                    Vector3.up,
                                    Vector3.forward,
                                    false,
                                    colliderVertices,
                                    colliderTriangles);
                            }

                            // Right wall
                            if (!IsSolid(x + 1, y, z))
                            {
                                BuildFace(
                                    new Vector3(x + 1, y, z),
                                    Vector3.up,
                                    Vector3.forward,
                                    true,
                                    colliderVertices,
                                    colliderTriangles);
                            }

                            // Bottom wall
                            if (!IsSolid(x, y - 1, z))
                            {
                                BuildFace(
                                    new Vector3(x, y, z),
                                    Vector3.forward,
                                    Vector3.right,
                                    false,
                                    colliderVertices,
                                    colliderTriangles);
                            }

                            // Top wall
                            if (!IsSolid(x, y + 1, z))
                            {
                                BuildFace(
                                    new Vector3(x, y + 1, z),
                                    Vector3.forward,
                                    Vector3.right,
                                    true,
                                    colliderVertices,
                                    colliderTriangles);
                            }

                            // Back
                            if (!IsSolid(x, y, z - 1))
                            {
                                BuildFace(
                                    new Vector3(x, y, z),
                                    Vector3.up,
                                    Vector3.right,
                                    true,
                                    colliderVertices,
                                    colliderTriangles);
                            }

                            // Front
                            if (!IsSolid(x, y, z + 1))
                            {
                                BuildFace(
                                    new Vector3(x, y, z + 1),
                                    Vector3.up,
                                    Vector3.right,
                                    false,
                                    colliderVertices,
                                    colliderTriangles);
                            }
                        }

                        // Left wall
                        if (IsTransparent(x - 1, y, z, Side.Right))
                        {
                            BuildFace(
                                worldX,
                                worldY,
                                worldZ,
                                Side.Left,
                                id,
                                new Vector3(x, y, z),
                                Vector3.up,
                                Vector3.forward,
                                false,
                                visualVertices,
                                uvs,
                                visualTriangles);
                        }

                        // Right wall
                        if (IsTransparent(x + 1, y, z, Side.Left))
                        {
                            BuildFace(
                                worldX,
                                worldY,
                                worldZ,
                                Side.Right,
                                id,
                                new Vector3(x + 1, y, z),
                                Vector3.up,
                                Vector3.forward,
                                true,
                                visualVertices,
                                uvs,
                                visualTriangles);
                        }

                        // Bottom wall
                        if (IsTransparent(x, y - 1, z, Side.Up))
                        {
                            BuildFace(
                                worldX,
                                worldY,
                                worldZ,
                                Side.Down,
                                id,
                                new Vector3(x, y, z),
                                Vector3.forward,
                                Vector3.right,
                                false,
                                visualVertices,
                                uvs,
                                visualTriangles);
                        }

                        // Top wall
                        if (IsTransparent(x, y + 1, z, Side.Down))
                        {
                            BuildFace(
                                worldX,
                                worldY,
                                worldZ,
                                Side.Up,
                                id,
                                new Vector3(x, y + 1, z),
                                Vector3.forward,
                                Vector3.right,
                                true,
                                visualVertices,
                                uvs,
                                visualTriangles);
                        }

                        // Back
                        if (IsTransparent(x, y, z - 1, Side.Front))
                        {
                            BuildFace(
                                worldX,
                                worldY,
                                worldZ,
                                Side.Back,
                                id,
                                new Vector3(x, y, z),
                                Vector3.up,
                                Vector3.right,
                                true,
                                visualVertices,
                                uvs,
                                visualTriangles);
                        }

                        // Front
                        if (IsTransparent(x, y, z + 1, Side.Back))
                        {
                            BuildFace(
                                worldX,
                                worldY,
                                worldZ,
                                Side.Front,
                                id,
                                new Vector3(x, y, z + 1),
                                Vector3.up,
                                Vector3.right,
                                false,
                                visualVertices,
                                uvs,
                                visualTriangles);
                        }
                    }
                }
            }

            var visualMesh = new Mesh
            {
                vertices = visualVertices.ToArray(),
                uv = uvs.ToArray(),
                triangles = visualTriangles.ToArray()
            };
            visualMesh.RecalculateBounds();
            visualMesh.RecalculateNormals();
            _meshFilter.mesh = visualMesh;

            var colliderMesh = new Mesh
            {
                vertices = colliderVertices.ToArray(),
                triangles = colliderTriangles.ToArray()
            };
            colliderMesh.RecalculateBounds();
            colliderMesh.RecalculateNormals();
            _meshCollider.sharedMesh = null;
            _meshCollider.sharedMesh = colliderMesh;
        }

        private void BuildFace(
            int x,
            int y, 
            int z,
            Side side,
            int id,
            Vector3 corner,
            Vector3 up,
            Vector3 right,
            bool reversed,
            ICollection<Vector3> vertices,
            ICollection<Vector2> uvs,
            ICollection<int> triangles)
        {
            BuildFace(
                corner,
                up,
                right,
                reversed,
                vertices,
                triangles);

            var uvWidth = _resourceManager.BlockUvSize;
            var uvCorner = _resourceManager.BlockUvPositions[Block.Blocks[id].GetUvName(x, y, z, _world, side)];

            uvs.Add(uvCorner);
            uvs.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
            uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
            uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));
        }

        private void BuildFace(
            Vector3 corner,
            Vector3 up,
            Vector3 right,
            bool reversed,
            ICollection<Vector3> vertices,
            ICollection<int> triangles)
        {
            var index = vertices.Count;

            vertices.Add(corner);
            vertices.Add(corner + up);
            vertices.Add(corner + up + right);
            vertices.Add(corner + right);

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

        private bool IsTransparent(int x, int y, int z, Side side)
        {
            if (y < 0)
            {
                return false;
            }
            if (y >= Height)
            {
                return true;
            }

            int id;

            if ((x < 0) || (z < 0) || (x >= Width) || (z >= Width))
            {
                var worldPosition = new Position3(x, y, z) + Data.Position;
                var chunk = _world.FindChunk(worldPosition);
                if (chunk == null)
                {
                    return true;
                }
                id = chunk.GetBlockIdGlobal(worldPosition);
            }
            else
            {
                id = Data.Map[x, y, z];
            }

            return Block.Blocks[id].IsTransparent(
                x + Data.Position.X,
                y + Data.Position.Y,
                z + Data.Position.Z,
                _world,
                side);
        }

        private bool IsSolid(int x, int y, int z)
        {
            if (y < 0)
            {
                return false;
            }
            if (y >= Height)
            {
                return true;
            }

            int id;

            if ((x < 0) || (z < 0) || (x >= Width) || (z >= Width))
            {
                var worldPosition = new Position3(x, y, z) + Data.Position;
                var chunk = _world.FindChunk(worldPosition);
                if (chunk == null)
                {
                    return false;
                }
                id = chunk.GetBlockIdGlobal(worldPosition);
            }
            else
            {
                id = Data.Map[x, y, z];
            }

            return Block.Blocks[id].IsSolid(
                x + Data.Position.X,
                y + Data.Position.Y,
                z + Data.Position.Z,
                _world);
        }

        private void SetBlockIdLocal(int x, int y, int z, int id)
        {
            if (x < 0 || x >= Width)
            {
                throw new ArgumentOutOfRangeException("x", x, "The x coordinate is invalid.");
            }
            if (y < 0 || y >= Height)
            {
                throw new ArgumentOutOfRangeException("y", y, "The y coordinate is invalid.");
            }
            if (z < 0 || z >= Width)
            {
                throw new ArgumentOutOfRangeException("z", z, "The z coordinate is invalid.");
            }

            var oldId = Data.Map[x, y, z];

            if (oldId == id)
            {
                return;
            }

            var localPosition = new Position3(x, y, z);

            _randomTickBlocks.Remove(localPosition);
            if (Block.Blocks[id].RequiresRandomTickUpdate)
            {
                _randomTickBlocks.Add(localPosition);
            }
            Data.Map[x, y, z] = id;

            IsDirty = true;

            if (x == 0)
            {
                var chunk = _world.FindChunk(new Position3(x - 2, y, z) + Data.Position);
                if (chunk != null)
                {
                    chunk.IsDirty = true;
                }
            }
            if (x == Width - 1)
            {
                var chunk = _world.FindChunk(new Position3(x + 2, y, z) + Data.Position);
                if (chunk != null)
                {
                    chunk.IsDirty = true;
                }
            }
            if (z == 0)
            {
                var chunk = _world.FindChunk(new Position3(x, y, z - 2) + Data.Position);
                if (chunk != null)
                {
                    chunk.IsDirty = true;
                }
            }
            if (z == Width - 1)
            {
                var chunk = _world.FindChunk(new Position3(x, y, z + 2) + Data.Position);
                if (chunk != null)
                {
                    chunk.IsDirty = true;
                }
            }
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

            return Data.Map[x, y, z];
        }
    }
}