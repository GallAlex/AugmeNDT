using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxisTicks : MonoBehaviour
{
    public GameObject tickMarkPrefab;

    public GameObject ticks;
    public List<GameObject> tickList;  // Ticks with labels

    public void SetTickMarkStyle()
    {
        tickMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/VisContainer/Tick");
    }

    public void CreateTicks(Transform axisLine, int numberOfTicks)
    {
        if (tickMarkPrefab == null) SetTickMarkStyle();

        ticks = new GameObject("Ticks");
        ticks.transform.parent = axisLine;

        float tickSpacing = GetTickSpacing(numberOfTicks);

        for (int tick = 0; tick <= numberOfTicks; tick++)
        {
            float step = tick * tickSpacing;
            //Vector3 tickPosition = new Vector3(step + axisLine.position.x, step + axisLine.position.y, step + axisLine.position.z);
            Vector3 tickPosition = new Vector3(step + axisLine.position.x, axisLine.position.y, axisLine.position.z);

            GameObject tickInstance = Instantiate(tickMarkPrefab, tickPosition, Quaternion.identity, ticks.transform);
            tickInstance.name = "Tick " + tick;

            TextMesh tickLabel = tickInstance.GetComponentInChildren<TextMesh>();
            tickLabel.text = step.ToString();
        }
    }

    private float GetTickSpacing(int numberOfTicks)
    {
        return 1.0f / (float)numberOfTicks;
    }

}
