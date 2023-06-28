using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace AugmeNDT{
    /// <summary>
    /// Creates the Unit Container, Axes, and DataMarks for 2D/3D Visualizations
    /// </summary>
    public class VisContainer
    {
        public GameObject anchoredContainerPrefab;
        public GameObject anchoredContainer;
        public GameObject visContainer;
        public GameObject axisContainer;
        public GameObject gridContainer;
        public GameObject dataMarkContainer;
        public GameObject colorLegendContainer;
        public GameObject dataMarkAxisLineContainer;

        // Main Container Elements
        public List<DataAxis> dataAxisList;         // Axes with Ticks
        public List<DataGrid> dataGridList;         // Grids
        public List<DataMark> dataMarkList;         // Data Marks
        public List<DataMarkAxisLine> dataMarkAxisLineList; // Lines between Data Mark and Axis

        // DataValues
        public Dictionary<VisChannel, double[]> channelValues;  // Data Values for each Channel
        public Dictionary<VisChannel, Scale> channelScale;      // Scaling for each used Channel
        public bool specificColorsSet = false;
        public Color[] specificColorChannel;                    // Color channel filled with specific colors
        private int dataMarkCount = 0;

        // Interactor
        private VisInteractor visInteractor;

        private Bounds containerBounds;              // Width, Height, Length of the Container
        private ContainerHandle containerHandle;                     // Stores reference to the Container Handle
        private float[] xyzOffset;
        private int[] xyzTicks;
        private Color[] colorScheme;

        #region CREATION OF ELEMENTS

        public GameObject CreateVisContainer(string visName)
        {
            // Define Container Bounds
            containerBounds.center = new Vector3(0.5f, 0.5f, 0.5f);
            containerBounds.size = new Vector3(1f, 1f, 1f);

            // Initialize Lists
            dataAxisList = new List<DataAxis>();
            dataGridList = new List<DataGrid>();
            dataMarkList = new List<DataMark>();
            dataMarkAxisLineList = new List<DataMarkAxisLine>();

            channelValues = new Dictionary<VisChannel, double[]>();
            channelScale = new Dictionary<VisChannel, Scale>();

            anchoredContainerPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/VisContainer/AnchoredContainer");
            anchoredContainer = GameObject.Instantiate(anchoredContainerPrefab, anchoredContainerPrefab.transform.position, Quaternion.identity);
            anchoredContainer.name = visName;

            containerHandle = anchoredContainer.GetComponent<ContainerHandle>();

            //Create Hierarchy (Empty GameObjects)
            visContainer = new GameObject("VisContainer");
            colorLegendContainer = new GameObject("Color Legend");

            axisContainer = new GameObject("Axes");
            gridContainer = new GameObject("Grids");
            dataMarkContainer = new GameObject("Data Marks");
            dataMarkAxisLineContainer = new GameObject("Data Marks Axis Line");

            //Add Childs
            visContainer.transform.parent = anchoredContainer.transform;
            colorLegendContainer.transform.parent = anchoredContainer.transform;

            axisContainer.transform.parent = visContainer.transform;
            gridContainer.transform.parent = visContainer.transform;
            dataMarkContainer.transform.parent = visContainer.transform;
            dataMarkAxisLineContainer.transform.parent = visContainer.transform;

            return anchoredContainer;
        }

        public void DeleteVisContainer()
        {
            GameObject.Destroy(colorLegendContainer);
            GameObject.Destroy(visContainer);
            GameObject.Destroy(anchoredContainer);
        }

        public void SetTitle(string title)
        {
            anchoredContainer.name = title;
        }

        public void SetHandleText(string text)
        {
            containerHandle.SetSideText(text);
        }

        /// <summary>
        /// Removes the Handle of the Anchored Container
        /// </summary>
        public void RemoveContainerHandle()
        {
            containerHandle.Remove();
        }

        /// <summary>
        /// Method to create a new Axis for a Direction (X/Y/Z), and add it to the list of DataAxis.
        /// The scaling of the axis is calculated by the min and max values of the data.
        /// The values and the Scale are stored in the channelValues and channelScale Dictionary.
        /// </summary>
        /// <param name="axisLabel"></param>
        /// <param name="minMaxValues"></param>
        /// <param name="axisDirection"></param>
        public void CreateAxis(string axisLabel, double[] minMaxValues, Direction axisDirection)
        {
            DataAxis axis = new DataAxis(xyzOffset[(int)axisDirection]);
            //Return Length of current axis
            Scale scale = CreateAxisScale(minMaxValues, xyzOffset[(int)axisDirection]);

            axis.CreateAxis(axisContainer.transform, axisLabel, scale, axisDirection, xyzTicks[(int)axisDirection]);

            dataAxisList.Add(axis);
        }

        /// <summary>
        /// Method to create a new Axis for a Direction (X/Y/Z), and add it to the list of DataAxis.
        /// The scaling of the axis is calculated by the amount of attribute names.
        /// The values and the Scale are stored in the channelValues and channelScale Dictionary.
        /// </summary>
        /// <param name="axisLabel"></param>
        /// <param name="attributeNames"></param>
        /// <param name="axisDirection"></param>
        public void CreateAxis(string axisLabel, string[] attributeNames, Direction axisDirection)
        {
            DataAxis axis = new DataAxis(xyzOffset[(int)axisDirection]);
            //Return Length of current axis
            Scale scale = CreateAxisScale(attributeNames, xyzOffset[(int)axisDirection]);

            axis.CreateAxis(axisContainer.transform, axisLabel, scale, axisDirection, xyzTicks[(int)axisDirection]);

            dataAxisList.Add(axis);
        }

        //TODO: Create multiple grids on different positions (at tick position?)
        public void CreateGrid(Direction axis1, Direction axis2)
        {
            DataGrid grid = new DataGrid();
            Direction[] axisDirections = { axis1, axis2 };

            grid.CreateGrid(gridContainer.transform, containerBounds, xyzOffset[(int)axis1], xyzOffset[(int)axis2], axisDirections, xyzTicks[(int)axis1], xyzTicks[(int)axis2]);

            dataGridList.Add(grid);
        }

        public void CreateDataMarks(GameObject markPrefab, int[] pivotInCenter)
        {
            //int dataMarkCount = channelValues.ElementAt(1).Value.Length;
            CheckChannelDataValueCount();

            for (int mark = 0; mark < dataMarkCount; mark++)
            {
                //TODO: Performance of adding prefab every time?
                DataMark dataMark = new DataMark(dataMarkList.Count, markPrefab);
                dataMark.SetVisInteractor(visInteractor);

                //Create Values
                DataMark.Channel channel = DataMark.DefaultDataChannel();
                channel.pivotPointCenter = pivotInCenter;
                channel = GetDataMarkChannelValues(channel, mark);

                dataMark.CreateDataMark(dataMarkContainer.transform, channel);
                dataMarkList.Add(dataMark);
            }
        }

        /// <summary>
        /// Adds the given color legend as gameobject to the colorLegendContainer.
        /// </summary>
        /// <param name="legend"></param>
        public virtual void CreateColorLegend(GameObject legend)
        {
            legend.transform.parent = colorLegendContainer.transform;

            //TODO: Set specific docking positions for elements (legend,...)?
            //Move the legend to the right edge of the container (+ half of the width of the legend)
            colorLegendContainer.transform.localPosition = new Vector3(containerBounds.size.x + legend.transform.localScale.x / 2.0f, containerBounds.size.y / 2.0f, containerBounds.size.z / 2.0f);
            colorLegendContainer.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }

        public void CreateDataMarkAxisLine(int dataMarkId)
        {
            DataMarkAxisLine markLine = new DataMarkAxisLine();

            markLine.DrawAxisLine(dataMarkAxisLineContainer.transform, containerBounds, dataMarkList[dataMarkId]);

            dataMarkAxisLineList.Add(markLine);
        }

        #endregion

        #region Channels

        public void SetChannel(VisChannel visChannel, double[] dataValues)
        {
            switch (visChannel)
            {
                case VisChannel.XPos:
                    channelScale.Add(visChannel, GetAxisScale(Direction.X));  //Uses the X-Axis Scale
                    channelValues.Add(visChannel, dataValues);
                    break;
                case VisChannel.YPos:
                    channelScale.Add(visChannel, GetAxisScale(Direction.Y));  //Uses the Y-Axis Scale
                    channelValues.Add(visChannel, dataValues);
                    break;
                case VisChannel.ZPos:
                    channelScale.Add(visChannel, GetAxisScale(Direction.Z));  //Uses the Z-Axis Scale
                    channelValues.Add(visChannel, dataValues);
                    break;
                case VisChannel.XSize:
                    //channelScale.Add(visChannel, CreateSizeScale(dataValuesAxis, NEUE RANGE));
                    channelScale.Add(visChannel, CreateSizeScale(GetAxisScale(Direction.X).domain.ToArray(), xyzOffset[(int)Direction.X]));
                    channelValues.Add(visChannel, dataValues);
                    break;
                case VisChannel.YSize:
                    channelScale.Add(visChannel, CreateSizeScale(GetAxisScale(Direction.Y).domain.ToArray(), xyzOffset[(int)Direction.Y]));
                    channelValues.Add(visChannel, dataValues);
                    break;
                case VisChannel.ZSize:
                    channelScale.Add(visChannel, CreateSizeScale(GetAxisScale(Direction.Z).domain.ToArray(), xyzOffset[(int)Direction.Z]));
                    channelValues.Add(visChannel, dataValues);
                    break;
                case VisChannel.XRotation:
                case VisChannel.YRotation:
                case VisChannel.ZRotation:
                    //Todo: Rotation [0, 359] so that min != max
                    channelScale.Add(visChannel, GetChannelScale(dataValues, new[] { 0.0, 360.0 }));
                    channelValues.Add(visChannel, dataValues);
                    break;
                case VisChannel.Color:
                    channelValues.Add(visChannel, dataValues);
                    break;
                default:
                    break;
            }

        }

        public void SetSpecificColor(Color[] colorValues)
        {
            specificColorsSet = true;
            specificColorChannel = colorValues;
        }

        public DataMark.Channel GetDataMarkChannelValues(DataMark.Channel channel, int valueIndex)
        {
            //Fill DataMark for every Channel which has Data set
            foreach (var setChannel in channelValues)
            {
                switch (setChannel.Key)
                {
                    case VisChannel.XPos:
                        channel.position.x = (float)channelScale[setChannel.Key].GetScaledValue(setChannel.Value[valueIndex]);
                        break;
                    case VisChannel.YPos:
                        channel.position.y = (float)channelScale[setChannel.Key].GetScaledValue(setChannel.Value[valueIndex]);
                        break;
                    case VisChannel.ZPos:
                        channel.position.z = (float)channelScale[setChannel.Key].GetScaledValue(setChannel.Value[valueIndex]);
                        break;
                    case VisChannel.XSize:
                        channel.size.x = (float)channelScale[setChannel.Key].GetScaledValue(setChannel.Value[valueIndex]);
                        break;
                    case VisChannel.YSize:
                        channel.size.y = (float)channelScale[setChannel.Key].GetScaledValue(setChannel.Value[valueIndex]);
                        break;
                    case VisChannel.ZSize:
                        channel.size.z = (float)channelScale[setChannel.Key].GetScaledValue(setChannel.Value[valueIndex]);
                        break;
                    case VisChannel.XRotation:
                        channel.rotation.x = (float)channelScale[setChannel.Key].GetScaledValue(setChannel.Value[valueIndex]);
                        break;
                    case VisChannel.YRotation:
                        channel.rotation.y = (float)channelScale[setChannel.Key].GetScaledValue(setChannel.Value[valueIndex]);
                        break;
                    case VisChannel.ZRotation:
                        channel.rotation.z = (float)channelScale[setChannel.Key].GetScaledValue(setChannel.Value[valueIndex]);
                        break;
                    case VisChannel.Color:
                        if (specificColorsSet) channel.color = specificColorChannel[valueIndex];
                        else channel.color = ScaleColor.GetInterpolatedColor(setChannel.Value[valueIndex], setChannel.Value.Min(), setChannel.Value.Max(), colorScheme);
                        break;
                    default:
                        break;
                }
            }

            return channel;
        }

        /// <summary>
        /// The Method gathers information about all values set for the DataMarks and return their equivalent value sin the Dataset
        /// </summary>
        public void GatherDataMarkValueInformation(int dataMarkId)
        {
            string info = "DataMark " + dataMarkId + ":\n";

            //Gather all set Channels
            foreach (var setChannel in channelValues)
            {
                if(setChannel.Key == VisChannel.Color) break;
                //string value = channelScale[setChannel.Key].GetDomainValueName(setChannel.Value[dataMarkId]);
                double value = setChannel.Value[dataMarkId];
                info += setChannel.Key + ": " + value + "\n";
            }

            Debug.Log(info);
        }

        #endregion


        #region CHANGE OF ELEMENTS

        public void ChangeAxis(int axisID, string axisLabel, Scale dataScale, int numberOfTicks)
        {
            if (axisID < 0 || axisID > dataAxisList.Count)
            {
                Debug.LogError("Selected axis does not exist");
                return;
            }

            dataAxisList[axisID].ChangeAxis(axisLabel, dataScale, numberOfTicks);

        }

        public void ChangeDataMarks()
        {
            if (dataMarkList.Count <= 0)
            {
                Debug.LogError("No data mark present");
                return;
            }

            for (var markID = 0; markID < dataMarkList.Count; markID++)
            {
                DataMark.Channel channel = DataMark.DefaultDataChannel();
                var dataMark = dataMarkList[markID];

                channel = GetDataMarkChannelValues(channel, markID);
                dataMark.ChangeDataMark(channel);
            }
        }

        public void ChangeDataMark(int dataMarkID, DataMark.Channel channel)
        {
            if (dataMarkID < 0 || dataMarkID > dataMarkList.Count)
            {
                Debug.LogError("Data Mark does not exist");
                return;
            }

            dataMarkList[dataMarkID].ChangeDataMark(channel);

        }

        public void HideColorLegend(GameObject legend)
        {

        }

        #endregion

        /// <summary>
        /// Returns the current applied Scale for the selected Axis
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        public Scale GetAxisScale(Direction axis)
        {
            if (dataAxisList.Count < (int)axis + 1)
            {
                Debug.LogError("Selected Axis is not created");
                return null;
            }

            return dataAxisList[(int)axis].dataScale;
        }

        public void SetColorScheme(Color[] colorScheme)
        {
            this.colorScheme = colorScheme;
        }

        /// <summary>
        /// Defines the tick offest for every Axis
        /// Moves the first tick from the axis origin and the last tick from the end of the axis by by the offset
        /// </summary>
        /// <param name="xyzOffset"></param>
        public void SetAxisOffsets(float[] xyzOffset)
        {
            this.xyzOffset = xyzOffset;
        }

        /// <summary>
        /// Sets the amount of Ticks for each Axis
        /// </summary>
        /// <param name="xyzTicks"></param>
        public void SetAxisTickNumber(int[] xyzTicks)
        {
            this.xyzTicks = xyzTicks;
        }

        // Sets the Interactor of the respective Vis
        public void SetVisInteractor(VisInteractor interactor)
        {
            visInteractor = interactor;
        }

        public void SetContainerBounds(Bounds cBounds)
        {
            containerBounds = cBounds;
        }

        private Vector3 GetCenterOfVisContainer()
        {
            Vector3 center = visContainer.transform.position + visContainer.transform.lossyScale / 2f;
            return center;
        }

        /// <summary>
        /// Method moves the Grid in the VisContainer side which is further away from the camera
        /// For this the Container is viewed as consiting out of 8 octants. 
        /// </summary>
        public void MoveGridBasedOnViewingDirection()
        {
            if (dataGridList.Count < 3) return;

            Vector3 center = GetCenterOfVisContainer();
            Vector3 cDir = Camera.main.transform.position;

            // Calculate in which octant  (8 possibilities) of the cube the camera is located
            // >-Bottom->  --+  |  +-+   >-Top->  -++  |  +++
            // >-Bottom->  ---  |  +--   >-Top->  -+-  |  ++-

            //## Bottom Part ##
            // --+
            if (cDir.x < center.x && cDir.y < center.y && cDir.z > center.z)
            {
                //XY
                dataGridList[0].GetGridObject().transform.localPosition = new Vector3(0, 0, 0);
                //YZ
                dataGridList[1].GetGridObject().transform.localPosition = new Vector3(1, 0, 0);
                //XZ
                dataGridList[2].GetGridObject().transform.localPosition = new Vector3(0, 1, 0);
            }
            //+-+
            else if (cDir.x > center.x && cDir.y < center.y && cDir.z > center.z)
            {
                //XY
                dataGridList[0].GetGridObject().transform.localPosition = new Vector3(0, 0, 0);
                //YZ
                dataGridList[1].GetGridObject().transform.localPosition = new Vector3(0, 0, 0);
                //XZ
                dataGridList[2].GetGridObject().transform.localPosition = new Vector3(0, 1, 0);
            }
            //---
            else if (cDir.x < center.x && cDir.y < center.y && cDir.z < center.z)
            {
                //XY
                dataGridList[0].GetGridObject().transform.localPosition = new Vector3(0, 0, 1);
                //YZ
                dataGridList[1].GetGridObject().transform.localPosition = new Vector3(1, 0, 0);
                //XZ
                dataGridList[2].GetGridObject().transform.localPosition = new Vector3(0, 1, 0);
            }
            //+--
            else if (cDir.x > center.x && cDir.y < center.y && cDir.z < center.z)
            {
                //XY
                dataGridList[0].GetGridObject().transform.localPosition = new Vector3(0, 0, 1);
                //YZ
                dataGridList[1].GetGridObject().transform.localPosition = new Vector3(0, 0, 0);
                //XZ
                dataGridList[2].GetGridObject().transform.localPosition = new Vector3(0, 1, 0);
            }
            //## Top Part ##
            // -++
            if (cDir.x < center.x && cDir.y > center.y && cDir.z > center.z)
            {
                //XY
                dataGridList[0].GetGridObject().transform.localPosition = new Vector3(0, 0, 0);
                //YZ
                dataGridList[1].GetGridObject().transform.localPosition = new Vector3(1, 0, 0);
                //XZ
                dataGridList[2].GetGridObject().transform.localPosition = new Vector3(0, 0, 0);
            }
            //+++
            else if (cDir.x > center.x && cDir.y > center.y && cDir.z > center.z)
            {
                //XY
                dataGridList[0].GetGridObject().transform.localPosition = new Vector3(0, 0, 0);
                //YZ
                dataGridList[1].GetGridObject().transform.localPosition = new Vector3(0, 0, 0);
                //XZ
                dataGridList[2].GetGridObject().transform.localPosition = new Vector3(0, 0, 0);
            }
            //-+-
            else if (cDir.x < center.x && cDir.y > center.y && cDir.z < center.z)
            {
                //XY
                dataGridList[0].GetGridObject().transform.localPosition = new Vector3(0, 0, 1);
                //YZ
                dataGridList[1].GetGridObject().transform.localPosition = new Vector3(1, 0, 0);
                //XZ
                dataGridList[2].GetGridObject().transform.localPosition = new Vector3(0, 0, 0);
            }
            //++-
            else if (cDir.x > center.x && cDir.y > center.y && cDir.z < center.z)
            {
                //XY
                dataGridList[0].GetGridObject().transform.localPosition = new Vector3(0, 0, 1);
                //YZ
                dataGridList[1].GetGridObject().transform.localPosition = new Vector3(0, 0, 0);
                //XZ
                dataGridList[2].GetGridObject().transform.localPosition = new Vector3(0, 0, 0);
            }

        }

        /// <summary>
        /// Returns the min position and max position of the specific Axis in the Container including the offset
        /// </summary>
        /// <returns></returns>
        private float[] GetAxisOffsetCoord(Direction axis)
        {
            Vector3 min = containerBounds.min;
            Vector3 max = containerBounds.max;
            return new[] { min[(int)axis] + xyzOffset[(int)axis], max[(int)axis] - xyzOffset[(int)axis] };
        }

        /// <summary>
        /// Create Range for an Axis based on the Tick Offset and the Axis Length
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        private List<double> GetAxisRange(Direction axis)
        {
            float[] axisOffsetCoord = GetAxisOffsetCoord(axis);
        
            return new List<double> { axisOffsetCoord[0], axisOffsetCoord[1]};
        }

        /// <summary>
        /// Method returns the Scale Function for double Values added to the Axis
        /// Uses ScaleLinear
        /// </summary>
        /// <param name="dataValues"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        private Scale CreateAxisScale(double[] dataValues, float offset)
        {
            List<double> range = new List<double>(2)
            {
                0.0d + offset,
                1.0d - offset
            };

            List<double> domain = new List<double>(2)
            {
                dataValues.Min(),
                dataValues.Max()
            };

            Scale scaleFunction = new ScaleLinear(domain, range);

            return scaleFunction;
        }

        /// <summary>
        /// Method returns the Scale Function for string Values added to the Axis
        /// Uses ScaleNominal
        /// </summary>
        /// <param name="dataValues"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        private Scale CreateAxisScale(string[] dataValues, float offset)
        {
            List<double> range = new List<double>(2)
            {
                0.0d + offset,
                1.0d - offset
            };

            List<double> domain = new List<double>(2)
            {
                0,
                dataValues.Length - 1
            };

            Scale scaleFunction = new ScaleNominal(domain, range, dataValues.ToList());

            return scaleFunction;
        }

        /// <summary>
        /// Method returns the Scale Function for double Values representing the Channel Size
        /// Size scaling is ranged between [0, 1 - 2*offset], where offset depends on the respective axis (and its offsets).
        /// The axis is assumed to between [0, 1]
        /// Uses ScaleLinear
        /// </summary>
        /// <param name="dataValues"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        private Scale CreateSizeScale(double[] dataValues, float offset)
        {
            List<double> range = new List<double>(2)
            {
                0.0d,                  // smallest size is 0
                1.0d - (offset * 2.0f) // biggest size depends on axis
            };

            List<double> domain = new List<double>(2)
            {
                // NOT DATAVALUES USE MIN/MAX Domain OF AXIS
                dataValues.Min(),
                dataValues.Max()
                //0,
                //1
            };

            Scale scaleFunction = new ScaleLinear(domain, range);

            return scaleFunction;
        }

        /// <summary>
        /// Method returns the Scale Function for double Values
        /// Uses ScaleLinear
        /// </summary>
        /// <param name="dataValues"></param>
        /// <param name="rangeVal"></param>
        /// <returns></returns>
        private Scale GetChannelScale(double[] dataValues, double[] rangeVal)
        {
            List<double> range = new List<double>(2)
            {
                rangeVal[0],
                rangeVal[1]
            };

            List<double> domain = new List<double>(2)
            {
                dataValues.Min(),
                dataValues.Max()
            };

            Scale scaleFunction = new ScaleLinear(domain, range);

            return scaleFunction;
        }

        private void CheckChannelDataValueCount()
        {
            if (channelValues == null || channelValues.Count <= 0)
            {
                Debug.LogWarning("No Channel Values added!");
            }

            dataMarkCount = channelValues[0].Length;

            foreach (var channel in channelValues)
            {
                if (dataMarkCount != channel.Value.Length)
                {
                    Debug.LogWarning("Channel " + channel.Key +" has " + channel.Value.Length + " Values, while first added Channel " + channelValues.ElementAt(0).Key + " had " + dataMarkCount + " Values!\n Data marks may be missing.");
                    return;
                }
            }
        }

    }
}
