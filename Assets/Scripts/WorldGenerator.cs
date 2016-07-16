using UnityEngine;

namespace Assets.Scripts
{
    public static class WorldGenerator
    {
        public static int GetTheoreticalId(Position3 worldPosition, World world)
        {
            Random.seed = world.Seed;

            var grain0Offset = new Vector3(Random.value * 10000, Random.value * 10000, Random.value * 10000);
            var grain1Offset = new Vector3(Random.value * 10000, Random.value * 10000, Random.value * 10000);

            return GetTheoreticalId(worldPosition, grain0Offset, grain1Offset);
        }

        private static int GetTheoreticalId(Position3 worldPosition, Vector3 offset0, Vector3 offset1)
        {
            const float heightBase = 10f;
            const float maxHeight = Chunk.Height - 10f;
            const float heightSwing = maxHeight - heightBase;

            var id = Block.Grass.Id;

            var clusterValue = CalculateNoiseValue(worldPosition, offset1, 0.02f);
            var blobValue = CalculateNoiseValue(worldPosition, offset1, 0.05f);
            var mountainValue = CalculateNoiseValue(worldPosition, offset0, 0.009f);
            if ((Mathf.Abs(mountainValue) < 0.001f) && (blobValue < 0.2f))
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
            mountainValue += (blobValue * 10) - 5f;

            return mountainValue >= worldPosition.Y ? id : 0;
        }

        private static float CalculateNoiseValue(Position3 pos, Vector3 offset, float scale)
        {
            var noiseX = Mathf.Abs((pos.X + offset.x) * scale);
            var noiseY = Mathf.Abs((pos.Y + offset.y) * scale);
            var noiseZ = Mathf.Abs((pos.Z + offset.z) * scale);

            return Mathf.Max(0, Noise.Generate(noiseX, noiseY, noiseZ));
        }
    }
}
