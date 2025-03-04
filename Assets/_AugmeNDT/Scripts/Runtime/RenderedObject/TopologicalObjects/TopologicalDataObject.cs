using Assets.Scripts.DataStructure;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AugmeNDT
{
    /// <summary>
    /// Manages topological data, including critical points and gradient information.
    /// Loads data from CSV files and makes it accessible to visualization components.
    /// </summary>
    public class TopologicalDataObject : MonoBehaviour
    {
        public static TopologicalDataObject Instance;
        public List<CriticalPointDataset> criticalPointList = new List<CriticalPointDataset>();
        public List<GradientDataset> gradientList = new List<GradientDataset>();
        public float maxMag = 0; // Maximum gradient magnitude
        
        private void Awake()
        {
            Instance = this;
            LoadCriticalPoints(ParseCSV("Critical_Points"));
            LoadGradients(ParseCSV("gradient"));
        }

        // TODO: It will be implemented more appropriately for the project
        private string[] ParseCSV(string path)
        {
            TextAsset csvFile = Resources.Load<TextAsset>(path);
            if (csvFile != null)
            {
                string[] lines = csvFile.text.Split('\n');
                return lines;
            }
            return null;
        }
        private void LoadCriticalPoints(string[] lines)
        {
            if (lines == null)
                return;

            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(',');

                int id = int.Parse(values[0]); 
                int type = int.Parse(values[1]);
                float x = float.Parse(values[2]);
                float y = float.Parse(values[3]);
                float z = float.Parse(values[4]); 

                if (type <= 3)
                    criticalPointList.Add(new CriticalPointDataset(id, type, new Vector3(x, y, z)));
            }
        }
        private void LoadGradients(string[] lines)
        {
            if (lines == null)
                return;

            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(',');

                if (values.Length < 8) continue;

                float x = float.Parse(values[1]);
                float y = float.Parse(values[2]);
                float z = float.Parse(values[3]);
                float gradX = float.Parse(values[4]);
                float gradY = float.Parse(values[5]);
                float gradZ = float.Parse(values[6]);
                float magnitude = float.Parse(values[7]);

                int Id = gradientList.Count;
                Vector3 position = new Vector3(x, y, z);
                GradientDataset data = new GradientDataset(Id, position, new Vector3(gradX, gradY, gradZ), magnitude);

                gradientList.Add(data);
                maxMag = magnitude > maxMag ? magnitude : maxMag;
            }

            Debug.Log($"Total vectors:{gradientList.Count}");
        }
    }
}
