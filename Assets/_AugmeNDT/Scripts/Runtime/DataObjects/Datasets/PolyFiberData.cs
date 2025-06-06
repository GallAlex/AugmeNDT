// /*
//  * MIT License
//  * Copyright (c) 2025 Alexander Gall
//  */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace AugmeNDT{
    /// <summary>
    /// Class stores the values of a polygonal model for fibers
    /// </summary>
    public class PolyFiberData : ScriptableObject, IPolygonDataset
    {
        private string dataSetName;
        private long numberOfFibers = -1;
        private Dictionary<string, double[]> data; //Used for DataVis
        private AbstractDataset abstractFiberDataset; //Used for Statistics

        private int[] label;
        private double[] realX1;
        private double[] realY1;
        private double[] realZ1;
        private double[] realX2;
        private double[] realY2;
        private double[] realZ2;
        private double[] straightLength;
        private double[] curvedLength;
        private double[] diameter;
        private double[] surfaceArea;
        private double[] volume;
        private int[] seperatedFibre;
        private int[] curvedFibre;

        // Values for drawing
        private float[] maxDimension;
        private float[] minDimension;
        private float maxDiameter;
        private float minDiameter;

        private float targetSize = 1f;
        private ScaleLinear scalingX;
        private ScaleLinear scalingY;
        private ScaleLinear scalingZ;
        private ScaleLinear scalingDiameter;

        #region Getter/Setter

        public long NumberOfFibers
        {
            get => numberOfFibers;
            set => numberOfFibers = value;
        }

        public int[] Label
        {
            get => label;
            set => label = value;
        }

        public double[] RealX1
        {
            get => realX1;
            set => realX1 = value;
        }

        public double[] RealY1
        {
            get => realY1;
            set => realY1 = value;
        }

        public double[] RealZ1
        {
            get => realZ1;
            set => realZ1 = value;
        }

        public double[] RealX2
        {
            get => realX2;
            set => realX2 = value;
        }

        public double[] RealY2
        {
            get => realY2;
            set => realY2 = value;
        }

        public double[] RealZ2
        {
            get => realZ2;
            set => realZ2 = value;
        }

        public double[] StraightLength
        {
            get => straightLength;
            set => straightLength = value;
        }

        public double[] CurvedLength
        {
            get => curvedLength;
            set => curvedLength = value;
        }

        public double[] Diameter
        {
            get => diameter;
            set => diameter = value;
        }

        public double[] SurfaceArea
        {
            get => surfaceArea;
            set => surfaceArea = value;
        }

        public double[] Volume
        {
            get => volume;
            set => volume = value;
        }

        public int[] SeperatedFibre
        {
            get => seperatedFibre;
            set => seperatedFibre = value;
        }

        public int[] CurvedFibre
        {
            get => curvedFibre;
            set => curvedFibre = value;
        }

        #endregion

        public void SetDatasetName(string dataSetName)
        {
            this.dataSetName = dataSetName;
        }

        public void FillPolyFiberData(List<List<string>> csvValues)
        {
            numberOfFibers = csvValues[0].GetRange(1, csvValues[0].Count - 1).Count;
            data = new Dictionary<string, double[]>(csvValues.Count);


            for (int column = 0; column < csvValues.Count; column++)
            {
                string[] valuesWithoutHeader = csvValues[column].GetRange(1, csvValues[column].Count - 1).ToArray();

                //ToDo File with possible Spellings for the Headers of the dame value, Check String Encoding, Maybe changes to position in csv File or compare key Words in string
                switch (csvValues[column][0])
                {
                    case "Label":
                        label = Array.ConvertAll(valuesWithoutHeader, int.Parse);
                        data.Add("Label", Array.ConvertAll(valuesWithoutHeader, double.Parse));
                        break;
                    case "RealX1 [�m]":
                        realX1 = Array.ConvertAll(valuesWithoutHeader, s => double.Parse(s, CultureInfo.InvariantCulture));
                        data.Add("RealX1 [�m]", realX1);
                        break;
                    case "RealY1 [�m]":
                        realY1 = Array.ConvertAll(valuesWithoutHeader, s => double.Parse(s, CultureInfo.InvariantCulture));
                        data.Add("RealY1 [�m]", realY1);
                        break;
                    case "RealZ1 [�m]":
                        realZ1 = Array.ConvertAll(valuesWithoutHeader, s => double.Parse(s, CultureInfo.InvariantCulture));
                        data.Add("RealZ1 [�m]", realZ1);
                        // flip z axis (*-1) to make it work with the Unity coordinate system
                        for (int i = 0; i < realZ1.Length; i++)
                        {
                            realZ1[i] *= -1;
                        }
                        break;
                    case "RealX2 [�m]":
                        realX2 = Array.ConvertAll(valuesWithoutHeader, s => double.Parse(s, CultureInfo.InvariantCulture));
                        data.Add("RealX2 [�m]", realX2);
                        break;
                    case "RealY2 [�m]":
                        realY2 = Array.ConvertAll(valuesWithoutHeader, s => double.Parse(s, CultureInfo.InvariantCulture));
                        data.Add("RealY2 [�m]", realY2);
                        break;
                    case "RealZ2 [�m]":
                        realZ2 = Array.ConvertAll(valuesWithoutHeader, s => double.Parse(s, CultureInfo.InvariantCulture));
                        data.Add("RealZ2 [�m]", realZ2);
                        // flip z axis (*-1) to make it work with the Unity coordinate system
                        for (int i = 0; i < realZ2.Length; i++)
                        {
                            realZ2[i] *= -1;
                        }
                        break;
                    case "StraightLength [�m]":
                        straightLength = Array.ConvertAll(valuesWithoutHeader, s => double.Parse(s, CultureInfo.InvariantCulture));
                        data.Add("StraightLength [�m]", straightLength);
                        break;
                    case "CurvedLength [�m]":
                        curvedLength = Array.ConvertAll(valuesWithoutHeader, s => double.Parse(s, CultureInfo.InvariantCulture));
                        break;
                    case "Diameter [�m]":
                        diameter = Array.ConvertAll(valuesWithoutHeader, s => double.Parse(s, CultureInfo.InvariantCulture));
                        data.Add("Diameter [�m]", diameter);
                        break;
                    case "SurfaceArea [�m]�m2]":
                        surfaceArea = Array.ConvertAll(valuesWithoutHeader, s => double.Parse(s, CultureInfo.InvariantCulture));
                        data.Add("SurfaceArea [�m]�m2]", surfaceArea);
                        break;
                    case "Volume [�m]�m3]":
                        volume = Array.ConvertAll(valuesWithoutHeader, s => double.Parse(s, CultureInfo.InvariantCulture));
                        data.Add("Volume [�m]�m3]", volume);
                        break;
                    case "Seperated Fibre":
                        seperatedFibre = Array.ConvertAll(valuesWithoutHeader, int.Parse);
                        data.Add("Seperated Fibre", Array.ConvertAll(valuesWithoutHeader, double.Parse));
                        break;
                    case "Curved Fibre":
                        curvedFibre = Array.ConvertAll(valuesWithoutHeader, int.Parse);
                        data.Add("Curved Fibre", Array.ConvertAll(valuesWithoutHeader, double.Parse));
                        break;
                }
            }

            // For drawing the fibers, we need to calculate the min and max values of the drawing properties
            CalculateMinMax();
            CalculateScaling(targetSize);

        }

        /// <summary>
        /// Returns the coordinates of the start and end point of a fiber
        /// </summary>
        /// <param name="fiberId"></param>
        /// <returns>List containing x,y,z coordinate of start- and endpoint</returns>
        public List<Vector3> GetFiberCoordinates(int fiberId)
        {
            //TODO: Uses float instead of double!!
            List<Vector3> linePoints = new List<Vector3>();
            linePoints.Add(new Vector3((float)realX1[fiberId], (float)realY1[fiberId], (float)realZ1[fiberId])); 
            linePoints.Add(new Vector3((float)realX2[fiberId], (float)realY2[fiberId], (float)realZ2[fiberId]));

            return linePoints;
        }

        public List<Vector3> GetScaledFiberCoordinates(int fiberId, float targetSize)
        {
            
            Debug.Log("RealX1: " + realX1[fiberId] + " RealY1: " + realY1[fiberId] + " RealZ1: " + realZ1[fiberId]);
            Debug.Log("RealX2: " + realX2[fiberId] + " RealY2: " + realY2[fiberId] + " RealZ2: " + realZ2[fiberId]);

            List<Vector3> linePoints = new List<Vector3>();
            linePoints.Add(new Vector3((float)scalingX.GetScaledValue(realX1[fiberId]), (float)scalingY.GetScaledValue(realY1[fiberId]), (float)scalingZ.GetScaledValue(realZ1[fiberId])));
            linePoints.Add(new Vector3((float)scalingX.GetScaledValue(realX2[fiberId]), (float)scalingY.GetScaledValue(realY2[fiberId]), (float)scalingZ.GetScaledValue(realZ2[fiberId])));

            Debug.Log("Scaled RealX1: " + linePoints[0].x + " RealY1: " + linePoints[0].y + " RealZ1: " + linePoints[0].z);
            Debug.Log("Scaled RealX2: " + linePoints[1].x + " RealY2: " + linePoints[1].y + " RealZ2: " + linePoints[1].z);

            return linePoints;
        }

        /// <summary>
        /// Returns the radius of a fiber
        /// </summary>
        /// <param name="fiberId"></param>
        /// <returns>Radius of fiber</returns>
        public float GetFiberRadius(int fiberId)
        {
            //TODO: Uses float instead of double!!
            return (float)diameter[fiberId] / 2.0f;
        }

        public float GetScaledFiberRadius(int fiberId, float targetSize)
        {
            float scaleFactorDiamter = targetSize / maxDiameter;
            return ((float)diameter[fiberId] * scaleFactorDiamter) / 2.0f;
        }

        public AbstractDataset ExportForDataVis()
        {
            if (abstractFiberDataset == null)
            {
                //TODO: Make specific method to remove entries from file
                Dictionary<string, double[]> reducedData = data; //Used for Vis
                reducedData.Remove("Label");
                reducedData.Remove("Seperated Fibre");
                reducedData.Remove("Curved Fibre");

                abstractFiberDataset = new AbstractDataset(dataSetName, reducedData.Keys.ToList(), reducedData);
            }

            return abstractFiberDataset;
        }

        /// <summary>
        /// Calculates the min and max values of the drawing properties (Position, Diameter)
        /// </summary>
        private void CalculateMinMax()
        {
            // Calculate min/max values between realX1, realX2, realY1, realY2, realZ1, realZ2
            double maxX = Math.Max(realX1.Max(), realX2.Max());
            double minX = Math.Min(realX1.Min(), realX2.Min());

            double maxY = Math.Max(realY1.Max(), realY2.Max());
            double minY = Math.Min(realY1.Min(), realY2.Min());

            double maxZ = Math.Max(realZ1.Max(), realZ2.Max());
            double minZ = Math.Min(realZ1.Min(), realZ2.Min());

            // Calculate the biggest Pos value
            maxDimension = new[] {(float)maxX, (float)maxY, (float)maxZ};
            minDimension = new[] { (float)minX, (float)minY, (float)minZ };

            // Get the maximum absolut diameter
            maxDiameter = (float)diameter.Max();
            minDiameter = (float)diameter.Min();
        }

        /// <summary>
        /// Calculates the Scaling for the drawing properties (Position)
        /// </summary>
        private void CalculateScaling(float targeSize)
        {
            List<double> domainX = new List<double> { minDimension[0], maxDimension[0] };
            scalingX = new ScaleLinear(domainX);

            List<double> domainY = new List<double> { minDimension[1], maxDimension[1] };
            scalingY = new ScaleLinear(domainY);

            List<double> domainZ = new List<double> { minDimension[2], maxDimension[2] };
            scalingZ = new ScaleLinear(domainZ);

            List<double> dia = new List<double> { minDiameter, maxDiameter };
            scalingDiameter = new ScaleLinear(dia);
        }

        public override string ToString()
        {
            string values = "\nlabel = " + string.Join("\t", label) + "\n";
            values += "realX1 = " + string.Join("\t", realX1) + "\n";
            values += "realY1 = " + string.Join("\t", realY1) + "\n";
            values += "realZ1 = " + string.Join("\t", realZ1) + "\n";
            values += "realX2 = " + string.Join("\t", realX2) + "\n";
            values += "realY2 = " + string.Join("\t", realY2) + "\n";
            values += "realZ2 = " + string.Join("\t", realZ2) + "\n";
            values += "straightLength = " + string.Join("\t", straightLength) + "\n";
            values += "curvedLength = " + string.Join("\t", curvedLength) + "\n";
            values += "diameter = " + string.Join("\t", diameter) + "\n";
            values += "surfaceArea = " + string.Join("\t", surfaceArea) + "\n";
            values += "volume = " + string.Join("\t", volume) + "\n";
            values += "seperatedFibre = " + string.Join("\t", seperatedFibre) + "\n";
            values += "curvedFibre = " + string.Join("\t", curvedFibre) + "\n";

            return base.ToString() + values;
        }
    }
}
