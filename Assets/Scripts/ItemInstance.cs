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

                    var isRightTransparent = IsTransparent(x + 1, y, pixels);
                    var isLeftTransparent = IsTransparent(x - 1, y, pixels);
                    var isUpTransparent = IsTransparent(x, y + 1, pixels);
                    var isDownTransparent = IsTransparent(x, y - 1, pixels);

                    var color = GetColor(x, y, pixels);
                    color.a = 1f;

                    BuildUpDownFace(
                        x,
                        y,
                        color,
                        vertices,
                        colors,
                        triangles,
                        isRightTransparent,
                        isLeftTransparent,
                        isUpTransparent,
                        isDownTransparent);
                }
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

        private static void BuildUpDownFace(
            int x,
            int y,
            Color color,
            ICollection<Vector3> vertices,
            ICollection<Color> colors,
            ICollection<int> triangles,
            bool isRightTransparent,
            bool isLeftTransparent,
            bool isUpTransparent,
            bool isDownTransparent)
        {
            var index = vertices.Count;

            var right = new Vector3(PixelSize, 0f, 0f);
            var up = new Vector3(0f, PixelSize, 0f);
            var frontCorner = new Vector3(x*PixelSize, y*PixelSize, 0f);
            var backCorner = new Vector3(x*PixelSize, y*PixelSize, PixelSize);

            vertices.Add(frontCorner);
            vertices.Add(frontCorner + up);
            vertices.Add(frontCorner + up + right);
            vertices.Add(frontCorner + right);

            vertices.Add(backCorner);
            vertices.Add(backCorner + up);
            vertices.Add(backCorner + up + right);
            vertices.Add(backCorner + right);

            colors.Add(color);
            colors.Add(color);
            colors.Add(color);
            colors.Add(color);

            colors.Add(color);
            colors.Add(color);
            colors.Add(color);
            colors.Add(color);

            triangles.Add(index + 0);
            triangles.Add(index + 1);
            triangles.Add(index + 2);
            triangles.Add(index + 0);
            triangles.Add(index + 2);
            triangles.Add(index + 3);

            triangles.Add(index + 4);
            triangles.Add(index + 7);
            triangles.Add(index + 6);
            triangles.Add(index + 4);
            triangles.Add(index + 6);
            triangles.Add(index + 5);

            if (isRightTransparent)
            {
                triangles.Add(index + 3);
                triangles.Add(index + 2);
                triangles.Add(index + 6);
                triangles.Add(index + 3);
                triangles.Add(index + 6);
                triangles.Add(index + 7);
            }
            if (isLeftTransparent)
            {
                triangles.Add(index + 0);
                triangles.Add(index + 4);
                triangles.Add(index + 5);
                triangles.Add(index + 0);
                triangles.Add(index + 5);
                triangles.Add(index + 1);
            }
            if (isUpTransparent)
            {
                triangles.Add(index + 1);
                triangles.Add(index + 5);
                triangles.Add(index + 6);
                triangles.Add(index + 1);
                triangles.Add(index + 6);
                triangles.Add(index + 2);
            }
            if (isDownTransparent)
            {
                triangles.Add(index + 0);
                triangles.Add(index + 3);
                triangles.Add(index + 7);
                triangles.Add(index + 0);
                triangles.Add(index + 7);
                triangles.Add(index + 4);
            }
        }

        private static Color GetColor(int x, int y, IList<Color> pixels)
        {
            var index = y*TextureSize + x;
            return pixels[index];
        }

        private static bool IsTransparent(int x, int y, IList<Color> pixels)
        {
            if (x < 0 || y < 0 || x >= TextureSize || y >= TextureSize)
            {
                return true;
            }
            var color = GetColor(x, y, pixels);
            return color.a < 1f;
        }
    }
}