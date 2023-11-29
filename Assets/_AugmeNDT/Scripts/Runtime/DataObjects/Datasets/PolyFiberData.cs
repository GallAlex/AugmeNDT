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
                    case "RealX1 [µm]":
                        realX1 = Array.ConvertAll(valuesWithoutHeader, s => double.Parse(s, CultureInfo.InvariantCulture));
                        data.Add("RealX1 [µm]", realX1);
                        break;
                    case "RealY1 [µm]":
                        realY1 = Array.ConvertAll(valuesWithoutHeader, s => double.Parse(s, CultureInfo.InvariantCulture));
                        data.Add("RealY1 [µm]", realY1);
                        break;
                    case "RealZ1 [µm]":
                        realZ1 = Array.ConvertAll(valuesWithoutHeader, s => double.Parse(s, CultureInfo.InvariantCulture));
                        data.Add("RealZ1 [µm]", realZ1);
                        break;
                    case "RealX2 [µm]":
                        realX2 = Array.ConvertAll(valuesWithoutHeader, s => double.Parse(s, CultureInfo.InvariantCulture));
                        data.Add("RealX2 [µm]", realX2);
                        break;
                    case "RealY2 [µm]":
                        realY2 = Array.ConvertAll(valuesWithoutHeader, s => double.Parse(s, CultureInfo.InvariantCulture));
                        data.Add("RealY2 [µm]", realY2);
                        break;
                    case "RealZ2 [µm]":
                        realZ2 = Array.ConvertAll(valuesWithoutHeader, s => double.Parse(s, CultureInfo.InvariantCulture));
                        data.Add("RealZ2 [µm]", realZ2);
                        break;
                    case "StraightLength [µm]":
                        straightLength = Array.ConvertAll(valuesWithoutHeader, s => double.Parse(s, CultureInfo.InvariantCulture));
                        data.Add("StraightLength [µm]", straightLength);
                        break;
                    case "CurvedLength [µm]":
                        curvedLength = Array.ConvertAll(valuesWithoutHeader, s => double.Parse(s, CultureInfo.InvariantCulture));
                        break;
                    case "Diameter [µm]":
                        diameter = Array.ConvertAll(valuesWithoutHeader, s => double.Parse(s, CultureInfo.InvariantCulture));
                        data.Add("Diameter [µm]", diameter);
                        break;
                    case "SurfaceArea [µm]µm2]":
                        surfaceArea = Array.ConvertAll(valuesWithoutHeader, s => double.Parse(s, CultureInfo.InvariantCulture));
                        data.Add("SurfaceArea [µm]µm2]", surfaceArea);
                        break;
                    case "Volume [µm]µm3]":
                        volume = Array.ConvertAll(valuesWithoutHeader, s => double.Parse(s, CultureInfo.InvariantCulture));
                        data.Add("Volume [µm]µm3]", volume);
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

        public AbstractDataset ExportForDataVis()
        {
            //TODO: Make specific method to remove entries from file
            Dictionary<string, double[]> reducedData = data; //Used for Vis
            reducedData.Remove("Label");
            reducedData.Remove("Seperated Fibre");
            reducedData.Remove("Curved Fibre");

            return new AbstractDataset(dataSetName, reducedData.Keys.ToList(), reducedData);
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
