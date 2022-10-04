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
        tickMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/Tick");
    }

    public void CreateTicks(Transform axisLine, int numberOfTicks)
    {
        if (tickMarkPrefab == null) SetTickMarkStyle();

        ticks = new GameObject("Ticks");
        ticks.transform.parent = axisLine;

        for (int tick = 0; tick < numberOfTicks; tick++)
        {
            float step = tick / 1.0f;
            Vector3 tickPosition = new Vector3(step + axisLine.position.x, step + axisLine.position.y, step + axisLine.position.z);

            GameObject tickInstance = Instantiate(tickMarkPrefab, tickPosition, Quaternion.identity, ticks.transform);
            
            TextMesh tickLabel = tickInstance.GetComponentInChildren<TextMesh>();
            tickLabel.text = step.ToString();
        }
    }


}
