 
using UnityEngine;

public class ProCamCalibrationDebug : MonoBehaviour
{
    public ProCamCalibration calibration;
    public ProCamProjection projection;

    [Header("Test point in RGB camera coordinates")]
    public Vector3 testPoint = new Vector3(0, 0, 2);

    [Header("Results")]
    public Vector2 rgbPixel;
    public Vector2 projectorPixel;

    public bool rgbProjectionValid;
    public bool projectorProjectionValid;

    void Start()
    {
        if (calibration == null)
            calibration = FindObjectOfType<ProCamCalibration>();

        if (projection == null)
            projection = FindObjectOfType<ProCamProjection>();

        Invoke(nameof(RunTest), 0.5f);
    }

    [ContextMenu("Run Calibration Test")]
    public void RunTest()
    {
        if (calibration == null || projection == null)
        {
            Debug.LogError("Calibration components missing.");
            return;
        }

        if (!calibration.calibrationLoaded)
        {
            Debug.LogError("Calibration has not loaded.");
            return;
        }

        rgbProjectionValid =
            projection.ProjectToRGB(
                testPoint,
                out rgbPixel
            );

        projectorProjectionValid =
            projection.ProjectToProjector(
                testPoint,
                out projectorPixel
            );

        Debug.Log("==================================");
        Debug.Log("PROCAM PROJECTION TEST");
        Debug.Log("==================================");

        Debug.Log("3D point: " + testPoint);

        Debug.Log(
            $"RGB pixel: {rgbPixel}, valid: {rgbProjectionValid}"
        );

        Debug.Log(
            $"Projector pixel: {projectorPixel}, " +
            $"valid: {projectorProjectionValid}"
        );

        Debug.Log("==================================");
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawSphere(
            transform.TransformPoint(testPoint),
            0.03f
        );
    }
}
