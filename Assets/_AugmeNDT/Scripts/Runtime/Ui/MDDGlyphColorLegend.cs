using System.Collections.Generic;
using UnityEngine;

public class MDDGlyphColorLegend
{
    public GameObject colorLegendInstance;
    public GameObject colorLegendPrefab;
    public GameObject text3DPrefab;

    private Texture[] texturesToSet;

    public MDDGlyphColorLegend()
    {
        text3DPrefab = (GameObject)Resources.Load("Prefabs/Text3D");
        colorLegendPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/MDDGlyphColorLegend");

        texturesToSet = new Texture[7];
        texturesToSet[0] = (Texture)Resources.Load("Icons/RightSkewed_peaky");
        texturesToSet[1] = (Texture)Resources.Load("Icons/RightSkewed_flat");
        texturesToSet[2] = (Texture)Resources.Load("Icons/Symmetric_peaky");
        texturesToSet[3] = (Texture)Resources.Load("Icons/Symmetric_normal");
        texturesToSet[4] = (Texture)Resources.Load("Icons/Symmetric_flat");
        texturesToSet[5] = (Texture)Resources.Load("Icons/LeftSkewed_peaky");
        texturesToSet[6] = (Texture)Resources.Load("Icons/LeftSkewed_flat");
    }

    public GameObject CreateColorLegend(Vector3 position, List<Color[]> colorSchemeList)
    {
        colorLegendInstance = GameObject.Instantiate(colorLegendPrefab, position, Quaternion.identity);

        Transform colorBarContainer = colorLegendInstance.transform.Find("ColorBars");

        int currentTex = 0;
        int currentcolorBar = 0;

        foreach (Transform colorBars in colorBarContainer.transform)
        {
            int currentCube = 0;

            foreach (Transform colorCube in colorBars)
            {
                bool hasQuads = false;
                colorCube.GetComponent<Renderer>().material.color = colorSchemeList[currentcolorBar][currentCube];

                foreach (Transform quad in colorCube)
                {
                    hasQuads = true;
                    Debug.Log("Setting texture " + currentTex);
                    quad.GetComponent<Renderer>().material.SetTexture("_MainTex", texturesToSet[currentTex]);
                }

                if(hasQuads) currentTex++;
                currentCube++;
            }

            currentcolorBar++;
        }

        return colorLegendInstance;
    }

}
