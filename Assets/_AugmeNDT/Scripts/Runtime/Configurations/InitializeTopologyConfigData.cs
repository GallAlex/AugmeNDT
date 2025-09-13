using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

namespace AugmeNDT
{
    public static class InitializeTopologyConfigData
    {
        private static string configPath = System.IO.Path.Combine(Application.streamingAssetsPath, 
            "topologyConfig.json");

        public static TopologyConfigData LoadTopologyConfiguration()
        {
            if (!System.IO.File.Exists(configPath))
                return CreateTopologyConfiguration();

            string jsonData = System.IO.File.ReadAllText(configPath);
            TopologyConfigData config = JsonUtility.FromJson<TopologyConfigData>(jsonData);
            return config;

        }

        private static TopologyConfigData CreateTopologyConfiguration()
        {
            // Create a new configuration with default values
            TopologyConfigData config = new TopologyConfigData();
            string jsonData = JsonUtility.ToJson(config, true); // true = pretty formatting
            try
            {
                System.IO.File.WriteAllText(configPath, jsonData);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }

            return config;
        }
    }
}
