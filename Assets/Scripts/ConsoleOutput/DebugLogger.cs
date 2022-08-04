using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshPro))]
public class DebugLogger : MonoBehaviour
{
    [Header("Visual Feedback")]
    [Tooltip("If you want to see everything being sent to the console.")]
    [SerializeField]
    bool IgnoreLogLevel;

    [Tooltip("Show only the specific Log entries.")]
    [SerializeField]
    LogType LogLevel;

    [Tooltip("Maximum number of messages before deleting the older messages.")]
    [SerializeField]
    private int maxNumberOfMessages=15;

    [Tooltip("Check this if you want the stack trace printed after the message.")]
    [SerializeField]
    private bool includeStackTrace=false;

    [Tooltip("Include stack trace of Log Level printed after the message.")]
    [SerializeField]
    private bool includeLogLevelStackTrace = false;

    [Header("Auditory Feedback")]
    [Tooltip("Play a sound when the message panel is updated.")]
    [SerializeField]
    private bool playSoundOnMessage;

    private bool newMessageArrived = false;

    private TextMeshPro debugText;

    // The queue with the messages:
    private Queue<string> messageQueue;

    // The message sound, should you use one
    private AudioSource messageSound;

    void OnEnable()
    {
        messageQueue = new Queue<string>();       
        debugText = GetComponent<TextMeshPro>();
        Application.logMessageReceivedThreaded += Application_logMessageReceivedThreaded;
        messageSound = this.GetComponent<AudioSource>();
    }
   

    private void Application_logMessageReceivedThreaded(string condition, string stackTrace, LogType type)
    {
        string timestamp = System.DateTime.Now.ToString("HH:mm:ss.fff");
        if (IgnoreLogLevel || type == LogLevel)
        {

            if (messageSound!=null && playSoundOnMessage)
            {
                messageSound.Play();
            }

            newMessageArrived = true;

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("\n");

            //Add Color
            string colorString = ColorUtility.ToHtmlStringRGB(getTargetColor(type));

            stringBuilder.Append("<color=#").Append(colorString).Append(">");
            stringBuilder.Append(type.ToString()).Append(" at <" + timestamp + ">: ");

            stringBuilder.Append(condition);

            if (includeStackTrace)
            {
                //Skip if Log StackTrace not needed
                if (type == LogType.Log && includeLogLevelStackTrace)
                {
                    stringBuilder.Append("\nStackTrace: ");
                    stringBuilder.Append(stackTrace);
                }
                if (type != LogType.Log)
                {
                    stringBuilder.Append("\nStackTrace: ");
                    stringBuilder.Append(stackTrace);
                }

            }

            stringBuilder.Append(" </color>");

            condition = stringBuilder.ToString();
            messageQueue.Enqueue(condition);
        
            if (messageQueue.Count > maxNumberOfMessages)
            {
                messageQueue.Dequeue();
            }
        }
    }

    void OnDisable()
    {
        Application.logMessageReceivedThreaded -= Application_logMessageReceivedThreaded;
    }

    /// <summary>
    /// Print the queue to the text mesh.
    /// </summary>

    void PrintQueue()
    {
        StringBuilder stringBuilder = new StringBuilder();
        string[] messageList = messageQueue.ToArray();

        //for (int i = 0; i < messageList.Length; i++)
        for (int i = messageList.Length - 1; i >= 0; i--) 
        { 
            stringBuilder.Append(messageList[i]);
            stringBuilder.Append("\n");
        }        

        string message = stringBuilder.ToString();
        debugText.text = message;
    }

    /// <summary>
    /// Returns the text color for the given LogType
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    Color getTargetColor(LogType type)
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

    /// <summary>
    /// This Update method checks if a new message has arrived. The check is placed here to ensure
    /// that only the main thread will try to access the Text Mesh.
    /// </summary>

    void Update()
    {
        if (newMessageArrived)
        {
            PrintQueue();
            newMessageArrived = false;
        }
    }
}
