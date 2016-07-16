using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class ResourceManager : MonoBehaviour
    {
        public const int ItemTextureSize = 32;
        public const float ItemTexturePixelSize = 0.01f;

        private const int BlockTextureSize = 128;
        private const int BlockTexturesPerLength = 16;
        private const int ChunkTextureSize = BlockTextureSize*2*BlockTexturesPerLength;

        public Texture2D[] BlockTextures;
        public Texture2D[] ItemTextures;
        public Shader ChunkShader;

        public Material ChunkMaterial { get; private set; }

        public Vector2 BlockUvSize { get; private set; }

        public Dictionary<string, Vector2> BlockUvPositions { get; private set; }

        public Dictionary<string, Color[]> ItemColors { get; private set; } 

        protected virtual void Awake()
        {
            LoadBlockTextures();
            LoadItemTextures();
        }

        private void LoadBlockTextures()
        {
            if (BlockTextures.Length > BlockTexturesPerLength * BlockTexturesPerLength)
            {
                throw new InvalidOperationException("There are to many textures.");
            }

            var mainTexturePixels = new Color[ChunkTextureSize * ChunkTextureSize];
            BlockUvPositions = new Dictionary<string, Vector2>();
            BlockUvSize = new Vector2(
                (float)BlockTextureSize / ChunkTextureSize,
                (float)BlockTextureSize / ChunkTextureSize);

            for (var i = 0; i < BlockTextures.Length; i++)
            {
                var column = i % BlockTexturesPerLength;
                var row = i / BlockTexturesPerLength;

                var blockTexture = BlockTextures[i];
                if (blockTexture.width != BlockTextureSize || blockTexture.height != BlockTextureSize)
                {
                    throw new InvalidOperationException("The texture size is invalid.");
                }
                var blockTexturePixels = blockTexture.GetPixels();

                var mainOffsetX = column * 2 * BlockTextureSize;
                var mainOffsetY = row * 2 * BlockTextureSize;

                BlockUvPositions.Add(
                    blockTexture.name,
                    new Vector2(
                        (float)(mainOffsetX + BlockTextureSize / 2) / ChunkTextureSize,
                        (float)(mainOffsetY + BlockTextureSize / 2) / ChunkTextureSize));

                for (var x = 0; x < BlockTextureSize; x++)
                {
                    for (var y = 0; y < BlockTextureSize; y++)
                    {
                        var color = GetSubTextureColor(x, y, blockTexturePixels);
                        SetMainTextureColor(
                            mainOffsetX + BlockTextureSize / 2 + x,
                            mainOffsetY + BlockTextureSize / 2 + y,
                            color,
                            mainTexturePixels);
                        if (x < BlockTextureSize / 2)
                        {
                            SetMainTextureColor(
                                mainOffsetX + BlockTextureSize / 2 - x - 1,
                                mainOffsetY + BlockTextureSize / 2 + y,
                                color,
                                mainTexturePixels);

                            if (y < BlockTextureSize / 2)
                            {
                                SetMainTextureColor(
                                    mainOffsetX + BlockTextureSize / 2 - x - 1,
                                    mainOffsetY + BlockTextureSize / 2 - y - 1,
                                    color,
                                    mainTexturePixels);
                            }
                            else
                            {
                                SetMainTextureColor(
                                    mainOffsetX + BlockTextureSize / 2 - x - 1,
                                    mainOffsetY + BlockTextureSize / 2 + 2 * BlockTextureSize - y - 1,
                                    color,
                                    mainTexturePixels);
                            }
                        }
                        else
                        {
                            SetMainTextureColor(
                                mainOffsetX + BlockTextureSize / 2 + 2 * BlockTextureSize - x - 1,
                                mainOffsetY + BlockTextureSize / 2 + y,
                                color,
                                mainTexturePixels);
                            if (y < BlockTextureSize / 2)
                            {
                                SetMainTextureColor(
                                    mainOffsetX + BlockTextureSize / 2 + 2 * BlockTextureSize - x - 1,
                                    mainOffsetY + BlockTextureSize / 2 - y - 1,
                                    color,
                                    mainTexturePixels);
                            }
                            else
                            {
                                SetMainTextureColor(
                                    mainOffsetX + BlockTextureSize / 2 + 2 * BlockTextureSize - x - 1,
                                    mainOffsetY + BlockTextureSize / 2 + 2 * BlockTextureSize - y - 1,
                                    color,
                                    mainTexturePixels);
                            }
                        }
                        if (y < BlockTextureSize / 2)
                        {
                            SetMainTextureColor(
                                mainOffsetX + BlockTextureSize / 2 + x,
                                mainOffsetY + BlockTextureSize / 2 - y - 1,
                                color,
                                mainTexturePixels);
                        }
                        else
                        {
                            SetMainTextureColor(
                                mainOffsetX + BlockTextureSize / 2 + x,
                                mainOffsetY + BlockTextureSize / 2 + 2 * BlockTextureSize - y - 1,
                                color,
                                mainTexturePixels);
                        }
                    }
                }
            }

            var chunkTexture = new Texture2D(ChunkTextureSize, ChunkTextureSize, TextureFormat.ARGB32, false)
            {
                filterMode = FilterMode.Trilinear,
                anisoLevel = 16,
                wrapMode = TextureWrapMode.Clamp,
            };

            chunkTexture.SetPixels(mainTexturePixels);
            chunkTexture.Apply(true, true);

            ChunkMaterial = new Material(ChunkShader)
            {
                mainTexture = chunkTexture,
                color = Color.white
            };
        }

        private void LoadItemTextures()
        {
            ItemColors = new Dictionary<string, Color[]>();

            foreach (var itemTexture in ItemTextures)
            {
                ItemColors.Add(itemTexture.name, itemTexture.GetPixels());
            }
        }

        private static void SetMainTextureColor(int x, int y, Color color, IList<Color> pixels)
        {
            var index = y*ChunkTextureSize + x;
            pixels[index] = color;
        }

        private static Color GetSubTextureColor(int x, int y, IList<Color> pixels)
        {
            var index = y*BlockTextureSize + x;
            return pixels[index];
        }
    }
}
