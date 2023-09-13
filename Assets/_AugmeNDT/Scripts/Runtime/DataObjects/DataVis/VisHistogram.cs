using System;
using UnityEngine;

namespace AugmeNDT
{

    public class VisHistogram : Vis
    {
        private HistogramValues hist;
        private Attribute bins;
        private Attribute frequencies;
        private double[] yPos;

        public VisHistogram()
        {
            title = "Histogram";
            axes = 2;

            //Define Data Mark and Tick Prefab
            dataMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/Marks/Bar");
            tickMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/VisContainer/Tick");
        }

        public override GameObject CreateVis(GameObject container)
        {
            base.CreateVis(container);

            SetVisParams();

            //## 00: Preprocess Data
            PrepareHistogram(channelEncoding[VisChannel.XPos]);

            xyzTicks = new[] { hist.GetBinCount(), 10, 10 };
            visContainer.SetAxisTickNumber(xyzTicks);

            //## 01: Create Axes and Grids

            // X Axis
            CreateAxis(bins, false, Direction.X);
            visContainer.CreateGrid(Direction.X, Direction.Y);

            // Y Axis
            visContainer.CreateAxis(frequencies.GetName(), new []{ 0.0, frequencies.GetMinMaxVal()[1] }, Direction.Y); 

            //## 02: Set Remaining Vis Channels (Color,...)
            SetChannel(VisChannel.XPos, bins, false);
            visContainer.SetChannel(VisChannel.YPos, yPos);
            SetChannel(VisChannel.YSize, frequencies, false);
            SetChannel(VisChannel.Color, frequencies, false);

            //## 03: Draw all Data Points with the provided Channels 
            visContainer.CreateDataMarks(dataMarkPrefab, new[] { 1, 0, 1 });

            //## 04: Rescale Chart
            visContainerObject.transform.localScale = new Vector3(width, height, depth);

            return visContainerObject;
        }


        private void PrepareHistogram(Attribute attribute)
        {
            hist = new HistogramValues(attribute.GetNumericalVal());

            bins = new Attribute(attribute.GetName(), hist.GetBinIntervals());
            frequencies = new Attribute("Frequency", hist.GetBinFrequencies()); // Histograms normaly start from 0, so ignore min value (=> from 0.0 to frequencies.GetMinMaxVal()[1] ) 

            yPos = new double[hist.GetBinCount()];                              // yPos should start at 0
            Array.Fill(yPos, 0);
        }

    }

}