using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Vis;

public class AxisTicks
{
    public GameObject tickMarkPrefab;

    public GameObject ticks;
    public List<GameObject> tickList;  // Ticks with labels

    public void SetTickMarkStyle()
    {
        if (tickMarkPrefab == null) tickMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/VisContainer/Tick");
    }

    public void CreateTicks(Transform axisLine, Scale scale, int numberOfTicks)
    {
        if (tickMarkPrefab == null) SetTickMarkStyle();

        ticks = new GameObject("Ticks");
        ticks.transform.parent = axisLine;

        tickList = new List<GameObject>();

        float tickSpacing = GetTickSpacing(numberOfTicks);

        for (int tick = 0; tick <= numberOfTicks; tick++)
        {
            float step = tick * tickSpacing;
            float scaledValue = scale.GetDomainValue(step);

            //Vector3 tickPosition = new Vector3(step + axisLine.position.x, step + axisLine.position.y, step + axisLine.position.z);
            Vector3 tickPosition = new Vector3(step + axisLine.position.x, axisLine.position.y, axisLine.position.z);

            GameObject tickInstance = GameObject.Instantiate(tickMarkPrefab, tickPosition, Quaternion.identity, ticks.transform);
            tickInstance.name = "Tick " + tick;
            tickList.Add(tickInstance);

            TextMesh tickLabel = tickInstance.GetComponentInChildren<TextMesh>();
            tickLabel.text = scaledValue.ToString();
        }
    }

    private float GetTickSpacing(int numberOfTicks)
    {
        return 1.0f / (float)numberOfTicks;
    }

}
