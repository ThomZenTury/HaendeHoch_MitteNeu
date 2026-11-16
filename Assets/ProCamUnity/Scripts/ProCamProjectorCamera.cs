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

    [Header("Physical Camera Settings")]

    [Tooltip("Reference sensor width in millimetres.")]
    public float referenceSensorWidth = 36f;

    [SerializeField]
    private float calculatedFocalLength;


    [Header("Manual Lens Shift Adjustment")]

    [Tooltip("Additional vertical lens shift in millimetres.")]
    [Range(-50f, 50f)]
    public float verticalShiftOffsetMM = 0f;

    [Tooltip("Original calibrated vertical shift in millimetres.")]
    [SerializeField]
    private float initialVerticalShiftMM;

    [Header("Status")]
    public bool projectionApplied;

    private Camera projectorCamera;

    private ProjectionMode appliedMode;

    private int appliedCalibrationVersion = -1;

    private float appliedVerticalShiftOffsetMM = float.NaN;


    // =====================================================
    // INITIALIZATION
    // =====================================================

    private void Awake()
    {
        projectorCamera = GetComponent<Camera>();

        projectorCamera.usePhysicalProperties = true;
    }


    // =====================================================
    // AUTOMATIC UPDATE
    // =====================================================

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

        bool shiftChanged =
            !Mathf.Approximately(
                appliedVerticalShiftOffsetMM,
                verticalShiftOffsetMM
            );

        if (!projectionApplied ||
            calibrationChanged ||
            modeChanged ||
            shiftChanged)
        {
            ApplyCalibration();
        }
    }


    // =====================================================
    // APPLY CALIBRATION
    // =====================================================

    public void ApplyCalibration()
    {
        if (calibration == null)
            return;

        var p = calibration.projector;

        if (p.width <= 0 ||
            p.height <= 0 ||
            p.fx <= 0 ||
            p.fy <= 0)
        {
            Debug.LogError(
                "ProCamProjectorCamera: Invalid projector intrinsics."
            );

            return;
        }

        // ==============================================
        // 1. CALCULATE PHYSICAL CAMERA PARAMETERS
        // ==============================================

        float sensorWidth = referenceSensorWidth;

        float focalLength =
            p.fx * sensorWidth / p.width;

        calculatedFocalLength = focalLength;


        float sensorHeight =
            focalLength * p.height / p.fy;

        // Original calibrated lens shift
        float calibratedShiftX =
            (p.cx - p.width * 0.5f) / p.width;

        float calibratedShiftY =
            (p.cy - p.height * 0.5f) / p.height;

        // Convert original vertical shift into mm
        float calibratedShiftYMM =
            calibratedShiftY * sensorHeight;

        // Display initial calibration offset
        initialVerticalShiftMM =
            calibratedShiftYMM;

        // ==============================================
        // 2. APPLY MANUAL MILLIMETRE OFFSET
        // ==============================================

        float additionalShiftY =
            verticalShiftOffsetMM / sensorHeight;

        Vector2 lensShift = new Vector2(
            calibratedShiftX,
            calibratedShiftY + additionalShiftY
        );

        Vector2 sensorSize = new Vector2(
            sensorWidth,
            sensorHeight
        );

        // ==============================================
        // 3. APPLY COMMON CAMERA SETTINGS
        // ==============================================

        projectorCamera.nearClipPlane = nearClip;

        projectorCamera.farClipPlane = farClip;

        projectorCamera.aspect =
            (float)p.width / p.height;

        projectorCamera.usePhysicalProperties = true;

        projectorCamera.gateFit =
            Camera.GateFitMode.None;

        projectorCamera.sensorSize = sensorSize;

        projectorCamera.focalLength = focalLength;

        projectorCamera.lensShift = lensShift;

        // ==============================================
        // 4. SELECT PROJECTION METHOD
        // ==============================================

        projectorCamera.ResetProjectionMatrix();

        switch (projectionMode)
        {
            case ProjectionMode.PhysicalCamera:

                ApplyPhysicalCamera(
                    focalLength,
                    sensorSize,
                    lensShift
                );

                break;

            case ProjectionMode.CalibratedMatrix:

                ApplyCalibratedMatrix();

                break;
        }

        // ==============================================
        // 5. UPDATE STATUS
        // ==============================================

        appliedMode = projectionMode;

        appliedCalibrationVersion =
            calibration.calibrationVersion;

        appliedVerticalShiftOffsetMM =
            verticalShiftOffsetMM;

        projectionApplied = true;

        // ==============================================
        // 6. DEBUG INFORMATION
        // ==============================================

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
            $"Initial Vertical Shift: {initialVerticalShiftMM:F4} mm\n" +
            $"Manual Vertical Offset: {verticalShiftOffsetMM:F4} mm\n" +
            $"Effective Vertical Shift: {initialVerticalShiftMM + verticalShiftOffsetMM:F4} mm\n" +
            $"Lens Shift: {lensShift}\n" +
            "================================"
        );
    }


    // =====================================================
    // PHYSICAL CAMERA PROJECTION
    // =====================================================

    private void ApplyPhysicalCamera(
        float focalLength,
        Vector2 sensorSize,
        Vector2 lensShift)
    {
        projectorCamera.usePhysicalProperties = true;

        projectorCamera.sensorSize = sensorSize;

        projectorCamera.focalLength = focalLength;

        projectorCamera.lensShift = lensShift;

        projectorCamera.gateFit =
            Camera.GateFitMode.None;

        var p = calibration.projector;

        Matrix4x4 physicalProjection;

        Camera.CalculateProjectionMatrixFromPhysicalProperties(
            out physicalProjection,
            focalLength,
            sensorSize,
            lensShift,
            nearClip,
            farClip,
            new Camera.GateFitParameters(
                Camera.GateFitMode.None,
                (float)p.width / p.height
            )
        );

        projectorCamera.projectionMatrix =
            physicalProjection;

        Debug.Log(
            "Using Unity Physical Camera projection.\n" +
            $"Projection m02: {physicalProjection.m02}\n" +
            $"Projection m12: {physicalProjection.m12}"
        );
    }


    // =====================================================
    // ORIGINAL CALIBRATED MATRIX PROJECTION
    // =====================================================

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

        projectorCamera.projectionMatrix =
            projection;

        Debug.Log(
            "Using original calibrated projection matrix."
        );
    }


    // =====================================================
    // MANUAL CALIBRATION RELOAD
    // =====================================================

    [ContextMenu("Reload Projector Calibration")]
    public void ReloadProjectorCalibration()
    {
        if (calibration == null)
        {
            Debug.LogError(
                "ProCamProjectorCamera: No calibration component assigned."
            );

            return;
        }

        calibration.LoadCalibration();

        Debug.Log(
            "Projector calibration reload requested."
        );
    }
}
