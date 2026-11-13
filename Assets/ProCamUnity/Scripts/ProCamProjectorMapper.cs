using UnityEngine;

public class ProCamProjectorMapper : MonoBehaviour
{
    [Header("Calibration")]
    public ProCamCalibration calibration;

    [Header("RGB Camera")]
    public Transform rgbCameraTransform;

    [Header("Projector Camera")]
    public Transform projectorCamera;

    private bool projectorPoseApplied = false;


    [Header("Projector")]
    public bool applyDistortion = true;

    [Header("Debug")]
    public bool showDebug = true;

    public bool IsReady
    {
        get
        {
            return calibration != null &&
                   calibration.calibrationLoaded &&
                   rgbCameraTransform != null;
        }
    }

    public bool TryProjectWorldPoint(
        Vector3 worldPoint,
        out Vector2 projectorPixel)
    {
        projectorPixel = Vector2.zero;

        if (!IsReady)
            return false;

        Vector3 rgbPoint =
            rgbCameraTransform.InverseTransformPoint(worldPoint);

        Vector3 projectorPoint =
            calibration.TransformRgbToProjector(rgbPoint);

        if (projectorPoint.z <= 0.001f)
            return false;

        var intrinsics = calibration.projector;

        float x = projectorPoint.x / projectorPoint.z;
        float y = projectorPoint.y / projectorPoint.z;

        if (applyDistortion &&
            intrinsics.distortion != null &&
            intrinsics.distortion.Length > 0)
        {
            float r2 = x * x + y * y;
            float k1 = intrinsics.distortion[0];

            float radial = 1.0f + k1 * r2;

            x *= radial;
            y *= radial;
        }

        float u = intrinsics.fx * x + intrinsics.cx;
        float v = intrinsics.cy - intrinsics.fy * y;

        projectorPixel = new Vector2(u, v);

        return
            u >= 0 &&
            u < intrinsics.width &&
            v >= 0 &&
            v < intrinsics.height;
    }

    private void LateUpdate()
    {
        if (projectorPoseApplied)
            return;

        if (!IsReady || projectorCamera == null)
            return;

        ApplyProjectorPose();
    }

    private void ApplyProjectorPose()
    {
        // Projector pose expressed in RGB camera coordinates,
        // transformed into Unity world coordinates.
        Matrix4x4 worldFromProjector =
            rgbCameraTransform.localToWorldMatrix *
            calibration.rgbFromProjector;

        Vector3 position =
            worldFromProjector.GetColumn(3);

        Quaternion rotation =
            worldFromProjector.rotation;

        projectorCamera.SetPositionAndRotation(
            position,
            rotation
        );

        projectorPoseApplied = true;

        Debug.Log(
            "PROJECTOR POSE APPLIED\n" +
            "Position: " + position + "\n" +
            "Rotation: " + rotation.eulerAngles
        );
    }


    // public void ApplyProjectorPose()
    // {
    //     if (calibration == null ||
    //         rgbCameraTransform == null ||
    //         projectorCamera == null)
    //     {
    //         Debug.LogError("Missing projector calibration references.");
    //         return;
    //     }

    //     Matrix4x4 worldFromProjector =
    //         rgbCameraTransform.localToWorldMatrix *
    //         calibration.rgbFromProjector;

    //     Vector3 position = worldFromProjector.GetColumn(3);
    //     Quaternion rotation = worldFromProjector.rotation;

    //     projectorCamera.SetPositionAndRotation(position, rotation);

    //     Debug.Log(
    //         $"Projector pose applied: Position={position}, Rotation={rotation.eulerAngles}"
    //     );
    // }


    public Vector2 ProjectorPixelToUV(Vector2 pixel)
    {
        if (!IsReady)
            return Vector2.zero;

        float u = pixel.x / calibration.projector.width;
        float v = 1.0f -
                  pixel.y / calibration.projector.height;

        return new Vector2(u, v);
    }

    public bool TryProjectWorldPointToUV(
        Vector3 worldPoint,
        out Vector2 uv)
    {
        uv = Vector2.zero;

        Vector2 pixel;

        if (!TryProjectWorldPoint(worldPoint, out pixel))
            return false;

        uv = ProjectorPixelToUV(pixel);

        return true;
    }

    public Vector3 WorldToProjector(Vector3 worldPoint)
    {
        if (!IsReady)
            return Vector3.zero;

        Vector3 rgbPoint =
            rgbCameraTransform.InverseTransformPoint(worldPoint);

        return calibration.TransformRgbToProjector(rgbPoint);
    }

    private void OnDrawGizmos()
    {
        if (!showDebug || !IsReady)
            return;

        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            rgbCameraTransform.position,
            0.05f
        );
    }
}
