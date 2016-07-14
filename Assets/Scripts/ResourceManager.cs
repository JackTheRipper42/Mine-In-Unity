using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Scripts
{
    public class ResourceManager : MonoBehaviour
    {
        private const int SubTextureSize = 128;
        private const int TilesPerLength = 16;
        private const int MainTextureSize = SubTextureSize*2*TilesPerLength;

        public Texture2D[] Textures;
        public Shader Shader;

        public Material Material { get; private set; }

        public Vector2 UvSize { get; private set; }

        public Dictionary<string, Vector2> UvPositions { get; private set; }

        protected virtual void Awake()
        {
            if (Textures.Length > TilesPerLength*TilesPerLength)
            {
                throw new InvalidOperationException("There are to many textures.");
            }

            var mainTexturePixels = new Color[MainTextureSize*MainTextureSize];
            UvPositions = new Dictionary<string, Vector2>();
            UvSize = new Vector2(
                (float) SubTextureSize/MainTextureSize,
                (float) SubTextureSize/MainTextureSize);

            for (var i = 0; i < Textures.Length; i++)
            {
                var column = i%TilesPerLength;
                var row = i/TilesPerLength;

                var subTexture = Textures[i];
                var subTexturePixels = subTexture.GetPixels();
                
                var mainOffsetX = column*2*SubTextureSize;
                var mainOffsetY = row*2*SubTextureSize;

                UvPositions.Add(
                    subTexture.name,
                    new Vector2(
                        (float) (mainOffsetX + SubTextureSize/2)/MainTextureSize,
                        (float) (mainOffsetY + SubTextureSize/2)/MainTextureSize));

                for (var x = 0; x < SubTextureSize; x++)
                {
                    for (var y = 0; y < SubTextureSize; y++)
                    {
                        var color = GetSubTexturePixel(x, y, subTexturePixels);
                        SetMainTexturePixel(
                            mainOffsetX + SubTextureSize/2 + x,
                            mainOffsetY + SubTextureSize/2 + y,
                            color,
                            mainTexturePixels);
                        if (x < SubTextureSize/2)
                        {
                            SetMainTexturePixel(
                                mainOffsetX + SubTextureSize/2 - x - 1,
                                mainOffsetY + SubTextureSize/2 + y,
                                color,
                                mainTexturePixels);

                            if (y < SubTextureSize/2)
                            {
                                SetMainTexturePixel(
                                    mainOffsetX + SubTextureSize/2 - x - 1,
                                    mainOffsetY + SubTextureSize/2 - y - 1,
                                    color,
                                    mainTexturePixels);
                            }
                            else
                            {
                                SetMainTexturePixel(
                                    mainOffsetX + SubTextureSize/2 - x - 1,
                                    mainOffsetY + SubTextureSize/2 + 2*SubTextureSize - y - 1,
                                    color,
                                    mainTexturePixels);
                            }
                        }
                        else
                        {
                            SetMainTexturePixel(
                                mainOffsetX + SubTextureSize/2 + 2*SubTextureSize - x - 1,
                                mainOffsetY + SubTextureSize/2 + y,
                                color,
                                mainTexturePixels);
                            if (y < SubTextureSize/2)
                            {
                                SetMainTexturePixel(
                                    mainOffsetX + SubTextureSize/2 + 2*SubTextureSize - x - 1,
                                    mainOffsetY + SubTextureSize/2 - y - 1,
                                    color,
                                    mainTexturePixels);
                            }
                            else
                            {
                                SetMainTexturePixel(
                                    mainOffsetX + SubTextureSize/2 + 2*SubTextureSize - x - 1,
                                    mainOffsetY + SubTextureSize/2 + 2*SubTextureSize - y - 1,
                                    color,
                                    mainTexturePixels);
                            }
                        }
                        if (y < SubTextureSize/2)
                        {
                            SetMainTexturePixel(
                                mainOffsetX + SubTextureSize/2 + x,
                                mainOffsetY + SubTextureSize/2 - y - 1,
                                color,
                                mainTexturePixels);
                        }
                        else
                        {
                            SetMainTexturePixel(
                                mainOffsetX + SubTextureSize/2 + x,
                                mainOffsetY + SubTextureSize/2 + 2*SubTextureSize - y - 1,
                                color,
                                mainTexturePixels);
                        }
                    }
                }
            }

            var mainTexture = new Texture2D(MainTextureSize, MainTextureSize, TextureFormat.ARGB32, false)
            {
                filterMode = FilterMode.Trilinear,
                anisoLevel = 16,
                wrapMode = TextureWrapMode.Clamp,
                alphaIsTransparency = true,                
            };
           
            mainTexture.SetPixels(mainTexturePixels);
            mainTexture.Apply(true, true);

            Material = new Material(Shader)
            {
                mainTexture = mainTexture,
                color = Color.white
            };

            //File.WriteAllBytes("stuff.png", mainTexture.EncodeToPNG());
        }

        private static void SetMainTexturePixel(int x, int y, Color color, IList<Color> pixels)
        {
            var index = y*MainTextureSize + x;
            pixels[index] = color;
        }

        private static Color GetSubTexturePixel(int x, int y, IList<Color> pixels)
        {
            var index = y*SubTextureSize + x;
            return pixels[index];
        }
    }
}
