using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

namespace AugmeNDT
{
    public class UseTopologicalDataAnalysis : MonoBehaviour
    {
        private Stopwatch scaleTimer = new Stopwatch();

        private void Start()
        {
            StartCoroutine(CheckForFiberRoutine());
        }

        private IEnumerator CheckForFiberRoutine()
        {
            // Wait one frame (volume object is created during this time)
            yield return null;

            // If volume has not been created yet, keep checking every 0.5s to avoid performance impact
            while (true)
            {
                GameObject volume = GameObject.Find("Volume");
                if (volume != null && volume.activeInHierarchy)
                {
                    // Only add component if it doesn't already exist
                    if (volume.GetComponent<TopologicalDataObject>() == null)
                    {
                        volume.AddComponent<TopologicalDataObject>();

                        BoundsControl boundsControl = volume.transform.parent.gameObject.GetComponent<BoundsControl>();
                        boundsControl.ScaleStopped.AddListener(OnScaleEnded);
                        boundsControl.ScaleStarted.AddListener(OnScaleStarted);

                        ObjectManipulator manipulator = volume.transform.parent.gameObject.GetComponent<ObjectManipulator>();
                        manipulator.OnManipulationEnded.AddListener(OnMovingEnded);
                        manipulator.OnManipulationStarted.AddListener(OnMovingStarted);
                    }

                    yield break; // Exit coroutine once the volume is found and configured
                }
                yield return new WaitForSeconds(0.5f);
            }
        }

        private void OnScaleStarted()
        {
            // Pause 2D flow objects during scaling
            if (RectangleManager.rectangleManager != null)
            {
                if (FlowObject2DManager.Instance != null)
                    FlowObject2DManager.Instance.PauseFlowObject();
            }

            // Pause 3D flow objects during scaling
            if (Rectangle3DManager.rectangle3DManager != null)
            {
                if (FlowObject3DManager.Instance != null)
                    FlowObject3DManager.Instance.PauseFlowObject();
            }
        }

        private void OnScaleEnded()
        {
            // Start timing the update process
            scaleTimer.Reset();
            scaleTimer.Start();

            // Update the topological data after scaling
            TopologicalDataObject.instance.UpdateData();

            // Update 2D rectangle if available
            if (RectangleManager.rectangleManager != null)
            {
                RectangleManager.rectangleManager.UpdateRectangleAfterScaling();

                if (Glyph2DVectorField.Instance != null)
                    Glyph2DVectorField.Instance.ShowArrows();

                if (StreamLine2D.Instance != null)
                    StreamLine2D.Instance.ShowStreamLines(true);

                if (FlowObject2DManager.Instance != null)
                    FlowObject2DManager.Instance.StartFlowObject();
            }

            // Update 3D rectangle if available
            if (Rectangle3DManager.rectangle3DManager != null)
            {
                Rectangle3DManager.rectangle3DManager.UpdateRectangleAfterScaling();

                if (StreamLine3D.Instance != null)
                    StreamLine3D.Instance.ShowStreamLines();

                if (Glyph3DVectorField.instance != null)
                    Glyph3DVectorField.instance.ShowVectorsAndCriticalPoints(true);

                if (FlowObject3DManager.Instance != null)
                    FlowObject3DManager.Instance.StartFlowObject();
            }

            // Stop timer and log the elapsed time for scaling update
            scaleTimer.Stop();
            UnityEngine.Debug.Log($"Scale update duration: {scaleTimer.ElapsedMilliseconds} ms");
        }

        private void OnMovingStarted(ManipulationEventData eventData)
        {
            // Pause flow objects when movement starts
            if (RectangleManager.rectangleManager != null)
            {
                if (FlowObject2DManager.Instance != null)
                    FlowObject2DManager.Instance.PauseFlowObject();
            }

            if (Rectangle3DManager.rectangle3DManager != null)
            {
                if (FlowObject3DManager.Instance != null)
                    FlowObject3DManager.Instance.PauseFlowObject();
            }
        }

        private void OnMovingEnded(ManipulationEventData eventData)
        {
            // Determine if the volume was rotated (by a minimal threshold)
            bool rotated = Quaternion.Angle(TopologicalDataObject.instance.volumeTransform.rotation,
                TopologicalDataObject.instance.lastVolumeRotation) >= 0.1f;

            // Update topological data positions, scale, and rotation
            TopologicalDataObject.instance.UpdateData();

            // Update 2D visuals
            if (RectangleManager.rectangleManager != null)
            {
                RectangleManager.rectangleManager.UpdateRectangleAfterScaling();

                if (rotated)
                {
                    if (Glyph2DVectorField.Instance != null)
                        Glyph2DVectorField.Instance.ShowArrows();

                    if (StreamLine2D.Instance != null)
                        StreamLine2D.Instance.ShowStreamLines(true);
                }
                if (FlowObject2DManager.Instance != null)
                    FlowObject2DManager.Instance.StartFlowObject();
            }

            // Update 3D visuals
            if (Rectangle3DManager.rectangle3DManager != null)
            {
                Rectangle3DManager.rectangle3DManager.UpdateRectangleAfterScaling();

                if (rotated)
                {
                    if (Glyph3DVectorField.instance != null)
                        Glyph3DVectorField.instance.ShowVectorsAndCriticalPoints();

                    if (StreamLine3D.Instance != null)
                        StreamLine3D.Instance.ShowStreamLines(true);
                }

                if (FlowObject3DManager.Instance != null)
                    FlowObject3DManager.Instance.StartFlowObject();
            }
        }
    }
}