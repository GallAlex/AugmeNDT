// /*
//  * MIT License
//  * Copyright (c) 2025 Alexander Gall
//  */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;

namespace AugmeNDT{

    public class DebugLogger : MonoBehaviour
    {


        [Header("Visual Feedback")]
        [Tooltip("Where to output in the Scene")]
        [SerializeField]
        private TMP_Text debugTextField;

        [Tooltip("If you want to see everything being sent to the console.")]
        [SerializeField]
        bool IgnoreLogLevel = true;

        [Tooltip("Show only the specific Log entries. IgnoreLogLevel unchecked")]
        [SerializeField]
        LogType LogLevel = LogType.Log;

        [Tooltip("Maximum number of messages before deleting the older messages.")]
        [SerializeField]
        private int maxNumberOfMessages=  40;

        [Tooltip("Check this if you want the stack trace printed after the message.")]
        [SerializeField]
        private bool includeStackTrace = false;

        [Tooltip("Include stack trace of Log Level printed after the message.")]
        [SerializeField]
        private bool includeLogLevelStackTrace = false;

        [Tooltip("If true, the newest message will be on top. If false, the oldest message will be on top.")]
        [SerializeField]
        private bool newMessageOnTop = true;

        [Header("File Feedback")]
        [Tooltip("Whether to write the logs to a log file.")]
        public bool writeLogFile = false;

        [Tooltip("Path to the log file path.")]
        public string logFilePath;

        [Tooltip("Name of the log file.")]
        public string logFileName = "Log.txt";

        [Tooltip("Flush logs to file every X seconds.")]
        [SerializeField] private float flushInterval = 5f;
        private float timeSinceLastFlush = 0f;

        // The queue with the messages:
        private ConcurrentQueue<string> messageQueue;

        private StreamWriter sw = null;
        private bool newMessageArrived = false;
        // Buffer for new logs
        private List<string> logBuffer = new List<string>();  

        void OnEnable()
        {
            // Check if TMP_Text has been set by User or is available in the GameObject 
            if (debugTextField == null)
            {
                debugTextField = GetComponent<TMP_Text>();

                if (debugTextField == null)
                {
                    throw new NullReferenceException("TMP_Text component for Logger is missing!");
                }
                
            }

            // If empty set the path for the log file to the persistent data directory 
            if (string.IsNullOrEmpty(logFilePath))
            {
                logFilePath = Application.persistentDataPath;
            }

            //Add filename to the path
            logFilePath = Path.Combine(logFilePath, logFileName);

            messageQueue = new ConcurrentQueue<string>();       

            // Triggered regardless of whether the message comes in on the main thread or not -> Handler code has to be thread-safe
            Application.logMessageReceivedThreaded += Application_logMessageReceivedThreaded;

            if (writeLogFile)
            {
                try
                {
                    sw = new StreamWriter(logFilePath, true, Encoding.UTF8) { AutoFlush = true };

                    // Write a new session header
                    string header = $"\n======= Application Start on {DateTime.Now:yyyy-MM-dd HH:mm:ss} =======\n";
                    sw.WriteLine(header);
                    sw.Flush();

                    Debug.Log("Logging to: " + logFilePath);
                }
                catch (IOException e)
                {
                    Debug.LogError("Error opening log file: " + e.Message);
                    writeLogFile = false; // Disable logging on error
                }
            }
        }

        void OnDisable()
        {
            // Write the remaining logs to the file
            if (writeLogFile && logBuffer.Count > 0)
            {
                WriteBufferedLogs();
            }

            Application.logMessageReceivedThreaded -= Application_logMessageReceivedThreaded;

            if (sw != null)
            {
                try
                {
                    sw.Close();
                }
                catch (IOException e)
                {
                    Debug.LogError("Error closing log file: " + e.Message);
                }
            }
        }

        /// <summary>
        /// This Update method checks if a new message has arrived. The check is placed here to ensure
        /// that only the main thread will try to access the Text Mesh.
        /// </summary>

        void Update()
        {
            if (newMessageArrived)
            {
                PrintQueue();

                // Periodic flush mechanism
                if (writeLogFile && logBuffer.Count > 0)
                {
                    timeSinceLastFlush += Time.deltaTime;
                    if (timeSinceLastFlush >= flushInterval)
                    {
                        WriteBufferedLogs();
                        timeSinceLastFlush = 0f;
                    }
                }

                // Reset Flag
                newMessageArrived = false;
            }

        }

        private void Application_logMessageReceivedThreaded(string condition, string stackTrace, LogType type)
        {
            newMessageArrived = true;
            string timestamp = System.DateTime.Now.ToString("HH:mm:ss.fff");

            if (IgnoreLogLevel || type == LogLevel)
            {

                StringBuilder logEntry = new StringBuilder();
                logEntry.Append("\n");

                //Add Color
                string color = ColorUtility.ToHtmlStringRGB(GetTargetColor(type));

                logEntry.Append($"<color=#{color}>{type} at {timestamp}: {condition}");

                // Add stack trace if requested
                if (includeStackTrace && (includeLogLevelStackTrace || type != LogType.Log))
                {
                    logEntry.Append($"\n\nStackTrace: {stackTrace}");
                }

                logEntry.Append("</color>");

                string finalLog = logEntry.ToString();
                messageQueue.Enqueue(finalLog);
                logBuffer.Add($"{timestamp} [{type}] {condition}");

                //Remove old messages if the queue is full
                if (messageQueue.Count > maxNumberOfMessages)
                {
                    messageQueue.TryDequeue(out string dequeueResult);
                }

            }
        }


        /// <summary>
        /// Print the queue to the text mesh.
        /// </summary>
        void PrintQueue()
        {
            StringBuilder stringBuilder = new StringBuilder();

            // if newMessageOnTop is true, we need to reverse the order of the messages
            if (newMessageOnTop)
            {
                var reversedQueue = new List<string>(messageQueue);
                reversedQueue.Reverse();
                foreach (var msg in reversedQueue)
                {
                    stringBuilder.AppendLine(msg);
                }
            }
            else
            {
                // Print the messages in the order they were received
                foreach (var msg in messageQueue)
                {
                    stringBuilder.AppendLine(msg);
                }
            }

            debugTextField.text = stringBuilder.ToString();
        }

        /// <summary>
        /// Write the whole queue to a text file
        /// </summary>
        void WriteBufferedLogs()
        {
            foreach (string log in logBuffer)
            {
                sw.WriteLine(log);
            }
            logBuffer.Clear();

        }


        /// <summary>
        /// Returns the text color for the given LogType
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        Color GetTargetColor(LogType type)
        {
            switch (type)
            {
                case LogType.Error:
                    return Color.red;
                case LogType.Assert:
                    return Color.magenta;
                case LogType.Warning:
                    return Color.yellow;
                case LogType.Log:
                    return Color.white;
                case LogType.Exception:
                    return Color.red;
                default:
                    return Color.white;
            }
        }


    }
}
