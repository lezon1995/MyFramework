using UnityEngine;
using Random = System.Random;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Uses random walk to generate a ground with controlled elevation
    /// </summary>
    public class MMGridGeneratorRandomWalkGround : MMGridGenerator
    {
        /// <summary>
        /// Uses random walk to generate a ground with controlled elevation
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="seed"></param>
        /// <param name="minHeightDifference"></param>
        /// <param name="maxHeightDifference"></param>
        /// <param name="minFlatDistance"></param>
        /// <param name="maxFlatDistance"></param>
        /// <returns></returns>
        public static int[,] Generate(int width, int height, int seed, int minHeightDifference, int maxHeightDifference, int minFlatDistance, int maxFlatDistance, int maxHeight)
        {
            Random random = new Random(seed.GetHashCode());
            UnityEngine.Random.InitState(seed);

            int[,] grid = PrepareGrid(ref width, ref height);

            int groundHeight = UnityEngine.Random.Range(0, maxHeight);
            int previousGroundHeight = groundHeight;
            int currentFlatDistance = -1;

            for (int i = 0; i < width; i++)
            {
                groundHeight = previousGroundHeight;
                int newElevation = UnityEngine.Random.Range(minHeightDifference, maxHeightDifference);
                int flatDistance = UnityEngine.Random.Range(minFlatDistance, maxFlatDistance);

                if (currentFlatDistance >= flatDistance - 1)
                {
                    if (random.Next(2) > 0)
                    {
                        groundHeight -= newElevation;
                    }
                    else if (previousGroundHeight + newElevation < height)
                    {
                        groundHeight += newElevation;
                    }

                    groundHeight = Mathf.Clamp(groundHeight, 1, maxHeight);
                    currentFlatDistance = 0;
                }
                else
                {
                    currentFlatDistance++;
                }

                for (int j = groundHeight; j >= 0; j--)
                {
                    grid[i, j] = 1;
                }

                previousGroundHeight = groundHeight;
            }

            return grid;
        }
    }
}