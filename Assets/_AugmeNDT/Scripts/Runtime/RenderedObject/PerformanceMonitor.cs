using System.IO;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Profiling;

namespace AugmeNDT
{
    public class PerformanceMonitor : MonoBehaviour
    {
        public bool logToFile = true;
        public float logInterval = 1f;

        private float elapsed = 0.0f;
        private string logPath;

        private Process currentProcess;
        private float lastCpuTime;
        private float lastCheckTime;
        private float cpuUsagePercent;

        void Start()
        {
            if (logToFile)
            {
                string folderPath = Application.dataPath + "/Logs";
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                logPath = folderPath + "/performance_log_"
                          + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";

                File.WriteAllText(logPath, "Time,FPS,Used RAM (MB),CPU (%),TotalRenderers\n");
            }

            // CPU init
            currentProcess = Process.GetCurrentProcess();
            lastCpuTime = (float)currentProcess.TotalProcessorTime.TotalMilliseconds;
            lastCheckTime = Time.realtimeSinceStartup;
        }

        void Update()
        {
            elapsed += Time.deltaTime;

            // CPU calculation (still via System.Diagnostics, Unity has no direct CPU API)
            float currentCpuTime = (float)currentProcess.TotalProcessorTime.TotalMilliseconds;
            float currentTime = Time.realtimeSinceStartup;
            float cpuDelta = currentCpuTime - lastCpuTime;
            float timeDelta = (currentTime - lastCheckTime) * 1000f;

            int coreCount = SystemInfo.processorCount;
            cpuUsagePercent = (cpuDelta / timeDelta) / coreCount * 100f;
            cpuUsagePercent = Mathf.Clamp(cpuUsagePercent, 0f, 100f);

            lastCpuTime = currentCpuTime;
            lastCheckTime = currentTime;

            if (logToFile && elapsed >= logInterval)
            {
                // FPS (Unity üzerinden)
                float fps = 1.0f / Time.unscaledDeltaTime;

                // RAM (Unity Profiler üzerinden, MB cinsinden)
                float ram = Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f);

                // Total rendered objects
                int totalRenderers = FindObjectsOfType<Renderer>().Length;

                string logLine = $"{Time.time:F1},{fps:F1},{ram:F2},{cpuUsagePercent:F1},{totalRenderers}\n";
                File.AppendAllText(logPath, logLine);

                elapsed = 0f;
            }
        }
    }
}
