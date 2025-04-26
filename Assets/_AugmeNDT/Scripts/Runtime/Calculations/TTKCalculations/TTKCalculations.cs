using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using UnityEngine;

namespace AugmeNDT
{
    public class TTKCalculations
    {
        // Paths for input/output and scripts
        public string mhdPath;
        private string ttkScripts = Application.streamingAssetsPath + @"/TTKScripts/";
        private string ttkResults = Application.streamingAssetsPath + @"/TTKResults/";
        private CultureInfo culture = new CultureInfo("en-US");

        /// <summary>
        /// Starts a Python process and retries up to 4 times until it succeeds.
        /// </summary>
        private bool StartPythonScript(string arguments)
        {
            int maxRetries = 4;
            int retryCount = 0;
            bool succeed = false;

            while (retryCount < maxRetries)
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = ttkScripts
                };

                using (Process process = new Process { StartInfo = psi })
                {
                    process.Start();

                    // Read output and error streams
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    // Check for success flag
                    if (output.Contains("PROCESS_IS_SUCCEED"))
                    {
                        succeed = true;
                        break;
                    }
                    else
                    {
                        Thread.Sleep(2000); // Wait 2 seconds before retrying
                        retryCount++;
                    }
                }
            }

            return succeed;
        }

        /// <summary>
        /// Gets the full 3D gradient field from TTK, or loads it from cache if available.
        /// </summary>
        public List<GradientDataset> GetGradientAllVectorField()
        {
            string csvOutputPath = ttkResults + @"gradient_AllVectorField.csv";
            if (File.Exists(csvOutputPath))
                return LoadGradients(csvOutputPath);

            string pythonScript = "./calculate_gradient_AllVectorField.py";
            string arguments = $"\"{pythonScript}\" --mhd \"{mhdPath}\" --csv \"{csvOutputPath}\"";

            bool isSucceed = StartPythonScript(arguments);
            return isSucceed ? LoadGradients(csvOutputPath) : new List<GradientDataset>();
        }

        /// <summary>
        /// Gets the critical points from the full 3D vector field.
        /// </summary>
        public List<CriticalPointDataset> GetCriticalPointAllVectorField()
        {
            string csvOutputPath = ttkResults + @"critical_points_AllVectorField.csv";
            if (File.Exists(csvOutputPath))
                return LoadCriticalPoints(csvOutputPath);

            string pythonScript = "./calculate_critical_point_AllVectorField.py";
            string arguments = $"\"{pythonScript}\" --mhd \"{mhdPath}\" --csv \"{csvOutputPath}\"";

            bool isSucceed = StartPythonScript(arguments);
            return isSucceed ? LoadCriticalPoints(csvOutputPath) : new List<CriticalPointDataset>();
        }

        /// <summary>
        /// Calculates the 2D slice gradient field using TTK.
        /// </summary>
        public List<GradientDataset> GetGradient2DSlice(Vector3 origin, Vector3 normal, List<int> minMaxValues)
        {
            string pythonScript = "./calculate_gradient_2D_Slice.py";
            string csvOutputPath = ttkResults + @"gradient_2D_Slice.csv";

            if (File.Exists(csvOutputPath))
                File.Delete(csvOutputPath);

            string arguments = $"\"{pythonScript}\" --mhd \"{mhdPath}\" " +
                               $"--origin {origin.x} {origin.y} {origin.z} " +
                               $"--normal {normal.x} {normal.y} {normal.z} " +
                               $" --voi {minMaxValues[0]} {minMaxValues[1]} " +  // min x, max x
                               $" {minMaxValues[2]} {minMaxValues[3]} " +       // min y, max y
                               $" {minMaxValues[4]} {minMaxValues[5]} " +       // min z, max z
                               $"--csv \"{csvOutputPath}\"";

            bool isSucceed = StartPythonScript(arguments);
            return isSucceed ? LoadGradients(csvOutputPath) : new List<GradientDataset>();
        }

        /// <summary>
        /// Calculates 2D slice critical points using TTK.
        /// </summary>
        public List<CriticalPointDataset> GetCriticalpoint2DSlice(Vector3 origin, Vector3 normal, List<int> minMaxValues)
        {
            string pythonScript = "./calculate_critical_point_2D_Slice.py";
            string csvOutputPath = ttkResults + @"critical_points_2D_Slice.csv";

            if (File.Exists(csvOutputPath))
                File.Delete(csvOutputPath);

            string arguments = $"\"{pythonScript}\" --mhd \"{mhdPath}\" " +
                               $"--origin {origin.x} {origin.y} {origin.z} " +
                               $"--normal {normal.x} {normal.y} {normal.z} " +
                               $"--voi {minMaxValues[0]} {minMaxValues[1]} " +
                               $" {minMaxValues[2]} {minMaxValues[3]} " +
                               $" {minMaxValues[4]} {minMaxValues[5]} " +
                               $"--csv \"{csvOutputPath}\"";

            bool isSucceed = StartPythonScript(arguments);
            return isSucceed ? LoadCriticalPoints(csvOutputPath) : new List<CriticalPointDataset>();
        }

        /// <summary>
        /// Gets gradient field data for a specific 3D volume of interest.
        /// NOTE: TTK requires all VOI values to be integers.
        /// </summary>
        public List<GradientDataset> GetGradient3DSubset(List<int> minMaxValues)
        {
            string pythonScript = "./calculate_gradient_3D_Subset.py";
            string csvOutputPath = ttkResults + @"gradient_3D_Subset.csv";

            if (File.Exists(csvOutputPath))
                File.Delete(csvOutputPath);

            string arguments = $"\"{pythonScript}\" --mhd \"{mhdPath}\" " +
                               $"--voi {minMaxValues[0]} {minMaxValues[1]} " +
                               $" {minMaxValues[2]} {minMaxValues[3]} " +
                               $" {minMaxValues[4]} {minMaxValues[5]} " +
                               $"--csv \"{csvOutputPath}\"";

            bool isSucceed = StartPythonScript(arguments);
            return isSucceed ? LoadGradients(csvOutputPath) : new List<GradientDataset>();
        }

        /// <summary>
        /// Gets critical points from a 3D volume subset using TTK.
        /// </summary>
        public List<CriticalPointDataset> GetCriticalpoint3DSubset(List<int> minMaxValues)
        {
            string pythonScript = "./calculate_criticalpoint_3D_Subset.py";
            string csvOutputPath = ttkResults + @"criticalpoint_3D_Subset.csv";

            if (File.Exists(csvOutputPath))
                File.Delete(csvOutputPath);

            string arguments = $"\"{pythonScript}\" --mhd \"{mhdPath}\" " +
                               $"--voi {minMaxValues[0]} {minMaxValues[1]} " +
                               $" {minMaxValues[2]} {minMaxValues[3]} " +
                               $" {minMaxValues[4]} {minMaxValues[5]} " +
                               $"--csv \"{csvOutputPath}\"";

            bool isSucceed = StartPythonScript(arguments);
            return isSucceed ? LoadCriticalPoints(csvOutputPath) : new List<CriticalPointDataset>();
        }

        /// <summary>
        /// Loads gradient data from a CSV file and parses it into GradientDataset objects.
        /// </summary>
        private List<GradientDataset> LoadGradients(string fullPath)
        {
            string[] lines = null;
            try
            {
                lines = File.ReadAllLines(fullPath);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"LoadGradients_CsvLoadingError: {e}");
            }

            List<GradientDataset> gradientList = new List<GradientDataset>();
            if (lines == null)
                return gradientList;

            for (int i = 1; i < lines.Length; i++) // Skip header
            {
                string[] values = lines[i].Split(',');

                if (values.Length < 6) continue;

                if (!double.TryParse(values[3], NumberStyles.Float, culture, out double gradX) ||
                    !double.TryParse(values[4], NumberStyles.Float, culture, out double gradY) ||
                    !double.TryParse(values[5], NumberStyles.Float, culture, out double gradZ))
                {
                    UnityEngine.Debug.LogError($"Failed to parse gradient values at row {i}: {lines[i]}");
                    continue;
                }

                float x = float.Parse(values[0]);
                float y = float.Parse(values[1]);
                float z = float.Parse(values[2]);
                float magnitude = Mathf.Sqrt((float)(gradX * gradX + gradY * gradY + gradZ * gradZ));

                int id = gradientList.Count;
                Vector3 position = new Vector3(x, y, z);
                GradientDataset data = new GradientDataset(id, position, new Vector3((float)gradX, (float)gradY, (float)gradZ), magnitude);

                gradientList.Add(data);
            }

            return gradientList;
        }

        /// <summary>
        /// Loads critical point data from a CSV file and parses it into CriticalPointDataset objects.
        /// </summary>
        private List<CriticalPointDataset> LoadCriticalPoints(string fullPath)
        {
            string[] lines = null;
            try
            {
                lines = File.ReadAllLines(fullPath);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"LoadCriticalPoints_CsvLoadingError: {e}");
            }

            List<CriticalPointDataset> criticalPointList = new List<CriticalPointDataset>();
            if (lines == null)
                return criticalPointList;

            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(',');

                int id = int.Parse(values[0]);
                int type = int.Parse(values[1]);
                float x = float.Parse(values[2]);
                float y = float.Parse(values[3]);
                float z = float.Parse(values[4]);

                // Only include types within the valid range
                if (type <= 3)
                    criticalPointList.Add(new CriticalPointDataset(id, type, new Vector3(x, y, z)));
            }

            return criticalPointList;
        }

        /// <summary>
        /// Saves a list of gradients to a CSV file using StreamWriter.
        /// </summary>
        public void SaveGradientListToCSV(string filePath, List<GradientDataset> gradientList)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Write header row (optional)
                writer.WriteLine("ID,PositionX,PositionY,PositionZ,DirectionX,DirectionY,DirectionZ,Magnitude");

                // Write each gradient entry in CSV format
                foreach (var gradient in gradientList)
                {
                    writer.WriteLine(
                        $"{gradient.ID}," +
                        $"{gradient.Position.x.ToString(culture)}," +
                        $"{gradient.Position.y.ToString(culture)}," +
                        $"{gradient.Position.z.ToString(culture)}," +
                        $"{gradient.Direction.x.ToString(culture)}," +
                        $"{gradient.Direction.y.ToString(culture)}," +
                        $"{gradient.Direction.z.ToString(culture)}," +
                        $"{gradient.Magnitude.ToString(culture)}"
                    );
                }
            }
        }
    }
}
