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

        public static List<Chunk> Chunks = new List<Chunk>();

        private static int Width
        {
            get { return World.CurrentWorld.ChunkWidth; }
        }

        private static int Height
        {
            get { return World.CurrentWorld.ChunkHeight; }
        }

        private byte[,,] _map;
        private Mesh _visualMesh;
        private MeshCollider _meshCollider;
        private MeshFilter _meshFilter;

        protected virtual void Start()
        {
            Chunks.Add(this);

            _meshCollider = GetComponent<MeshCollider>();
            _meshFilter = GetComponent<MeshFilter>();

            CalculateMapFromScratch();
            StartCoroutine(CreateVisualMesh());
        }

        private static byte GetTheoreticalByte(Vector3 pos)
        {
            Random.seed = World.CurrentWorld.Seed;

            var grain0Offset = new Vector3(Random.value*10000, Random.value*10000, Random.value*10000);
            var grain1Offset = new Vector3(Random.value*10000, Random.value*10000, Random.value*10000);
            var grain2Offset = new Vector3(Random.value*10000, Random.value*10000, Random.value*10000);

            return GetTheoreticalByte(pos, grain0Offset, grain1Offset, grain2Offset);
        }

        private static byte GetTheoreticalByte(Vector3 pos, Vector3 offset0, Vector3 offset1, Vector3 offset2)
        {
            const float heightBase = 10f;
            var maxHeight = Height - 10f;
            var heightSwing = maxHeight - heightBase;

            byte brick = 1;

            var clusterValue = CalculateNoiseValue(pos, offset1, 0.02f);
            var blobValue = CalculateNoiseValue(pos, offset1, 0.05f);
            var mountainValue = CalculateNoiseValue(pos, offset0, 0.009f);
            if ((Math.Abs(mountainValue) < 0.001f) && (blobValue < 0.2f))
            {
                brick = 2;
            }
            else if (clusterValue > 0.9f)
            {
                brick = 1;
            }
            else if (clusterValue > 0.8f)
            {
                brick = 3;
            }

            mountainValue = Mathf.Sqrt(mountainValue);
            mountainValue *= heightSwing;
            mountainValue += heightBase;
            mountainValue += (blobValue*10) - 5f;

            return mountainValue >= pos.y ? brick : (byte) 0;
        }

        private void CalculateMapFromScratch()
        {
            _map = new byte[Width, Height, Width];

            Random.seed = World.CurrentWorld.Seed;

            for (var x = 0; x < World.CurrentWorld.ChunkWidth; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    for (var z = 0; z < Width; z++)
                    {
                        _map[x, y, z] = GetTheoreticalByte(new Vector3(x, y, z) + transform.position);
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
                        if (_map[x, y, z] == 0) continue;

                        var brick = _map[x, y, z];

                        // Left wall
                        if (IsTransparent(x - 1, y, z))
                        {
                            BuildFace(
                                brick, 
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
                                brick, 
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
                                brick, 
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
                                brick,
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
                                brick, 
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
                                brick, 
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

        private static void BuildFace(
            byte brick, 
            Vector3 corner, 
            Vector3 up, 
            Vector3 right, 
            bool reversed,
            ICollection<Vector3> vertices, 
            ICollection<Vector2> uvs, 
            ICollection<int> tris)
        {
            var index = vertices.Count;

            vertices.Add(corner);
            vertices.Add(corner + up);
            vertices.Add(corner + up + right);
            vertices.Add(corner + right);

            var uvWidth = new Vector2(0.25f, 0.25f);
            var uvCorner = new Vector2(0.00f, 0.75f);

            uvCorner.x += (float) (brick - 1)/4;

            uvs.Add(uvCorner);
            uvs.Add(new Vector2(uvCorner.x, uvCorner.y + uvWidth.y));
            uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y + uvWidth.y));
            uvs.Add(new Vector2(uvCorner.x + uvWidth.x, uvCorner.y));

            if (reversed)
            {
                tris.Add(index + 0);
                tris.Add(index + 1);
                tris.Add(index + 2);
                tris.Add(index + 2);
                tris.Add(index + 3);
                tris.Add(index + 0);
            }
            else
            {
                tris.Add(index + 1);
                tris.Add(index + 0);
                tris.Add(index + 2);
                tris.Add(index + 3);
                tris.Add(index + 2);
                tris.Add(index + 0);
            }

        }

        private bool IsTransparent(int x, int y, int z)
        {
            if (y < 0)
            {
                return false;
            }
            var brick = GetByte(x, y, z);
            switch (brick)
            {
                case 0:
                    return true;
                default:
                    return false;
            }
        }

        public byte GetByte(int x, int y, int z)
        {

            if ((y < 0) || (y >= Height))
            {
                return 0;
            }

            if ((x < 0) || (z < 0) || (x >= Width) || (z >= Width))
            {
                var worldPos = new Vector3(x, y, z) + transform.position;
                var chunk = FindChunk(worldPos);
                if (chunk == this)
                {
                    return 0;
                }
                if (chunk == null)
                {
                    return GetTheoreticalByte(worldPos);
                }
                return chunk.GetByte(worldPos);
            }
            return _map[x, y, z];
        }

        public byte GetByte(Vector3 worldPos)
        {
            worldPos -= transform.position;
            var x = Mathf.FloorToInt(worldPos.x);
            var y = Mathf.FloorToInt(worldPos.y);
            var z = Mathf.FloorToInt(worldPos.z);
            return GetByte(x, y, z);
        }

        public static Chunk FindChunk(Vector3 pos)
        {

            for (var a = 0; a < Chunks.Count; a++)
            {
                var chunkPosition = Chunks[a].transform.position;

                if ((pos.x < chunkPosition.x) ||
                    (pos.z < chunkPosition.z) ||
                    (pos.x >= chunkPosition.x + Width) ||
                    (pos.z >= chunkPosition.z + Width))
                {
                    continue;
                }
                return Chunks[a];

            }
            return null;
        }

        public bool SetBrick(byte brick, Vector3 worldPos)
        {
            worldPos -= transform.position;
            return SetBrick(brick, Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y),
                Mathf.FloorToInt(worldPos.z));
        }

        public bool SetBrick(byte brick, int x, int y, int z)
        {
            if ((x < 0) || (y < 0) || (z < 0) || (x >= Width) || (y >= Height || (z >= Width)))
            {
                return false;
            }
            if (_map[x, y, z] == brick)
            {
                return false;
            }
            _map[x, y, z] = brick;
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