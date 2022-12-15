using MathNet.Numerics;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AxisTicks
{
    public GameObject tickMarkPrefab;

    public GameObject tickContainer;
    public List<GameObject> tickList;  // Ticks with labels
    //private ObjectPool<GameObject> tickPool;

    public int decimalPoints = 4;

    public void SetTickMarkStyle()
    {
        if (tickMarkPrefab == null) tickMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/VisContainer/Tick");
    }

    /// <summary>
    /// Method creates 
    /// </summary>
    /// <param name="axisLine"></param>
    /// <param name="scale"></param>
    /// <param name="numberOfTicks"></param>
    public void CreateTicks(Transform axisLine, Scale scale, int numberOfTicks)
    {
        if (tickMarkPrefab == null) SetTickMarkStyle();

        tickContainer = new GameObject("Ticks");
        tickContainer.transform.parent = axisLine;

        tickList = new List<GameObject>();

        //Todo: numberOfTicks == 1 should be 2

        // if scale is scale.linear, then tickSpacing is calculated
        double tickSpacing = GetTickSpacing(numberOfTicks);
        // if scale is scale.ordinal, then tickSpacing is 1
        //tickSpacing = 1.0f;
        

        //Debug.Log("Domain " + scale.domain[0] + " to " + scale.domain[1]);
        //Debug.Log("Range " + scale.range[0] + " to " + scale.range[1]);
        
        // tick from min to max value
        for (int tick = 0; tick < numberOfTicks; tick++)
        {
            double step = tick * tickSpacing;
            double scaledValue = scale.GetDomainValue(step);
            //Debug.Log("tickPos Nr " + tick + ": " + step);

            //Debug.Log("Value is " + scaledValue);

            Vector3 tickPosition = new Vector3((float)step + tickContainer.transform.localPosition.x, tickContainer.transform.localPosition.y, tickContainer.transform.localPosition.z);


            tickList.Add(CreateSingleTick(tick, tickPosition, scaledValue));

        }
    }

    /// <summary>
    /// Method changes position and text of already generated ticks (tickList) or creates new ones, based on new scale.
    /// If the number of ticks is less then before the additional ticks are set inactive. If the number of ticks is greater then before new ticks are generated.
    /// </summary>
    /// <param name="scale"></param>
    /// <param name="numberOfTicks"></param>
    public void ChangeTicks(Scale scale, int numberOfTicks)
    {
        double tickSpacing = GetTickSpacing(numberOfTicks);
        int ticksInList = tickList.Count;

        if (numberOfTicks == 0 && ticksInList == 0) return;

        for (int tick = 0; tick < Math.Max(numberOfTicks, ticksInList); tick++)
        {
            double step = tick * tickSpacing;
            double scaledValue = scale.GetDomainValue(step);

            
            var currTickPosition = new Vector3((float)step + tickContainer.transform.localPosition.x, tickContainer.transform.localPosition.y,
                tickContainer.transform.localPosition.z);

            // If there are more ticks in the list then needed, move the ones needed and set the additional ones inactive
            if (numberOfTicks <= ticksInList-1)
            {
                if (tick < numberOfTicks)
                {
                    // Change Pos and label and set tick active
                    MoveAndRenameSingleTick(tick, currTickPosition, scaledValue);
                }
                else
                {
                    tickList[tick].SetActive(false);
                }

            }
            // If there are less ticks in the list then needed, move existing ones and create new ones as needed
            else if (numberOfTicks >= ticksInList-1)
            {
                if (tick < ticksInList)
                {
                    // Change Pos and label and set tick active
                    MoveAndRenameSingleTick(tick, currTickPosition, scaledValue);
                }
                else
                {
                    // Create new tick
                    tickList.Add(CreateSingleTick(tick, currTickPosition, scaledValue));
                }
            }

        }
    }

    /// <summary>
    /// Method moves and renames an already created Tick in the list and set it active.
    /// It places the tick at the given position and renames the label to the provided value.
    /// </summary>
    /// <param name="tick">Gameobject of Tick with label</param>
    /// <param name="newPos">New position of the tick</param>
    /// <param name="scaledValue">Value which should be displayed as label</param>
    private void MoveAndRenameSingleTick(int tickID, Vector3 newPos, double scaledValue)
    {
        var tickObj = tickList[tickID];
        tickObj.SetActive(true);

        var label = tickObj.GetComponentInChildren<TextMesh>();
        tickObj.transform.localPosition = newPos;
        label.text = scaledValue.Round(decimalPoints).ToString();
    }

    /// <summary>
    /// Method creates a Tick with its position and the scaled value to display
    /// </summary>
    /// <param name="tickID">Used as name for the Gameobject</param>
    /// <param name="newPos"></param>
    /// <param name="scaledValue"></param>
    /// <returns>Returns a new Tick</returns>
    private GameObject CreateSingleTick(int tickID, Vector3 newPos, double scaledValue)
    {
        //GameObject tickInstance = GameObject.Instantiate(tickMarkPrefab, newPos, Quaternion.identity, tickContainer.transform);
        GameObject tickInstance = GameObject.Instantiate(tickMarkPrefab, tickContainer.transform, false);
        tickInstance.transform.localPosition = newPos;
        tickInstance.name = "Tick " + tickID;

        TextMesh tickLabel = tickInstance.GetComponentInChildren<TextMesh>();
        tickLabel.text = scaledValue.Round(decimalPoints).ToString();

        return tickInstance;
    }

    /// <summary>
    /// Calculates the distance between ticks based on the lenght of the Axis and the number of Ticks (for that range)
    /// The Axis length is assumed to be 1 Unity Unit.
    /// </summary>
    /// <param name="numberOfTicks"></param>
    /// <returns></returns>
    private double GetTickSpacing(int numberOfTicks)
    {
        if (numberOfTicks <= 0) return 0;
        int numberOfSpacings = numberOfTicks - 1;
        
        return 1.0f / (double)numberOfSpacings;
    }

}
