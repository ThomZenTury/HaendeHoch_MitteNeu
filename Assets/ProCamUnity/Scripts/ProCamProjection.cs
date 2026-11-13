 
using UnityEngine;

public class ProCamProjection : MonoBehaviour
{
    public ProCamCalibration calibration;

    void Awake()
    {
        if (calibration == null)
        {
            calibration = FindObjectOfType<ProCamCalibration>();
        }
    }

    // -------------------------------------------------
    // RGB CAMERA PROJECTION
    // -------------------------------------------------

    public bool ProjectToRGB(
        Vector3 pointCamera,
        out Vector2 pixel)
    {
        return ProjectPoint(
            pointCamera,
            calibration.rgb,
            out pixel
        );
    }

    // -------------------------------------------------
    // PROJECTOR PROJECTION
    // -------------------------------------------------

    public bool ProjectToProjector(
        Vector3 pointRGB,
        out Vector2 pixel)
    {
        Vector3 pointProjector =
            calibration.TransformRgbToProjector(pointRGB);

        return ProjectPoint(
            pointProjector,
            calibration.projector,
            out pixel
        );
    }

    // -------------------------------------------------
    // DEPTH TO RGB
    // -------------------------------------------------

    public Vector3 DepthToRGB(Vector3 pointDepth)
    {
        return calibration.TransformDepthToRgb(pointDepth);
    }

    // -------------------------------------------------
    // RGB TO DEPTH
    // -------------------------------------------------

    public Vector3 RGBToDepth(Vector3 pointRGB)
    {
        return calibration.TransformRgbToDepth(pointRGB);
    }

    // -------------------------------------------------
    // GENERAL CAMERA PROJECTION
    // -------------------------------------------------

    bool ProjectPoint(
        Vector3 point,
        ProCamCalibration.CameraIntrinsics intrinsics,
        out Vector2 pixel)
    {
        pixel = Vector2.zero;

        if (point.z <= 0.00001f)
            return false;

        float x = point.x / point.z;
        float y = point.y / point.z;

        float k1 = intrinsics.distortion[0];
        float k2 = intrinsics.distortion[1];
        float p1 = intrinsics.distortion[2];
        float p2 = intrinsics.distortion[3];
        float k3 = intrinsics.distortion[4];

        float r2 = x * x + y * y;
        float r4 = r2 * r2;
        float r6 = r4 * r2;

        float radial =
            1.0f +
            k1 * r2 +
            k2 * r4 +
            k3 * r6;

        float xd =
            x * radial +
            2.0f * p1 * x * y +
            p2 * (r2 + 2.0f * x * x);

        float yd =
            y * radial +
            p1 * (r2 + 2.0f * y * y) +
            2.0f * p2 * x * y;

        float u = intrinsics.fx * xd + intrinsics.cx;
        float v = intrinsics.fy * yd + intrinsics.cy;

        pixel = new Vector2(u, v);

        return
            u >= 0 &&
            u < intrinsics.width &&
            v >= 0 &&
            v < intrinsics.height;
    }
}
