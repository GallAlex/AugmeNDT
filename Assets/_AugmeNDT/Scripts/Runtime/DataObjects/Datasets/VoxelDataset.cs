using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;

namespace AugmeNDT{
    /// <summary>
    /// Class stores the values of a raw dataset
    /// </summary>
    public class VoxelDataset : ScriptableObject
    {
        public string filePath;

        [SerializeField]
        public string datasetName;

        [SerializeField]
        public int[] data = null;

        [SerializeField]
        public int dimX, dimY, dimZ;

        [SerializeField]
        public float scaleX = 1f, scaleY = 1f, scaleZ = 1f;


        private int minDataValue = int.MaxValue;
        private int maxDataValue = int.MinValue;
        private Texture3D dataTexture = null;
        private Texture3D gradientTexture = null;


        public async Task<Texture3D> GetDataTexture()
        {
            if (dataTexture == null)
            {
                Debug.Log("Calculate Texture");
                dataTexture = await CreateTextureInternal();
                Debug.Log("Texture created");
            }
            return dataTexture;
        }

        public Texture3D GetGradientTexture()
        {
            if (gradientTexture == null)
            {
                gradientTexture = CreateGradientTextureInternal();
            }
            return gradientTexture;
        }

        public int GetMinDataValue()
        {
            if (minDataValue == int.MaxValue)
                CalculateValueBounds();
            return minDataValue;
        }

        public int GetMaxDataValue()
        {
            if (maxDataValue == int.MinValue)
                CalculateValueBounds();
            return maxDataValue;
        }

        /// <summary>
        /// Checks the max size of the 3D texture.
        /// Maximum resolution of a 3D texture in Unity is 2048 x 2048 x 2048.
        /// Windows Mixed Reality supports textures with resolutions up to 4096x4096 but it's recommended that you author at 512x512 
        /// </summary>
        public void FixDimensions()
        {
            int MAX_DIM = 512;

            while (Mathf.Max(dimX, dimY, dimZ) > MAX_DIM)
            {
                Debug.LogWarning("Dimension exceeds limits (maximum: " + MAX_DIM + "). Dataset is downscaled by 2 on each axis!");
                DownScaleData();
            }
        }


        /// <summary>
        /// Downscales the data by averaging 8 voxels per each new voxel,
        /// and replaces downscaled data with the original data
        /// </summary>
        public void DownScaleData()
        {
            int halfDimX = dimX / 2 + dimX % 2;
            int halfDimY = dimY / 2 + dimY % 2;
            int halfDimZ = dimZ / 2 + dimZ % 2;
            int[] downScaledData = new int[halfDimX * halfDimY * halfDimZ];

            for (int x = 0; x < halfDimX; x++)
            {
                for (int y = 0; y < halfDimY; y++)
                {
                    for (int z = 0; z < halfDimZ; z++)
                    {
                        downScaledData[x + y * halfDimX + z * (halfDimX * halfDimY)] = Mathf.RoundToInt(GetAvgerageVoxelValues(x * 2, y * 2, z * 2));
                    }
                }
            }

            //Update data & data dimensions
            data = downScaledData;
            dimX = halfDimX;
            dimY = halfDimY;
            dimZ = halfDimZ;
        }


        private void CalculateValueBounds()
        {
            minDataValue = int.MaxValue;
            maxDataValue = int.MinValue;
            int dim = dimX * dimY * dimZ;
            for (int i = 0; i < dim; i++)
            {
                int val = data[i];
                minDataValue = Math.Min(minDataValue, val);
                maxDataValue = Math.Max(maxDataValue, val);
            }
        }

        private async Task<Texture3D> CreateTextureInternal()
        {
            Profiler.BeginSample("CreateTextureInternal");
        
            TextureFormat texformat = SystemInfo.SupportsTextureFormat(TextureFormat.RHalf) ? TextureFormat.RHalf : TextureFormat.RFloat;
            Texture3D texture = new Texture3D(dimX, dimY, dimZ, texformat, false);
            texture.wrapMode = TextureWrapMode.Clamp;

            int minValue = GetMinDataValue();
            int maxValue = GetMaxDataValue();
            int maxRange = maxValue - minValue;

            bool isHalfFloat = texformat == TextureFormat.RHalf;
            try
            {
                // Create a byte array for filling the texture. Store has half (16 bit) or single (32 bit) float values.
                int sampleSize = isHalfFloat ? 2 : 4;
                Debug.Log("Used memory for texture: " + (dimX * dimY * dimZ * sampleSize) / 1024 / 1024 + "MB");
                byte[] bytes = new byte[data.Length * sampleSize]; // This can cause OutOfMemoryException

                await Task.Run(() => CreateByteArray(bytes, data.Length, isHalfFloat, minValue, maxRange, sampleSize));

                //for (int iData = 0; iData < data.Length; iData++)
                //{
                //    //Todo: BitConverter.GetBytes and Array.Copy really slow
                //    float pixelValue = (float)(data[iData] - minValue) / maxRange;
                //    byte[] pixelBytes = isHalfFloat ? BitConverter.GetBytes(Mathf.FloatToHalf(pixelValue)) : BitConverter.GetBytes(pixelValue);

                //    Array.Copy(pixelBytes, 0, bytes, iData * sampleSize, sampleSize);
                //}

                texture.SetPixelData(bytes, 0);
            }
            catch (OutOfMemoryException)
            {
                Debug.LogWarning("Out of memory when creating texture. Using fallback method.");
                for (int x = 0; x < dimX; x++)
                for (int y = 0; y < dimY; y++)
                for (int z = 0; z < dimZ; z++)
                    texture.SetPixel(x, y, z, new Color((float)(data[x + y * dimX + z * (dimX * dimY)] - minValue) / maxRange, 0.0f, 0.0f, 0.0f));
            }

            texture.Apply();
            Profiler.EndSample();
        
            return texture;
        }

        private Texture3D CreateGradientTextureInternal()
        {
            TextureFormat texformat = SystemInfo.SupportsTextureFormat(TextureFormat.RGBAHalf) ? TextureFormat.RGBAHalf : TextureFormat.RGBAFloat;
            Texture3D texture = new Texture3D(dimX, dimY, dimZ, texformat, false);
            texture.wrapMode = TextureWrapMode.Clamp;

            int minValue = GetMinDataValue();
            int maxValue = GetMaxDataValue();
            int maxRange = maxValue - minValue;

            Color[] cols;
            try
            {
                cols = new Color[data.Length];
            }
            catch (OutOfMemoryException)
            {
                cols = null;
            }
            for (int x = 0; x < dimX; x++)
            {
                for (int y = 0; y < dimY; y++)
                {
                    for (int z = 0; z < dimZ; z++)
                    {
                        int iData = x + y * dimX + z * (dimX * dimY);

                        int x1 = data[Math.Min(x + 1, dimX - 1) + y * dimX + z * (dimX * dimY)] - minValue;
                        int x2 = data[Math.Max(x - 1, 0) + y * dimX + z * (dimX * dimY)] - minValue;
                        int y1 = data[x + Math.Min(y + 1, dimY - 1) * dimX + z * (dimX * dimY)] - minValue;
                        int y2 = data[x + Math.Max(y - 1, 0) * dimX + z * (dimX * dimY)] - minValue;
                        int z1 = data[x + y * dimX + Math.Min(z + 1, dimZ - 1) * (dimX * dimY)] - minValue;
                        int z2 = data[x + y * dimX + Math.Max(z - 1, 0) * (dimX * dimY)] - minValue;

                        Vector3 grad = new Vector3((x2 - x1) / (float)maxRange, (y2 - y1) / (float)maxRange, (z2 - z1) / (float)maxRange);

                        if (cols == null)
                        {
                            texture.SetPixel(x, y, z, new Color(grad.x, grad.y, grad.z, (float)(data[iData] - minValue) / maxRange));
                        }
                        else
                        {
                            cols[iData] = new Color(grad.x, grad.y, grad.z, (float)(data[iData] - minValue) / maxRange);
                        }
                    }
                }
            }
            if (cols != null) texture.SetPixels(cols);
            texture.Apply();
            return texture;
        }

        public float GetAvgerageVoxelValues(int x, int y, int z)
        {
            // if a dimension length is not an even number
            bool xC = x + 1 == dimX;
            bool yC = y + 1 == dimY;
            bool zC = z + 1 == dimZ;

            //if expression can only be true on the edges of the texture
            if (xC || yC || zC)
            {
                if (!xC && yC && zC) return (GetData(x, y, z) + GetData(x + 1, y, z)) / 2.0f;
                else if (xC && !yC && zC) return (GetData(x, y, z) + GetData(x, y + 1, z)) / 2.0f;
                else if (xC && yC && !zC) return (GetData(x, y, z) + GetData(x, y, z + 1)) / 2.0f;
                else if (!xC && !yC && zC) return (GetData(x, y, z) + GetData(x + 1, y, z) + GetData(x, y + 1, z) + GetData(x + 1, y + 1, z)) / 4.0f;
                else if (!xC && yC && !zC) return (GetData(x, y, z) + GetData(x + 1, y, z) + GetData(x, y, z + 1) + GetData(x + 1, y, z + 1)) / 4.0f;
                else if (xC && !yC && !zC) return (GetData(x, y, z) + GetData(x, y + 1, z) + GetData(x, y, z + 1) + GetData(x, y + 1, z + 1)) / 4.0f;
                else return GetData(x, y, z); // if xC && yC && zC
            }
            return (GetData(x, y, z) + GetData(x + 1, y, z) + GetData(x, y + 1, z) + GetData(x + 1, y + 1, z)
                    + GetData(x, y, z + 1) + GetData(x, y + 1, z + 1) + GetData(x + 1, y, z + 1) + GetData(x + 1, y + 1, z + 1)) / 8.0f;
        }

        public int GetData(int x, int y, int z)
        {
            return data[x + y * dimX + z * (dimX * dimY)];
        }

        private void CreateByteArray(byte[] bytes, int lenght, bool isHalfFloat, int minValue, int maxRange, int sampleSize)
        {
            for (int iData = 0; iData < lenght; iData++)
            {
                float pixelValue = (float)(data[iData] - minValue) / maxRange;
                byte[] pixelBytes = isHalfFloat ? BitConverter.GetBytes(Mathf.FloatToHalf(pixelValue)) : BitConverter.GetBytes(pixelValue);

                Array.Copy(pixelBytes, 0, bytes, iData * sampleSize, sampleSize);
            }
        }
    }
}
