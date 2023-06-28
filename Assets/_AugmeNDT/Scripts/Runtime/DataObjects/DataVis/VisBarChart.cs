using UnityEngine;

namespace AugmeNDT{
    public class VisBarChart : Vis
    {

        public VisBarChart()
        {
            title = "Basic Bar Chart";                                  
            axes = 3;

            dataMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/Marks/Bar");
            tickMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/VisContainer/Tick");
        }
    

        public override GameObject CreateVis(GameObject container)
        {
            base.CreateVis(container);

            SetVisParams();

            //## 01: Create Axes and Grids

            for (int currAxis = 0; currAxis < axes; currAxis++)
            {
                //encodedAttribute.Add(currAxis);
                int nextDim = (currAxis + 1) % axes;
                CreateAxis(channelEncoding[(VisChannel) currAxis], false, (Direction)currAxis);
                visContainer.CreateGrid((Direction)currAxis, (Direction)nextDim);
            }

            //## 02: Set Remaining Vis Channels (Color,...)
            SetChannel(VisChannel.XPos, channelEncoding[VisChannel.XPos], false);
            SetChannel(VisChannel.YSize, channelEncoding[VisChannel.YPos], false);
            if (axes == 3) SetChannel(VisChannel.ZPos, channelEncoding[VisChannel.ZPos], false);

            visContainer.SetChannel(VisChannel.Color, channelEncoding[VisChannel.Color].GetNumericalVal());

            //## 03: Draw all Data Points with the provided Channels 
            visContainer.CreateDataMarks(dataMarkPrefab, new[] { 1, 0, 1 });

            //## 04: Create Color Scalar Bar
            GameObject colorScalarBarContainer = new GameObject("Color Scale");
            colorScalarBarContainer.transform.parent = visContainerObject.transform;

            ColorScalarBar colorScalarBar = new ColorScalarBar();

            GameObject colorBar01 = colorScalarBar.CreateColorScalarBar(visContainerObject.transform.position, channelEncoding[VisChannel.Color].GetName(), channelEncoding[VisChannel.Color].GetMinMaxVal(), 1, colorScheme);
            //colorBar01.transform.parent = colorScalarBarContainer.transform;
            CreateColorLegend(colorBar01);

            //## 05: Rescale Chart
            visContainerObject.transform.localScale = new Vector3(width, height, depth);

            return visContainerObject;
        }

        public override void ChangeAxisAttribute(int axisId, int selectedDimension, int numberOfTicks)
        {
            /*
        // Record new selected attribute
        encodedAttribute[axisId] = selectedDimension;

        // Calculate new Scale based on selected Attribute
        List<double> domain = new List<double>(2);
        List<double> range = new List<double> { 0, 1 };

        domain.Add(dataSets[0].ElementAt(selectedDimension).Value.Min());
        domain.Add(dataSets[0].ElementAt(selectedDimension).Value.Max());

        Scale scale = CreateScale(dataScaleTypes[axisId], domain, range);


        visContainer.ChangeAxis(axisId, dataSets[0].ElementAt(selectedDimension).Key, scale, numberOfTicks);

        //Change Data Marks
        ChangeDataMarks();
        */
        }

        public override void ChangeDataMarks()
        {
            /*
        for (int value = 0; value < numberOfValues[0]; value++)
        {
            //Default:
            DataMark.Channel channel = DataMark.DefaultDataChannel();

            //X Axis
            var xCoordinate = visContainer.dataAxisList[0].dataScale.GetScaledValue(dataSets[0].ElementAt(encodedAttribute[0]).Value[value]);
            channel.position[0] = (float)xCoordinate;

            //Y Height
            var barHeight = visContainer.dataAxisList[1].dataScale.GetScaledValue(dataSets[0].ElementAt(encodedAttribute[1]).Value[value]);
            channel.size[1] = (float)barHeight;

            //Z Axis
            if (axes == 3)
            {
                var zCoordinate = visContainer.dataAxisList[2].dataScale.GetScaledValue(dataSets[0].ElementAt(encodedAttribute[2]).Value[value]);
                channel.position[2] = (float)zCoordinate;
            }

            visContainer.ChangeDataMark(value, channel);
        }
        */
        }

    }
}
