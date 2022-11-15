using MathNet.Numerics;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AxisTicks
{
    public GameObject tickMarkPrefab;

    public GameObject tickContainer;
    public List<GameObject> tickList;  // Ticks with labels

    public int numberOfTicksToDraw = 2; //Ticks inbetween min/max tick
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
    public void CreateTicks(Transform axisLine, Scale scale)
    {
        if (tickMarkPrefab == null) SetTickMarkStyle();

        tickContainer = new GameObject("Ticks");
        tickContainer.transform.parent = axisLine;

        tickList = new List<GameObject>();

        if (numberOfTicksToDraw <= 0) return;

        float tickSpacing = GetTickSpacing(numberOfTicksToDraw);

        // tick from min to max value
        for (int tick = 0; tick <= numberOfTicksToDraw; tick++)
        {
            float step = tick * tickSpacing;
            float scaledValue = scale.GetDomainValue(step);

            Vector3 tickPosition = new Vector3(step + tickContainer.transform.localPosition.x, tickContainer.transform.localPosition.y, tickContainer.transform.localPosition.z);


            tickList.Add(CreateSingleTick(tick, tickPosition, scaledValue));

        }
    }

    /// <summary>
    /// Method changes position and text of already generated ticks (tickList) or creates new ones, based on new scale.
    /// If the number of ticks is less then before the additional ticks are set inactive. If the number of ticks is greater then before new ticks are generated.
    /// </summary>
    /// <param name="scale"></param>
    /// <param name="numberOfTicks"></param>
    public void ChangeTicks(Scale scale)
    {
        float tickSpacing = GetTickSpacing(numberOfTicksToDraw);

        if (numberOfTicksToDraw == 0 && tickList.Count == 0) return;

        for (int tick = 0; tick < Math.Max(numberOfTicksToDraw, tickList.Count); tick++)
        {
            float step = tick * tickSpacing;
            float scaledValue = scale.GetDomainValue(step);

            var currTickPosition = new Vector3(step + tickContainer.transform.localPosition.x, tickContainer.transform.localPosition.y,
                tickContainer.transform.localPosition.z);

            if (numberOfTicksToDraw <= tickList.Count)
            {
                if (tick <= numberOfTicksToDraw && numberOfTicksToDraw > 0)
                {
                    // Change Pos and label and set tick active
                    MoveAndRenameSingleTick(tickContainer.transform, tick, currTickPosition, scaledValue);
                }
                else
                {
                    tickList[tick].SetActive(false);
                }

            }
            else if (numberOfTicksToDraw > tickList.Count)
            {
                if (tick < tickList.Count)
                {
                    // Change Pos and label and set tick active
                    MoveAndRenameSingleTick(tickContainer.transform, tick, currTickPosition, scaledValue);
                }
                else
                {

                    //GameObject tickInstance = GameObject.Instantiate(tickMarkPrefab, currTickPosition, Quaternion.identity, tickContainer.transform);
                    //var tickLabel = tickInstance.GetComponentInChildren<TextMesh>();
                    //tickLabel.text = scaledValue.Round(decimalPoints).ToString();
                    //tickInstance.name = "Tick " + tick;
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
    private void MoveAndRenameSingleTick(Transform axisLine, int tickID, Vector3 newPos, float scaledValue)
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
    private GameObject CreateSingleTick(int tickID, Vector3 newPos, float scaledValue)
    {
        GameObject tickInstance = GameObject.Instantiate(tickMarkPrefab, newPos, Quaternion.identity, tickContainer.transform);
        tickInstance.name = "Tick " + tickID;

        TextMesh tickLabel = tickInstance.GetComponentInChildren<TextMesh>();
        tickLabel.text = scaledValue.Round(decimalPoints).ToString();

        return tickInstance;
    }

    /// <summary>
    /// Method sets the amount of ticks that should be drawn.
    /// If numberOfTicks <= 0, then no tick will be drawn.
    /// If numberOfTicks > 0 then that many ticks inbetween min tick and max tick are drawn
    /// Example: numberOfTicks = 1 -> then 3 ticks will be drawn
    /// </summary>
    /// <param name="numberOfTicks"></param>
    public void SetNumberOfTicksToDraw(int numberOfTicks)
    {
        if (numberOfTicks <= 0) numberOfTicksToDraw = 0;
        else numberOfTicksToDraw = numberOfTicks + 1;
    }

    /// <summary>
    /// Calculates the distance between ticks based on the lenght of the Axis and the number of Ticks (for that range)
    /// The Axis length is assumed to be 1 Unity Unit.
    /// </summary>
    /// <param name="numberOfTicks"></param>
    /// <returns></returns>
    private float GetTickSpacing(int numberOfTicks)
    {
        if (numberOfTicksToDraw <= 0) return -1;

        return 1.0f / (float)numberOfTicks;
    }

}
