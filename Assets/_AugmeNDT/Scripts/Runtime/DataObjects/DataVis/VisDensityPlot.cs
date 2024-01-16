using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AugmeNDT
{

    public class VisDistributionPlot : Vis
    {
        public VisDistributionPlot()
        {
            title = "DistributionPlot";
            axes = 2;

            //Define Data Mark and Tick Prefab
            dataMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/Marks/Sphere");
            tickMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/VisContainer/Tick");
        }

        public override GameObject CreateVis(GameObject container)
        {
            base.CreateVis(container);

            SetVisParams();

            KernelDensityEstimation kde = new KernelDensityEstimation();
            List<double[]> kdeResult = kde.CalculateKernelDensity(channelEncoding[VisChannel.XPos].GetNumericalVal(), 1, 50);
            Attribute kdeX = new Attribute(channelEncoding[VisChannel.XPos].GetName(), kdeResult[0]);
            Attribute kdeY = new Attribute("kdeX", kdeResult[1]);

            xyzTicks = new[] { 10, 10, 10 };
            visContainer.SetAxisTickNumber(xyzTicks);

            //## 01:  Create Axes and Grids

            // X Axis
            CreateAxis(kdeX, false, Direction.X);
            visContainer.CreateGrid(Direction.X, Direction.Y);

            // Y Axis
            CreateAxis(kdeY, false, Direction.Y);

            //## 02: Set Remaining Vis Channels (Color,...)
            SetChannel(VisChannel.XPos, kdeX, false);
            SetChannel(VisChannel.YPos, kdeY, false);
            SetChannel(VisChannel.Color, kdeY, false);

            //## 03: Draw all Data Points with the provided Channels 
            visContainer.CreateDataMarks(dataMarkPrefab, new[] { 1, 1, 1 });


            //TODO: Create Line in DataMark when Prefab is Line

            int dataMarkPoints = visContainer.dataMarkList.Count;
            // Connect them to form a line
            GameObject lineDataMark = new GameObject("Line");
            lineDataMark.transform.parent = visContainer.dataMarkContainer.transform;
            LineRenderer lineRenderer = lineDataMark.AddComponent<LineRenderer>();

            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.widthMultiplier = 0.002f;
            lineRenderer.useWorldSpace = false;
            lineRenderer.positionCount = dataMarkPoints;

            Vector3[] points = new Vector3[dataMarkPoints];

            for (int i = 0; i < dataMarkPoints; i++)
            {
                points[i] = visContainer.dataMarkList[i].GetDataMarkInstance().transform.localPosition;
                //lr.SetPosition(i+1, visContainer.dataMarkList[i + 1].GetDataMarkInstance().transform.position);
            }
            lineRenderer.SetPositions(points);

            //## 04: Rescale Chart
            visContainerObject.transform.localScale = new Vector3(width, height, depth);

            return visContainerObject;
        }
    }

}
