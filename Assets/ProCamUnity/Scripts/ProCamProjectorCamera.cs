using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ProCamProjectorCamera : MonoBehaviour
{
    [Header("Calibration")]
    public ProCamCalibration calibration;

    [Header("Projection")]
    public float nearClip = 0.05f;
    public float farClip = 100f;

    [Header("Status")]
    public bool projectionApplied;

    private Camera projectorCamera;

    private void Awake()
    {
        projectorCamera = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (projectionApplied)
            return;

        if (calibration == null ||
            !calibration.calibrationLoaded)
            return;

        ApplyCalibration();
    }

    public void ApplyCalibration()
    {
        var p = calibration.projector;

        if (p.width <= 0 || p.height <= 0 ||
            p.fx <= 0 || p.fy <= 0)
        {
            Debug.LogError("Invalid projector intrinsics.");
            return;
        }

        // Convert calibrated pixel coordinates
        // into physical projection-plane boundaries.

        float left =
            -p.cx * nearClip / p.fx;

        float right =
            (p.width - p.cx) * nearClip / p.fx;

        float top =
            p.cy * nearClip / p.fy;

        float bottom =
            -(p.height - p.cy) * nearClip / p.fy;

        // Construct off-axis projection matrix.
        Matrix4x4 projection = Matrix4x4.Frustum(
            left,
            right,
            bottom,
            top,
            nearClip,
            farClip
        );

        projectorCamera.projectionMatrix = projection;

        projectorCamera.nearClipPlane = nearClip;
        projectorCamera.farClipPlane = farClip;

        projectorCamera.aspect =
            (float)p.width / p.height;

        projectionApplied = true;

        float horizontalFov =
            2f * Mathf.Atan(
                p.width / (2f * p.fx)
            ) * Mathf.Rad2Deg;

        float verticalFov =
            2f * Mathf.Atan(
                p.height / (2f * p.fy)
            ) * Mathf.Rad2Deg;

        Debug.Log(
            "================================\n" +
            "PROJECTOR CALIBRATION APPLIED\n" +
            "================================\n" +
            $"Resolution: {p.width} x {p.height}\n" +
            $"FOV Horizontal: {horizontalFov:F2}°\n" +
            $"FOV Vertical: {verticalFov:F2}°\n" +
            $"Focal Length: ({p.fx:F2}, {p.fy:F2})\n" +
            $"Principal Point: ({p.cx:F2}, {p.cy:F2})\n" +
            $"Distortion: {string.Join(", ", p.distortion)}\n" +
            "================================"
        );
    }
}
