using System.Linq;
using UnityEngine;

public class VisHistogram : Vis
{
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
        // Load basic Vis properties
        base.CreateVis(container);

        //## 00: Preprocess Data
        HistogramValues hist = new HistogramValues(channelEncoding[VisChannel.XPos]);

        xyzTicks = new[] { hist.GetBinCount(), 10, 10 };
        visContainer.SetAxisTickNumber(xyzTicks);


        // Run through all bins and create a string array with the lower (exclusive) and upper bound (inclusive) of each bin
        string[] binBoundIntervals = new string[hist.GetBinCount()];
        int[] binNumbers = new int[hist.GetBinCount()];                 // Array numbering all bins from 0 to n
        double[] yPos = new double[hist.GetBinCount()];                 // yPos should start at 0

        for (int i = 0; i < hist.GetBinCount(); i++)
        {
            if (i == 0)
            {
                binBoundIntervals[i] = hist.GetLowerBinBound(i).ToString("[ 0.00") + " - " + hist.GetUpperBinBound(i).ToString("0.00 ]");
            }
            else
            {
                binBoundIntervals[i] = hist.GetLowerBinBound(i).ToString("] 0.00") + " - " + hist.GetUpperBinBound(i).ToString("0.00 ]");
            }

            yPos[i] = 0;
            binNumbers[i] = i;
        }


        Debug.Log("BinCount: " + hist.GetBinCount());
        var frequencies = hist.GetBinFrequencies();
        string frequenciesString = "";
        for (var index = 0; index < frequencies.Length; index++)
        {
            frequenciesString += "Bin " + index + " has [" + frequencies[index] + "] \n";
        }
        Debug.Log(frequenciesString);

        //## 01: Create Axes and Grids

        // X Axis
        visContainer.CreateAxis(dataSets[0].GetAttributeName(0), binBoundIntervals, Direction.X);
        visContainer.CreateGrid(Direction.X, Direction.Y);

        // Y Axis
        visContainer.CreateAxis("Frequency", new []{ 0, hist.GetMinMaxFrequency()[1] }, Direction.Y); // Histograms normaly start from 0, so ignore min value

        //## 02: Set Remaining Vis Channels (Color,...)
        visContainer.SetChannel(VisChannel.XPos, binNumbers.Select(x => (double)x).ToArray());
        visContainer.SetChannel(VisChannel.YPos, yPos);
        visContainer.SetChannel(VisChannel.YSize, hist.GetBinFrequencies().Select(x => (double)x).ToArray());
        visContainer.SetChannel(VisChannel.Color, hist.GetBinFrequencies().Select(x => (double)x).ToArray());

        //## 03: Draw all Data Points with the provided Channels 
        visContainer.CreateDataMarks(dataMarkPrefab, new []{ 1, 0, 1 });

        //## 04: Rescale Chart
        visContainerObject.transform.localScale = new Vector3(width, height, depth);

        return visContainerObject;
    }

}
