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
            // Wait for one frame (mhd volume object will be created)
            yield return null;

            // If volume has not been created, wait in a loop
            // checking every 2 seconds to avoid performance impact
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

                    yield break; // Exit the coroutine once we've found and modified the volume
                }
                yield return new WaitForSeconds(0.5f);
            }
        }

        private void OnScaleStarted()
        {
            if (RectangleManager.rectangleManager != null)
            {
                if (FlowObject2DManager.Instance != null)
                    FlowObject2DManager.Instance.PauseFlowObject();
            }
        }
        
        private void OnScaleEnded()
        {
            // Zamanlayıcıyı başlat
            scaleTimer.Reset();
            scaleTimer.Start();

            // Scale güncelleme işlemini yap
            TopologicalDataObject.instance.UpdateData();

            // Rectangle'ı güncelle (eğer rectangle manager mevcutsa)
            if (RectangleManager.rectangleManager != null)
            {
                RectangleManager.rectangleManager.UpdateRectangleAfterScaling();

                if (Glyph2DVectorField.Instance != null && Glyph2DVectorField.Instance.IsVectorDrawn)
                    Glyph2DVectorField.Instance.ShowArrows();

                if (StreamLine2D.Instance != null && StreamLine2D.Instance.IsStreamLineDrawn)
                    StreamLine2D.Instance.ShowStreamLines(true);

                if (FlowObject2DManager.Instance != null)
                    FlowObject2DManager.Instance.StartFlowObject();
            }

            // Zamanlayıcıyı durdur ve süreyi logla
            scaleTimer.Stop();
            UnityEngine.Debug.Log($"Scale güncelleme süresi: {scaleTimer.ElapsedMilliseconds} ms");
        }
        
        private void OnMovingStarted(ManipulationEventData eventData)
        {
            if (RectangleManager.rectangleManager != null)
            {
                if (FlowObject2DManager.Instance != null)
                    FlowObject2DManager.Instance.PauseFlowObject();
            }
        }

        private void OnMovingEnded(ManipulationEventData eventData)
        {
            // Pozisyon güncelleme işlemini yap
            TopologicalDataObject.instance.UpdateData();
            // Rectangle'ı güncelle (eğer rectangle manager mevcutsa)
            if (RectangleManager.rectangleManager != null)
            {
                RectangleManager.rectangleManager.UpdateRectangleAfterScaling();

                if (FlowObject2DManager.Instance != null)
                    FlowObject2DManager.Instance.StartFlowObject();
            }
        }

        
    }
}