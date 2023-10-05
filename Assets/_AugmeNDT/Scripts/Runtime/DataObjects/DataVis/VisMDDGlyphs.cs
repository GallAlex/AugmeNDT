using Microsoft.MixedReality.Toolkit.UI;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace AugmeNDT{
    /// <summary>
    /// This class is used to create a multidimensional distribution Glyph chart visualization.
    /// </summary>
    public class VisMDDGlyphs : Vis
    {
        // Stores the minimum and maximum for each statistic value based on all dataset
        private List<DistributionValues> minMaxStatisticValues;

        // Stores for each attribute the difference value between first dataset and the next one
        private List<double[]> timeDifference;

        private bool use4DData = false;                     // If more than one dataset is loaded, should the z-Axis be for the other Datasets?

        private GameObject meanBarPrefab;

        private GameObject selectionBoxPrefab;
        private List<GameObject> selectionBoxes;

        private GameObject colorLegend;
        private VisTimeScatter timeScatter;


        public VisMDDGlyphs()
        {
            title = "MDD-Glyphs Chart";
            axes = 3;

            dataMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/Marks/Bar");
            tickMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/VisContainer/Tick");

            meanBarPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/Marks/Bar");

            selectionBoxPrefab = (GameObject)Resources.Load("Prefabs/SelectionBox");

            // Create Interactor
            //visInteractor = new VisMDDGlyphInteractor(this);
        }
    

        public override GameObject CreateVis(GameObject container)
        {
            base.CreateVis(container);

            //Set default channel encodings
            //SetChannelEncoding(VisChannel.XPos, dataSets[0].GetHeader());
            //SetChannelEncoding(VisChannel.YPos, dataSets[0].GetAllAttributeValues(true));
            //SetChannelEncoding(VisChannel.ZPos, dataSets[0].GetDerivedAttribute(DerivedAttributes.DerivedAttributeCalc.Kurtosis, true));

            if (dataSets.Count > 1) use4DData = true;
        
            if(!use4DData) xyzTicks = new int[] { dataSets[0].attributesCount, 13, 7 };
            else xyzTicks = new int[] { dataSets[0].attributesCount, 10, dataSets.Count };

            xyzOffset = new float[] { 0.05f, 0.05f, 0.05f };

            SetVisParams();

            Debug.Log("Create MDDGlyph");

            MDDGlyphColorLegend legend = new MDDGlyphColorLegend(this);


            if (!use4DData)
            {
                //## 02: Create Axes and Grids

                // X Axis
                CreateAxis(dataEnsemble.GetHeaderAttribute(0), false, (Direction)0);
                visContainer.CreateGrid((Direction)0, (Direction)1);

                // Y Axis
                visContainer.CreateAxis("Attributes Value", dataEnsemble.GetMinMaxAttrVal(0, true), (Direction)1);
                visContainer.CreateGrid((Direction)1, (Direction)2);

                // Z Axis
                visContainer.CreateAxis("Modality", dataEnsemble.GetMinMaxDerivedAttrVal(0, DerivedAttributes.DerivedAttributeCalc.Modality, false), (Direction)2);
                visContainer.CreateGrid((Direction)2, (Direction)0);


                //## 03: Calculate Channels
                //legend.SetMinMaxSkewKurtValues(new double[]{minMaxStatisticValues[0].Skewness, minMaxStatisticValues[1].Skewness}, new double[] { minMaxStatisticValues[0].Kurtosis, minMaxStatisticValues[1].Kurtosis });
                legend.SetMinMaxSkewKurtValues(dataEnsemble.GetMinMaxDerivedAttrVal(0, DerivedAttributes.DerivedAttributeCalc.Skewness, false), dataEnsemble.GetMinMaxDerivedAttrVal(0, DerivedAttributes.DerivedAttributeCalc.Kurtosis, false));

                //X Axis (Attributes)
                SetChannel(VisChannel.XPos, dataEnsemble.GetHeaderAttribute(0), false);

                //Y Axis (Q1)
                visContainer.SetChannel(VisChannel.YPos, dataEnsemble.GetDerivedAttributeValues(0, DerivedAttributes.DerivedAttributeCalc.LowerQuartile, true));

                // Y Size (IQR)
                visContainer.SetChannel(VisChannel.YSize, dataEnsemble.GetDerivedAttributeValues(0, DerivedAttributes.DerivedAttributeCalc.Iqr, true));

                //Z Axis (Modality)
                visContainer.SetChannel(VisChannel.ZPos, dataEnsemble.GetDerivedAttributeValues(0, DerivedAttributes.DerivedAttributeCalc.Modality, false));

                //Color (Skewness + Kurtosis)
                //visContainer.SetChannel(VisChannel.Color, dataEnsemble.GetDerivedAttributeValues(0, DerivedAttributes.DerivedAttributeCalc.Modality, true));
                CreateMDDGlyphColors(legend);
            }
            else
            {
                visContainer.SetHandleText("4D Vis");

                //## 02: Create Axes and Grids

                // X Axis
                //CreateAxis(channelEncoding[VisChannel.XPos], false, (Direction)0);
                CreateAxis(dataEnsemble.GetHeaderAttribute(0), false, (Direction)0);
                visContainer.CreateGrid((Direction)0, (Direction)1);

                // Y Axis
                visContainer.CreateAxis("Attributes Value", dataEnsemble.GetMinMaxAttrVal(true), (Direction)1);
                //visContainer.CreateAxis("Attributes Value", new []{minMaxStatisticValues[0].SmallestElement, minMaxStatisticValues[1].LargestElement}, (Direction)1);
                //CreateAxis(channelEncoding[VisChannel.YPos], true, (Direction)1);
                visContainer.CreateGrid((Direction)1, (Direction)2);

                // Z Axis
                //visContainer.CreateAxis("Timestep", datasetNames, (Direction)2); 
                Attribute timeSteps = new Attribute("Datasets", dataEnsemble.GetAbstractDataSetNames());
                CreateAxis(timeSteps, false, (Direction)2);
                visContainer.CreateGrid((Direction)2, (Direction)0);


                //## 03: Calculate Channels

                //Need dummy/repeated Vals for textual attributes
                double[] xPos = new double[dataEnsemble.GetDataSetCount() * dataEnsemble.GetDataSet(0).attributesCount];
                double[] zPos = new double[dataEnsemble.GetDataSetCount() * dataEnsemble.GetDataSet(0).attributesCount];

                //TODO: CHECK X Pos Vals!

                for (int dataSet = 0; dataSet < dataEnsemble.GetDataSetCount(); dataSet++)
                {
                    for (int attr = 0; attr < dataEnsemble.GetDataSet(0).attributesCount; attr++)
                    {
                        xPos[attr + (dataSet * dataEnsemble.GetDataSet(0).attributesCount)] = attr;
                        zPos[attr + (dataSet * dataEnsemble.GetDataSet(0).attributesCount)] = dataSet;
                    }

                }

                //Debug.Log(TablePrint.ToStringRow(xPos));
                //Debug.Log(TablePrint.ToStringRow(zPos));

                //legend.SetMinMaxSkewKurtValues(new double[]{minMaxStatisticValues[0].Skewness, minMaxStatisticValues[1].Skewness}, new double[] { minMaxStatisticValues[0].Kurtosis, minMaxStatisticValues[1].Kurtosis });
                legend.SetMinMaxSkewKurtValues(dataEnsemble.GetMinMaxDerivedAttrVal(DerivedAttributes.DerivedAttributeCalc.Skewness, false), dataEnsemble.GetMinMaxDerivedAttrVal(DerivedAttributes.DerivedAttributeCalc.Kurtosis, false));

                //X Axis (Attributes)
                visContainer.SetChannel(VisChannel.XPos, xPos);

                //Y Axis (Q1)
                visContainer.SetChannel(VisChannel.YPos, dataEnsemble.GetDerivedAttributeValues(DerivedAttributes.DerivedAttributeCalc.LowerQuartile, true));

                // Y Size (IQR)
                visContainer.SetChannel(VisChannel.YSize, dataEnsemble.GetDerivedAttributeValues(DerivedAttributes.DerivedAttributeCalc.Iqr, true));

                //Z Axis (Timesteps)
                visContainer.SetChannel(VisChannel.ZPos, zPos);

                //Color (Skewness + Kurtosis)
                //visContainer.SetChannel(VisChannel.Color, zPos);
                CreateMDDGlyphColors(legend);


                // ChiSquared
                // Saves a List of Datasets, each containing a List of Attributes with their TimeDifferences to the previous Dataset
                List<List<double>> timeVals = new List<List<double>>(dataSets.Count);
                string temp = "CalculateChiSquaredMetric: \n";

                // For each DataSet (without the last one)...
                for (int currentDataSet = 0; currentDataSet < dataSets.Count; currentDataSet++)
                {
                    List<double> attrTimeVals = new List<double>(dataSets[currentDataSet].attributesCount);
                    timeVals.Add(attrTimeVals);

                    temp += "\n Dataset " + currentDataSet + "\n";
                    
                    // For each Attribute...
                    for (int attr = 0; attr < dataEnsemble.GetDataSet(0).attributesCount; attr++)
                    {
                        // First DataSet has no time difference, Fill first row with 0s (0-> 0) 
                        if (currentDataSet == 0)
                        {
                            attrTimeVals.Add(0);
                            temp += "A" + attr + ": 0 -> 0 = " + attrTimeVals[attr] + "\n";

                        }
                        else
                        {
                            var diff = DistributionCalc.GetChiSquaredMetric(dataEnsemble.GetAttribute(currentDataSet-1, attr).GetNumericalVal(), dataEnsemble.GetAttribute(currentDataSet, attr).GetNumericalVal());
                            var origDiff = diff;
                            diff += timeVals[currentDataSet-1][attr];

                            attrTimeVals.Add(diff);

                            temp += "A" + attr + ": " + (currentDataSet - 1) + " -> " + currentDataSet + " = " + diff + " (" + origDiff + " + " + timeVals[currentDataSet - 1][attr] + ") \n";
                        }

                    }

                }

                Debug.Log(temp);

                // Normalize
                timeDifference = new List<double[]>(dataSets[0].attributesCount);

                for (int attr = 0; attr < dataEnsemble.GetDataSet(0).attributesCount; attr++)
                {
                    List<double> differences = new List<double>();
                    for (int currentDataSet = 0; currentDataSet < dataSets.Count; currentDataSet++)
                    {
                        differences.Add(timeVals[currentDataSet][attr]);
                    }

                    Attribute normalize = new Attribute("Normalized ChiSquaredMetric", differences.ToArray());
                    timeDifference.Add(normalize.GetNumericalValNorm());
                }

                /*
                //TODO: Save as Matrix? timeDifference[0][0] = 
                timeDifference = new List<double[]>(dataSets[0].attributesCount);
                string chiSquared = "ChiSquaredMetric: \n";

                // For all attributes (attributeCount)
                for (int attr = 0; attr < dataEnsemble.GetDataSet(0).attributesCount; attr++)
                {
                    List<double> differences = new List<double>();
                    differences.Add(0);
                    string temp = "CalculateChiSquaredMetric of " + dataSets[0].attributeNames[attr] + ": \n";

                    // Calculate time difference between current and next dataset
                    for (int currentDataSet = 0; currentDataSet < dataSets.Count-1; currentDataSet++)
                    {
                        var diff = DistributionCalc.GetChiSquaredMetric(dataEnsemble.GetAttribute(currentDataSet, attr).GetNumericalVal(), dataEnsemble.GetAttribute(currentDataSet + 1, attr).GetNumericalVal());
                        var origDiff = diff;
                        // Add value of last diff to current diff (differences[0] always 0)
                        diff += differences[currentDataSet];
                        differences.Add(diff);
                        temp += currentDataSet + " -> " + (currentDataSet + 1) + " = " + diff + " (" + origDiff + ")"+ "\n";
                    }
                    chiSquared += temp + "\n";

                    // Normalize summed ChiSquaredMetric
                    if (dataSets.Count > 1)
                    {
                        Attribute normalize = new Attribute("Normalized ChiSquaredMetric", differences.ToArray());
                        TablePrint.ToStringRow(normalize.GetNumericalValNorm());
                        timeDifference.Add(normalize.GetNumericalValNorm()); 
                    }
                    else timeDifference.Add(differences.ToArray());
                }

                Debug.Log(chiSquared);
                */
            }

            
            //## 04: Create Data Marks
            visContainer.CreateDataMarks(dataMarkPrefab, new []{1, 0, 1});
            CreateMeanBar(); 

            //## 05: Create Color Scalar Bar
            colorLegend = legend.GetColorLegend();
            CreateColorLegend(colorLegend);
            

            //## 06: Create Selection Boxes for X Axis
            CreateSelectionBoxes();


            //visContainer.GatherDataMarkValueInformation(0);
            //visContainer.GatherDataMarkValueInformation(1);

            //visContainer.CreateDataMarkAxisLine(0);
            //visContainer.CreateDataMarkAxisLine(1);

            //dataSets[0].PrintDatasetValues(false);
            //dataSets[0].PrintDatasetValues(true);

            //## 07: Rescale
            visContainerObject.transform.localScale = new Vector3(width, height, depth);


            //## 08: Set up individual Interctions
            SetUpVisTransitionInteractor(visContainerObject);

            return visContainerObject;
        }

        public override void ChangeDataMarks()
        {
            Debug.Log("Change MDDGlyph");
            visContainer.ChangeDataMarks();
        }


        public void CreateMDDGlyphColors(MDDGlyphColorLegend legend)
        {

            Debug.Log(">> Create NEW MDDGlyph Colors");

            int numberOfAttributes = dataEnsemble.GetDataSet(0).attributesCount;
            int numberOfDatasets = dataEnsemble.GetDataSetCount();

            Color[] c = new Color[numberOfAttributes * numberOfDatasets];

            for (int dataSet = 0; dataSet < numberOfDatasets; dataSet++)
            {
                for (int attr = 0; attr < numberOfAttributes; attr++)
                {
                    var index = attr + (dataSet * numberOfAttributes);

                    Color gylphColor = legend.GetColoring(
                        dataEnsemble.GetDerivedAttributeValues(dataSet, DerivedAttributes.DerivedAttributeCalc.Skewness,
                            false)[attr],
                        dataEnsemble.GetDerivedAttributeValues(dataSet, DerivedAttributes.DerivedAttributeCalc.Kurtosis,
                            false)[attr]);

                    c[index] = gylphColor;

                    //TODO: Make Visibility parameter in DataMarks
                    // Check if a dataMark was instantiated for this index (attribute)
                    if (index < visContainer.dataMarkList.Count)
                    {
                        // Hide all glyphs that are not in the IQR Range
                        if (gylphColor == new Color(0, 1.0f, 0, 0))
                        {
                            visContainer.dataMarkList[attr + (dataSet * numberOfAttributes)].GetDataMarkInstance().SetActive(false);
                        }
                        else
                        {
                            visContainer.dataMarkList[attr + (dataSet * numberOfAttributes)].GetDataMarkInstance().SetActive(true);
                        }

                    }
                }
            }

            visContainer.SetSpecificColor(c);

        }

        /// <summary>
        /// Method returns a List of Fiber IDs that are between (including border values) the IQR Range of the selected attribute
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public List<int> GetFiberIDsFromIQRRange(int attribute)
        {
            int datasetNumber = 0;
            
            var q1 = dataEnsemble.GetDerivedAttributeValues(datasetNumber, DerivedAttributes.DerivedAttributeCalc.LowerQuartile, true)[attribute];
            var q2 = dataEnsemble.GetDerivedAttributeValues(datasetNumber, DerivedAttributes.DerivedAttributeCalc.UpperQuartile, true)[attribute];

            // Go through every fiber and check if it is in the IQR Range (in the normalizedDataSets)
            List<int> fiberIDs = new List<int>();

            
            for (int i = 0; i < dataEnsemble.GetAttribute(datasetNumber, attribute).GetNumericalValNorm().Length; i++)
            {
                //var value = dataSets[datasetNumber].GetAttribute(attribute).GetNumericalValNorm()[i];
                var value = dataEnsemble.GetAttribute(datasetNumber, attribute).GetNumericalValNorm()[i];

                if (value >= q1 && value <= q2)
                {
                    fiberIDs.Add(i);
                }
            }

            return fiberIDs;
        }

        private void SetUpVisTransitionInteractor(GameObject container)
        {
            // Get VisContainer and add Component with update routine
            MDDTransition transitionScript = container.AddComponent<MDDTransition>();
         
            // Set class to call when Object is moved
            transitionScript.SetMDDVis(this);

            timeScatter = new VisTimeScatter();
            if (!use4DData)
            {
                timeScatter.axes = 2;
                timeScatter.width = 1;
                timeScatter.height = 1;
                timeScatter.depth = 1;
                //TODO: Copy the properties of the MDDGlyph Vis like offset, ticks,...
                //AbstractDataset statisticalData = new AbstractDataset();

                timeScatter.AppendData(dataSets[0]);
                timeScatter.SetChannelEncoding(VisChannel.XPos, dataSets[0].GetHeader());
                timeScatter.SetChannelEncoding(VisChannel.YPos, new Attribute(DerivedAttributes.DerivedAttributeCalc.Modality.ToString(), dataSets[0].GetDerivedAttribute(DerivedAttributes.DerivedAttributeCalc.Modality,false)));
                timeScatter.CreateVis(visContainerObject);
                timeScatter.visContainerObject.transform.Rotate(90,0,0);
                timeScatter.visContainerObject.SetActive(false);
                // Use Node-Link diagram to show the change between timesteps (y axis is chi-squared metric)
            }
            else
            {
                // Use as Y Axis the Change between Timesteps (ordered)
                // USe as X Axis the Attributes
                timeScatter.axes = 2;
                timeScatter.width = 1;
                timeScatter.height = 1;
                timeScatter.depth = 1;
                //TODO: Copy the properties of the MDDGlyph Vis like offset, ticks,...

                //Dictionary<string, double[]> timeData = new Dictionary<string, double[]>();
                //for (var index = 0; index < dataSets[0].attributesCount; index++)
                //{
                //    timeData.Add(dataSets[0].attributeNames[index], timeDifference[index]);
                //}
                //AbstractDataset timeDataset = new AbstractDataset("Time Relationship", dataSets[0].attributeNames, timeData);
                //timeScatter.AppendData(timeDataset);

                List<double> timeDiff = new List<double>();

                string timeDiffString = "timeDiffString: \n";

                for (int i = 0; i < dataEnsemble.GetDataSetCount(); i++)
                {
                    timeDiffString += "\n";
                    for (int attr = 0; attr < dataSets[0].attributesCount; attr++)
                    {
                        timeDiffString += ("Attr: " + attr + " dataset: " + i + " Value: " + timeDifference[attr][i]) + "\n";
                        timeDiff.Add(timeDifference[attr][i]);
                    }
                    
                }
                Debug.Log(timeDiffString);

                for (int dataSet = 0; dataSet < dataEnsemble.GetDataSetCount(); dataSet++)
                {
                    timeScatter.AppendData(dataSets[dataSet]);
                }
                Attribute timeDiffAttr = new Attribute("Time Difference", timeDiff.ToArray());

                timeScatter.SetChannelEncoding(VisChannel.XPos, dataSets[0].GetHeader());
                timeScatter.SetChannelEncoding(VisChannel.YPos, timeDiffAttr);
                timeScatter.SetChannelEncoding(VisChannel.Color, new Attribute("Datasets", dataEnsemble.GetAbstractDataSetNames()));

                Debug.Log(dataSets[0].GetHeader().PrintAttribute());
                Debug.Log(timeDiffAttr.PrintAttribute());

                //timeDataset.PrintDatasetValues(true);

                timeScatter.CreateVis(visContainerObject);
                timeScatter.visContainerObject.transform.Rotate(90, 0, 0);
                timeScatter.visContainerObject.SetActive(false);
            }

            timeScatter.visContainer.RemoveContainerHandle();
        }

        public void ApplyMDDTransition(bool apply2DTransiton)
        {
            if (apply2DTransiton)
            {
                // Hide the whole VisContainer and show a new one
                visContainer.visContainer.SetActive(false);
                colorLegend.SetActive(false);
                // Create new 2D Vis 
                timeScatter.visContainerObject.SetActive(true);

            }
            else
            {
                // Make the whole VisContainer visible
                visContainer.visContainer.SetActive(true);
                colorLegend.SetActive(true);
                //Hide 2D Vis
                timeScatter.visContainerObject.SetActive(false);

            }

        }

        private void CreateSelectionBoxes()
        {
            GameObject boxHierachy = new GameObject("SelectionBoxes");
            boxHierachy.transform.parent = visContainer.visContainer.transform;

            var numberOfBoxes = dataEnsemble.GetHeaderAttribute(0).GetNumberOfValues();
            selectionBoxes = new List<GameObject>(numberOfBoxes);

            // TODO: Get Size of one Glyphs for selection box width
            float glyphSize = 0.05f;
            Scale scaling = visContainer.GetAxisScale(Direction.X);

            for (int box = 0; box < numberOfBoxes; box++)
            { 
                // Get X Pos of Attribute for Box
                float xPos = (float)scaling.GetScaledValue(box);
                Vector3 boxPos = new Vector3(xPos, 0.5f, 0.5f);     // Chart is shifted by 0.5

                GameObject selectionBox = GameObject.Instantiate(selectionBoxPrefab, boxPos, Quaternion.identity, boxHierachy.transform);
                selectionBox.transform.localScale = new Vector3(glyphSize, selectionBox.transform.localScale.y, selectionBox.transform.localScale.z);
                selectionBox.name ="SelectionBox_" + box;

                // Give selectionBox a Attribute ID for later selection and ref to class
                SelectionBoxInteractable interactable = selectionBox.GetComponent<SelectionBoxInteractable>();
                interactable.selectionBoxID = box;
                interactable.refToMDDGlyph = this;
                interactable.chartArea = visContainer.GetContainerBounds();

                selectionBoxes.Add(selectionBox);
            }


        }

        //Should be called once a selection boxed is dragged out
        public Vis CreateNewVis(int attribute, Vector3 pos)
        {

            VisStackedHistogram stackedHistogram = new VisStackedHistogram();
            stackedHistogram.title = "Stacked Histogram";
            stackedHistogram.axes = 2;
            stackedHistogram.width = 1;
            stackedHistogram.height = 1;
            stackedHistogram.depth = 1;
            stackedHistogram.visInteractor = new VisMDDGlyphInteractor(this);
            stackedHistogram.multiGroups = multiGroups;

            for (int dataSet = 0; dataSet < dataEnsemble.GetDataSetCount(); dataSet++)
            {
                stackedHistogram.AppendData(dataEnsemble.GetDataSet(dataSet));
            }

            string axisName = "Datasets\n(" + dataEnsemble.GetAttribute(attribute).GetName() + ")";

            stackedHistogram.SetChannelEncoding(VisChannel.XPos, new Attribute(axisName, dataEnsemble.GetAbstractDataSetNames()));
            stackedHistogram.SetChannelEncoding(VisChannel.YPos, dataEnsemble.GetAttribute(attribute));

            stackedHistogram.CreateVis(visContainerObject);
            stackedHistogram.SetVisContainerPosition(pos);

            return stackedHistogram;
        }

        /// <summary>
        /// Run through all DataMarks and get the mean value for each attribute (in each Dataset)
        /// Create a Bar for each Glyph with the mean value as yPos and a X,Z size slightly bigger then the Glyph and a Y size of 2 percent of the Glyphs Y size
        /// </summary>
        private void CreateMeanBar()
        {
            double[] meanValues = dataEnsemble.GetDerivedAttributeValues(DerivedAttributes.DerivedAttributeCalc.Mean, true);
            Scale yScale = visContainer.GetAxisScale(Direction.Y);


            // Get the mean value for each attribute
            for (int dataMark = 0; dataMark < visContainer.dataMarkList.Count; dataMark++)
            {
                GameObject dataMarkInstance = visContainer.dataMarkList[dataMark].GetDataMarkInstance();

                // Skip drawing if the Glyph is too small
                if(dataMarkInstance.transform.localScale.y <= 0.0000001f) continue;

                Vector3 meanBarPos = new Vector3(dataMarkInstance.transform.localPosition.x, (float)yScale.GetScaledValue(meanValues[dataMark]), dataMarkInstance.transform.localPosition.z);
                GameObject meanBar = GameObject.Instantiate(meanBarPrefab, meanBarPos, Quaternion.identity);
                meanBar.name = "MeanBar_" + dataMark;
                meanBar.transform.localScale = new Vector3(dataMarkInstance.transform.localScale.x * 1.1f, dataMarkInstance.transform.localScale.y * 0.02f, dataMarkInstance.transform.localScale.z * 1.1f);
                meanBar.transform.parent = dataMarkInstance.transform;
                meanBar.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.gray);
            }

        }

    }
}

