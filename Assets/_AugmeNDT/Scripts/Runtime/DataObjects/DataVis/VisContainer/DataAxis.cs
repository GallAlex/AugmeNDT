using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;

namespace AugmeNDT{
    public enum Direction
    {
        X, // From 0 to +x
        Y, // From 0 to +y 
        Z, // From 0 to +z 
    }

    /// <summary>
    /// Class represents an Axis of a VisContainer with its prefab, scale and ticks
    /// </summary>
    public class DataAxis
    {
        [SerializeField]
        private GameObject axisLinePrefab;

        [SerializeField]
        private GameObject axisInstance;
        [SerializeField]
        private AxisTicks axisTicks;
        [SerializeField]
        //private TextMesh axisLabel;
        private TextMeshPro axisLabel;
    
        public Direction axisDirection = Direction.X;
        public Scale dataScale;

        private float tickOffset;
        private float tickLabelOffset = 0.03f;

        public DataAxis(float tickOffset)
        {
            this.tickOffset = tickOffset;
        }

        public GameObject CreateAxis(Transform visContainer, string axisTitle, Scale dataScale, Direction direction, int numberOfTicks)
        {
            axisLinePrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/VisContainer/Axis2");

            axisInstance = GameObject.Instantiate(axisLinePrefab, visContainer.position, Quaternion.identity, visContainer);

            //axisLabel = axisInstance.GetComponent<TextMesh>();
            //TODO: Add as reference?
            //axisLabel = axisInstance.GetComponentInChildren<TextMesh>();
            //axisLabel.text = axisTitle;
        
            axisLabel = axisInstance.GetComponentInChildren<TextMeshPro>();
            axisLabel.text = axisTitle;
            ConnectAttributeButton();
            
            this.dataScale = dataScale;

            axisDirection = direction;
            axisInstance.name = axisDirection.ToString() + " Axis";

            CreateAxisTicks(axisInstance.transform, dataScale, numberOfTicks);

            ChangeAxisDirection(axisInstance, direction);

            return axisInstance;
        }

        /// <summary>
        /// Method creates the set amount of ticks on the Axis based on the given scale.
        /// If numberOfTicks is zero no tick will be created, otherwise the set amount of ticks will be drawn
        /// </summary>
        /// <param name="axisTransform"></param>
        /// <param name="dataScale"></param>
        /// <param name="numberOfTicks">Amount of ticks inbetween min/max tick</param>
        /// <returns></returns>
        private AxisTicks CreateAxisTicks(Transform axisTransform, Scale dataScale, int numberOfTicks)
        {
            axisTicks = new AxisTicks();

            axisTicks.SetAxisProperties(tickOffset);
            axisTicks.CreateTicks(axisTransform, dataScale, numberOfTicks);

            return axisTicks;
        }

        // TODO: Allow to Change Direction?
        public void ChangeAxis(string axisTitle, Scale dataScale, int numberOfTicks)
        {
            this.dataScale = dataScale;

            axisLabel.text = axisTitle;
            if (numberOfTicks == 1) numberOfTicks = 2;
            axisTicks.ChangeTicks(this.dataScale, numberOfTicks);
        }


        //TODO: rotate Text
        //TODO: Show Axis on closer side and grid on side further away
        private void ChangeAxisDirection(GameObject axis, Direction dir)
        {
            switch (dir)
            {
                case Direction.X:
                    //Nothing To do
                    break;

                case Direction.Y:
                    axis.transform.Rotate(0, 0, 90);    //Rotate 90 degree in z
                    Transform yAxisTitle = axis.transform.Find("Title");
                    yAxisTitle.localPosition = new Vector3(yAxisTitle.localPosition.x, (-yAxisTitle.localPosition.y), yAxisTitle.localPosition.z);

                    foreach (var tickObject in axisTicks.tickList)
                    {
                        Transform tickLabel = tickObject.transform.Find("TickLabel");
                        tickLabel.localPosition = new Vector3(tickLabel.localPosition.x, -tickLabel.localPosition.y, tickLabel.localPosition.z);

                        TextMesh yTickLabelsTextMesh = tickObject.GetComponentInChildren(typeof(TextMesh)) as TextMesh;
                        yTickLabelsTextMesh.anchor = TextAnchor.MiddleRight;
                    }
                    break;

                case Direction.Z:
                    axis.transform.Rotate(0, -90, 0);    //Rotate 90 degree in y
                    //Transform zAxisTitle = axis.transform.Find("Title");
                    //zAxisTitle.localPosition = new Vector3(-zAxisTitle.localPosition.x, zAxisTitle.localPosition.y, zAxisTitle.localPosition.z);
                    //zAxisTitle.Rotate(0, 180, 0);

                    //Todo: Speed up by saving?
                    //TextMesh titleTextMesh = axis.GetComponentInChildren(typeof(TextMesh)) as TextMesh;
                    //titleTextMesh.transform.Rotate(0, 180, 0);
                    //titleTextMesh.anchor = TextAnchor.MiddleRight;
                
                    Transform zAttrButton = axis.transform.Find("Title").Find("AttributeButton");
                    zAttrButton.Rotate(0, 180, 0);
                    var buttonTextMeshPro = axis.GetComponentInChildren(typeof(TextMeshPro)) as TextMeshPro;
                    buttonTextMeshPro.alignment = TextAlignmentOptions.MidlineRight;
                
                    foreach (var tickObject in axisTicks.tickList)
                    {
                        Transform tickLabel = tickObject.transform.Find("TickLabel");
                        tickLabel.Rotate(0, 180, 0);
                    }
                    break;

                default:
                    //Use X
                    break;
            }
        }

        private void ConnectAttributeButton()
        {
            ButtonConfigHelper button = axisInstance.GetComponentInChildren(typeof(ButtonConfigHelper)) as ButtonConfigHelper;
            button.OnClick.AddListener(() => { Debug.Log("Axis " + (int)axisDirection + "pressed"); });
        }
    }
}