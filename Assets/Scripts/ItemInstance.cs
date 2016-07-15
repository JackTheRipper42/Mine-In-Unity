using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    [RequireComponent(typeof(MeshFilter))]
    public class ItemInstance : MonoBehaviour
    {
        private const int TextureSize = 32;
        private const float Thickness = 0.05f;
        private const float PixelSize = 0.01f;

        public Texture2D Texture2D;

        private MeshCollider _meshCollider;
        private MeshFilter _meshFilter;

        protected virtual void Start()
        {
            _meshCollider = GetComponent<MeshCollider>();
            _meshFilter = GetComponent<MeshFilter>();
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var colors = new List<Color>();

            var pixels = Texture2D.GetPixels();

            for (var x = 0; x < TextureSize; x++)
            {
                for (var y = 0; y < TextureSize; y++)
                {
                    if (IsTransparent(x, y, pixels))
                    {
                        continue;
                    }

                    var upIsTransparent = IsTransparent(x, y + 1, pixels);
                    var downIsTransparent = IsTransparent(x, y - 1, pixels);
                    var rightIsTransparent = IsTransparent(x + 1, y, pixels);
                    var leftIsTransparent = IsTransparent(x - 1, y, pixels);

                    var color = GetColor(x, y, pixels);
                    color.a = 1f;

                    // Left wall
                    if (leftIsTransparent)
                    {
                        BuildFace(
                            color,
                            new Vector3(x, y, 0),
                            Vector3.up,
                            Vector3.forward,
                            false,
                            vertices,
                            colors,
                            triangles);
                    }

                    // Right wall
                    if (rightIsTransparent)
                    {
                        BuildFace(
                            color,
                            new Vector3(x + 1, y, 0),
                            Vector3.up,
                            Vector3.forward,
                            true,
                            vertices,
                            colors,
                            triangles);
                    }

                    // Bottom wall
                    if (downIsTransparent)
                    {
                        BuildFace(
                            color,
                            new Vector3(x, y, 0),
                            Vector3.forward,
                            Vector3.right,
                            false,
                            vertices,
                            colors,
                            triangles);
                    }

                    // Top wall
                    if (upIsTransparent)
                    {
                        BuildFace(
                            color,
                            new Vector3(x, y + 1, 0),
                            Vector3.forward,
                            Vector3.right,
                            true,
                            vertices,
                            colors,
                            triangles);
                    }

                    // Back
                    BuildFace(
                        color,
                        new Vector3(x, y, 0),
                        Vector3.up,
                        Vector3.right,
                        true,
                        vertices,
                        colors,
                        triangles);

                    // Front
                    BuildFace(
                        color,
                        new Vector3(x, y, 1),
                        Vector3.up,
                        Vector3.right,
                        false,
                        vertices,
                        colors,
                        triangles);
                }
            }

            for (var i = 0; i < vertices.Count; i++)
            {
                var vertex = vertices[i];
                vertices[i] = new Vector3(vertex.x * PixelSize, vertex.y * PixelSize, vertex.z * Thickness);
            }

            var mesh = new Mesh
            {
                vertices = vertices.ToArray(),
                colors = colors.ToArray(),
                triangles = triangles.ToArray()
            };
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            _meshFilter.mesh = mesh;
            _meshCollider.sharedMesh = null;
            _meshCollider.sharedMesh = mesh;
        }

        private static void BuildFace(
            Color color,
            Vector3 corner,
            Vector3 up,
            Vector3 right,
            bool reversed,
            ICollection<Vector3> vertices,
            ICollection<Color> colors,
            ICollection<int> triangles)
        {
            var index = vertices.Count;
            corner.x -= 0.5f;

            vertices.Add(corner);
            vertices.Add(corner + up);
            vertices.Add(corner + up + right);
            vertices.Add(corner + right);

            colors.Add(color);
            colors.Add(color);
            colors.Add(color);
            colors.Add(color);

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

        private static Color GetColor(int x, int y, IList<Color> pixels)
        {
            var index = y * TextureSize + x;
            return pixels[index];
        }

        private static bool IsTransparent(int x, int y, IList<Color> pixels)
        {
            if (x < 0 || y < 0 || x >= TextureSize || y >= TextureSize)
            {
                return false;
            }
            var color = GetColor(x, y, pixels);
            return color.a < 1f;
        }
    }
}
