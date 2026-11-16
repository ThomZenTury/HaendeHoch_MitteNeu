using UnityEngine;

public enum ProjectionMode
{
    PhysicalCamera,
    CalibratedMatrix
}

[RequireComponent(typeof(Camera))]
public class ProCamProjectorCamera : MonoBehaviour
{
    [Header("Calibration")]
    public ProCamCalibration calibration;

    [Header("Projection")]
    public ProjectionMode projectionMode =
        ProjectionMode.PhysicalCamera;

    public float nearClip = 0.05f;
    public float farClip = 100f;

    [Header("Physical Camera Calibration")]

    public Vector2 physicalSensorSize =
        new Vector2(36f, 20.42268f);

    public float physicalFocalLength = 23.73492f;

    public Vector2 physicalLensShift =
        new Vector2(0.002994f, -0.609961f);

    [Header("Status")]
    public bool projectionApplied;

    private Camera projectorCamera;

    private ProjectionMode appliedMode;

    private int appliedCalibrationVersion = -1;


    // private ProjectionMode appliedMode;

    [Header("Physical Camera Settings")]

    [Tooltip("Reference sensor width in millimetres.")]
    public float referenceSensorWidth = 36f;

    [ContextMenu("Reload Projector Calibration")]
    public void ReloadProjectorCalibration()
    {
        if (calibration == null)
        {
            Debug.LogError("No calibration component assigned.");
            return;
        }

        calibration.LoadCalibration();

        Debug.Log("Projector calibration reload requested.");
    }



    private void Awake()
    {
        projectorCamera = GetComponent<Camera>();

        projectorCamera.usePhysicalProperties = true;
    }

    private void LateUpdate()
    {
        if (calibration == null ||
            !calibration.calibrationLoaded)
            return;

        bool calibrationChanged =
            appliedCalibrationVersion !=
            calibration.calibrationVersion;

        bool modeChanged =
            appliedMode != projectionMode;

        if (!projectionApplied ||
            calibrationChanged ||
            modeChanged)
        {
            ApplyCalibration();
        }
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

        // ==========================================
        // 1. CALCULATE PHYSICAL CAMERA PARAMETERS
        // ==========================================

        float sensorWidth = referenceSensorWidth;

        float focalLength =
            p.fx * sensorWidth / p.width;

        float sensorHeight =
            focalLength * p.height / p.fy;

        float shiftX =
            (p.cx - p.width * 0.5f) / p.width;

        float shiftY =
            (p.cy - p.height * 0.5f) / p.height;

        Vector2 sensorSize =
            new Vector2(sensorWidth, sensorHeight);

        Vector2 lensShift =
            new Vector2(shiftX, shiftY);

        // ==========================================
        // 2. APPLY COMMON CAMERA SETTINGS
        // ==========================================

        projectorCamera.nearClipPlane = nearClip;
        projectorCamera.farClipPlane = farClip;

        projectorCamera.aspect =
            (float)p.width / p.height;

        projectorCamera.usePhysicalProperties = true;

        projectorCamera.gateFit = Camera.GateFitMode.None;

        // ==========================================
        // 3. APPLY PHYSICAL CAMERA PARAMETERS
        // ==========================================

        projectorCamera.sensorSize = sensorSize;
        projectorCamera.focalLength = focalLength;
        projectorCamera.lensShift = lensShift;

        // ==========================================
        // 4. SELECT PROJECTION METHOD
        // ==========================================

        projectorCamera.ResetProjectionMatrix();

        switch (projectionMode)
        {
            case ProjectionMode.PhysicalCamera:

                ApplyPhysicalCamera();

                break;

            case ProjectionMode.CalibratedMatrix:

                ApplyCalibratedMatrix();

                break;
        }

        // ==========================================
        // 5. UPDATE STATUS
        // ==========================================

        appliedMode = projectionMode;

        appliedCalibrationVersion =
            calibration.calibrationVersion;

        projectionApplied = true;

        // ==========================================
        // 6. DEBUG INFORMATION
        // ==========================================

        Debug.Log(
            "================================\n" +
            "PROJECTOR CALIBRATION UPDATED\n" +
            "================================\n" +
            $"Mode: {projectionMode}\n" +
            $"Resolution: {p.width} x {p.height}\n" +
            $"fx: {p.fx:F4}\n" +
            $"fy: {p.fy:F4}\n" +
            $"cx: {p.cx:F4}\n" +
            $"cy: {p.cy:F4}\n" +
            "--------------------------------\n" +
            $"Sensor Size: {sensorSize}\n" +
            $"Focal Length: {focalLength:F5} mm\n" +
            $"Lens Shift: {lensShift}\n" +
            "================================"
        );
    }


    private void ApplyPhysicalCamera()
    {
        projectorCamera.usePhysicalProperties = true;

        // Use the calculated physical parameters.
        // Do not overwrite them with fixed values.

        projectorCamera.ResetProjectionMatrix();

        Debug.Log("Using Unity Physical Camera projection.");
    }


    private void ApplyCalibratedMatrix()
    {
        var p = calibration.projector;

        float left =
            -p.cx * nearClip / p.fx;

        float right =
            (p.width - p.cx) * nearClip / p.fx;

        float top =
            p.cy * nearClip / p.fy;

        float bottom =
            -(p.height - p.cy) * nearClip / p.fy;

        Matrix4x4 projection = Matrix4x4.Frustum(
            left,
            right,
            bottom,
            top,
            nearClip,
            farClip
        );

        projectorCamera.projectionMatrix = projection;

        Debug.Log("Using original calibrated projection matrix.");
    }
}
