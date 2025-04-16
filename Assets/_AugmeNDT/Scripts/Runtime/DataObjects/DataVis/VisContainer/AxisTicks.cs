using System;
using System.Collections.Generic;
using MathNet.Numerics;
using UnityEngine;
using TMPro;

namespace AugmeNDT{

    public class AxisTicks
    {
        public GameObject tickMarkPrefab;

        public GameObject tickContainer;
        public List<GameObject> tickList;  // Ticks with labels
        //private ObjectPool<GameObject> tickPool;

        private double[] axisStartEndPoints;
        private float tickOffset = 0;
        private int decimalPoints = 4;

        //Todo: Axis needs to  get the Values and makes a scale between Start/End of Axis (0,0)-(1,0)

        /// <summary>
        /// Initialize the Axis for the Ticks with StartPoint (0.0f, 0.0f, 0.0f) and Endpoint (1.0f, 0.0f, 0.0f) and Offset of zero.
        /// </summary>
        public AxisTicks()
        {
            tickOffset = 0.1f;

            double startPoint = 0.0f + tickOffset;
            double endPoint = 1.0f - tickOffset;
            axisStartEndPoints = new double[] { startPoint, endPoint };
        }

        /// <summary>
        /// Sets the Axis properties like the Offset from Start-/Endoint of the Line for the creation of Ticks.
        /// Default Value for the StartPoint (0.0f, 0.0f, 0.0f) and Endpoint (1.0f, 0.0f, 0.0f) are assumed.
        /// </summary>
        /// <param name="tickOffset"></param>
        public void SetAxisProperties(float tickOffset)
        {
            this.tickOffset = tickOffset;
            axisStartEndPoints = new double[] { 0.0f + tickOffset, 1.0f - tickOffset };
        }

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

            double tickSpacing = GetOffsetTickSpacing(numberOfTicks);       // increment

            // tick from min to max value
            for (int tick = 0; tick < numberOfTicks; tick++)
            {
                double step = StepFunction(tick, tickSpacing, axisStartEndPoints[0]);
                double domainValue = scale.GetDomainValue(step);

                Vector3 tickPosition = new Vector3((float)step + tickContainer.transform.localPosition.x, tickContainer.transform.localPosition.y, tickContainer.transform.localPosition.z);

                tickList.Add(CreateSingleTick(tick, tickPosition, domainValue, scale));
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
            double tickSpacing = GetOffsetTickSpacing(numberOfTicks);       // increment
            int ticksInList = tickList.Count;

            if (numberOfTicks == 0 && ticksInList == 0) return;

            for (int tick = 0; tick < Math.Max(numberOfTicks, ticksInList); tick++)
            {
                double step = StepFunction(tick, tickSpacing, axisStartEndPoints[0]);
                double domainValue = scale.GetDomainValue(step);

            
                var currTickPosition = new Vector3((float)step + tickContainer.transform.localPosition.x, tickContainer.transform.localPosition.y,
                    tickContainer.transform.localPosition.z);

                // If there are more ticks in the list then needed, move the ones needed and set the additional ones inactive
                if (numberOfTicks <= ticksInList-1)
                {
                    if (tick < numberOfTicks)
                    {
                        // Change Pos and label and set tick active
                        MoveAndRenameSingleTick(tick, currTickPosition, domainValue, scale);
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
                        MoveAndRenameSingleTick(tick, currTickPosition, domainValue, scale);
                    }
                    else
                    {
                        // Create new tick
                        tickList.Add(CreateSingleTick(tick, currTickPosition, domainValue, scale));
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
        /// <param name="domainValue">Value which should be displayed as label</param>
        private void MoveAndRenameSingleTick(int tickID, Vector3 newPos, double domainValue, Scale scale)
        {
            var tickObj = tickList[tickID];
            tickObj.SetActive(true);

            var label = tickObj.GetComponentInChildren<TextMeshPro>();
            tickObj.transform.localPosition = newPos;

            if (scale.dataScaleType == Scale.DataScaleType.Linear)
            {
                label.text = domainValue.Round(decimalPoints).ToString();
            }
            else if (scale.dataScaleType == Scale.DataScaleType.Nominal)
            {
                label.text = scale.GetDomainValueName(domainValue);
            }
        
        }

        /// <summary>
        /// Method creates a Tick with its position and the scaled value to display
        /// </summary>
        /// <param name="tickID">Used as name for the Gameobject</param>
        /// <param name="newPos"></param>
        /// <param name="domainValue"></param>
        /// <returns>Returns a new Tick</returns>
        private GameObject CreateSingleTick(int tickID, Vector3 newPos, double domainValue, Scale scale)
        {
            //GameObject tickInstance = GameObject.Instantiate(tickMarkPrefab, newPos, Quaternion.identity, tickContainer.transform);
            GameObject tickInstance = GameObject.Instantiate(tickMarkPrefab, tickContainer.transform, false);
            tickInstance.transform.localPosition = newPos;
            tickInstance.name = "Tick " + tickID;

            TextMeshPro tickLabel = tickInstance.GetComponentInChildren<TextMeshPro>();

            if (scale.dataScaleType == Scale.DataScaleType.Linear)
            {
                tickLabel.text = domainValue.Round(decimalPoints).ToString();
            }
            else if (scale.dataScaleType == Scale.DataScaleType.Nominal)
            {
                tickLabel.text = scale.GetDomainValueName(domainValue);
            }

            return tickInstance;
        }


        /// <summary>
        /// Calculates the distance (increment) between ticks based on the lenght of the Axis (axisStartEndPoints) and the number of Ticks (for that range)
        /// The Axis length is assumed to be 1 Unity Unit [starting at (0,0,0) and endinf at (1,0,0)].
        /// If only one tick is needed the increment is set to halfe the length of the Axis.
        /// </summary>
        /// <param name="numberOfTicks"></param>
        /// <returns></returns>
        private double GetOffsetTickSpacing(int numberOfTicks)
        {
            int numberOfSpacings = 0;

            if (numberOfTicks <= 0) return 0;
            if (numberOfTicks == 1) numberOfSpacings = 2;
            else numberOfSpacings = numberOfTicks - 1;

            //return the increment between ticks
            return (axisStartEndPoints[1] - axisStartEndPoints[0]) / (double)numberOfSpacings;
        }

        /// <summary>
        /// Step function which calculates the draw point for the tick (in 2D).
        /// startPos is therefore a x pos on the line
        /// </summary>
        /// <param name="currentTick"></param>
        /// <param name="tickSpacing"></param>
        /// <param name="startPos"></param>
        /// <returns></returns>
        private double StepFunction(int currentTick, double tickSpacing, double startPos)
        {
            double step = (currentTick * tickSpacing) + startPos;
            return step;
        }

    }
}
