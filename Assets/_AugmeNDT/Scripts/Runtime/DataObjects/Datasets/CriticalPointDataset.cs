using UnityEngine;

namespace Assets.Scripts.DataStructure
{
    /// <summary>
    /// The class stores critical points calculated by TTK
    /// </summary>
    public class CriticalPointDataset
    {
        public int ID;
        public int Type; // type of the critical point (source,sink,saddle-1,saddle-2 etc.)
        public Vector3 Position;

        public CriticalPointDataset(int id, int type, Vector3 position)
        {
            ID = id;
            Type = type;
            Position = position;
        }
    }
}
