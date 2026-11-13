using System.Collections;
using UnityEngine;

public class ProCamDepthProjectorMapper : MonoBehaviour
{
    [Header("Live Camera")]
    public ProCamOpenNI openNI;

    [Header("Calibration")]
    public ProCamCalibration calibration;

    [Header("Depth Settings")]
    [Tooltip("Depth values are interpreted as millimetres.")]
    public float depthMultiplier = 1.0f;

    public float minDepthMM = 200f;
    public float maxDepthMM = 6000f;

    [Header("Projector Settings")]
    public bool applyProjectorDistortion = true;

    [Header("Mapping Diagnostics")]
    public int rejectedInvalidDepth;
    public int rejectedBehindRGB;
    public int rejectedBehindProjector;
    public int rejectedOutsideProjector;
    public int rejectedOther = 0;


    [Header("Debug")]
    public bool debugMapping = false;


    // =========================================================
    // INITIALIZATION
    // =========================================================

    private IEnumerator Start()
    {
        Debug.Log(
            "ProCamDepthProjectorMapper: Initializing..."
        );

        while (calibration == null ||
               !calibration.calibrationLoaded)
        {
            yield return null;
        }

        Debug.Log(
            "ProCamDepthProjectorMapper: Calibration ready."
        );

        while (openNI == null ||
               !openNI.cameraInitialized)
        {
            yield return null;
        }

        Debug.Log(
            "ProCamDepthProjectorMapper: OpenNI camera ready."
        );

        Debug.Log(
            "ProCamDepthProjectorMapper: Live mapping ready."
        );
    }


    // =========================================================
    // LIVE DEPTH PIXEL -> PROJECTOR PIXEL
    // =========================================================

    public bool TryMapDepthPixel(
        Vector2 depthPixel,
        out Vector2 projectorPixel)
    {
        projectorPixel = Vector2.zero;

        if (openNI == null ||
            !openNI.cameraInitialized ||
            openNI.depthData == null)
        {
            return false;
        }

        int x = Mathf.RoundToInt(depthPixel.x);
        int y = Mathf.RoundToInt(depthPixel.y);

        if (x < 0 || x >= openNI.depthWidth ||
            y < 0 || y >= openNI.depthHeight)
        {
            return false;
        }

        int index = y * openNI.depthWidth + x;

        if (index < 0 ||
            index >= openNI.depthData.Length)
        {
            return false;
        }

        ushort depthValue = openNI.depthData[index];

        if (depthValue == 0)
            return false;

        return TryMapDepthPixelToProjectorPixel(
            x,
            y,
            depthValue,
            out projectorPixel
        );
    }


    // =========================================================
    // MAIN MAPPING FUNCTION
    //
    // DEPTH PIXEL
    //     ->
    // DEPTH CAMERA 3D
    //     ->
    // RGB CAMERA 3D
    //     ->
    // PROJECTOR CAMERA 3D
    //     ->
    // PROJECTOR PIXEL
    // =========================================================

    public bool TryMapDepthPixelToProjectorPixel(
    float depthU,
    float depthV,
    float depthValue,
    out Vector2 projectorPixel)
    {
        projectorPixel = Vector2.zero;

        // STEP 0: CALIBRATION CHECK

        if (calibration == null ||
            !calibration.calibrationLoaded)
        {
            rejectedOther++;
            return false;
        }

        // STEP 1: DEPTH VALIDATION

        if (!IsValidDepth(depthValue))
        {
            rejectedInvalidDepth++;
            return false;
        }

        float depthMM = depthValue * depthMultiplier;

        float fx = calibration.depth.fx;
        float fy = calibration.depth.fy;

        float cx = calibration.depth.cx;
        float cy = calibration.depth.cy;

        if (fx <= 0 || fy <= 0)
        {
            rejectedOther++;
            return false;
        }

        // STEP 2: DEPTH PIXEL -> DEPTH CAMERA 3D

        float xDepth =
            (depthU - cx) * depthMM / fx;

        float yDepth =
            -(depthV - cy) * depthMM / fy;

        float zDepth = depthMM;

        Vector3 depthPoint = new Vector3(
            xDepth,
            yDepth,
            zDepth
        ) * calibration.unityUnitsPerMillimetre;

        // STEP 3: DEPTH CAMERA -> RGB CAMERA

        Vector3 rgbPoint =
            calibration.TransformDepthToRgb(depthPoint);

        if (rgbPoint.z <= 0)
        {
            rejectedBehindRGB++;
            return false;
        }

        // STEP 4: RGB CAMERA -> PROJECTOR CAMERA

        Vector3 projectorPoint =
            calibration.TransformRgbToProjector(rgbPoint);

        if (projectorPoint.z <= 0)
        {
            rejectedBehindProjector++;
            return false;
        }

        // STEP 5: PROJECTOR CAMERA -> NORMALIZED IMAGE

        float nx = projectorPoint.x / projectorPoint.z;
        float ny = projectorPoint.y / projectorPoint.z;

        // STEP 6: PROJECTOR DISTORTION (D_simple)

        if (applyProjectorDistortion)
        {
            float k1 = calibration.projector.distortion[0];

            float r2 = nx * nx + ny * ny;

            float radial = 1.0f + k1 * r2;

            nx *= radial;
            ny *= radial;
        }

        // STEP 7: PROJECTOR PIXEL COORDINATES

        float pixelU =
            calibration.projector.fx * nx +
            calibration.projector.cx;

        float pixelV =
            calibration.projector.cy -
            calibration.projector.fy * ny;

        // STEP 8: PROJECTOR BOUNDS

        if (pixelU < 0 ||
            pixelU >= calibration.projector.width ||
            pixelV < 0 ||
            pixelV >= calibration.projector.height)
        {
            rejectedOutsideProjector++;
            return false;
        }

        projectorPixel = new Vector2(pixelU, pixelV);

        if (debugMapping)
        {
            Debug.Log(
                $"Depth ({depthU:F1},{depthV:F1}) " +
                $"[{depthMM:F0} mm] -> " +
                $"Projector ({pixelU:F1},{pixelV:F1})"
            );
        }

        return true;
    }



    // =========================================================
    // DEPTH PIXEL -> RGB CAMERA 3D POINT
    // =========================================================

    public bool TryGetRgbCameraPoint(
        float depthU,
        float depthV,
        float depthValue,
        out Vector3 rgbPoint)
    {
        rgbPoint = Vector3.zero;

        if (calibration == null ||
            !calibration.calibrationLoaded)
        {
            return false;
        }

        if (!IsValidDepth(depthValue))
            return false;

        float depthMM =
            depthValue * depthMultiplier;

        float fx = calibration.depth.fx;
        float fy = calibration.depth.fy;

        if (fx <= 0 || fy <= 0)
            return false;

        float x =
            (depthU - calibration.depth.cx) *
            depthMM / fx;

        float y =
            -(depthV - calibration.depth.cy) *
            depthMM / fy;

        Vector3 depthPoint = new Vector3(
            x,
            y,
            depthMM
        ) * calibration.unityUnitsPerMillimetre;

        rgbPoint =
            calibration.TransformDepthToRgb(depthPoint);

        return rgbPoint.z > 0;
    }


    // =========================================================
    // DEPTH VALIDATION
    // =========================================================

    private bool IsValidDepth(float depthValue)
    {
        if (float.IsNaN(depthValue) ||
            float.IsInfinity(depthValue))
        {
            return false;
        }

        float depthMM =
            depthValue * depthMultiplier;

        return depthMM >= minDepthMM &&
               depthMM <= maxDepthMM;
    }


    // =========================================================
    // DEBUG MAPPING
    // Retained for ProCamDepthMappingTest.cs
    // =========================================================

    public void DebugMapping(
        Vector2 depthPixel,
        float depthMillimetres)
    {
        Debug.Log(
            "========== DEPTH MAPPING TEST =========="
        );

        if (calibration == null ||
            !calibration.calibrationLoaded)
        {
            Debug.LogError(
                "Calibration has not loaded."
            );

            return;
        }

        if (!IsValidDepth(depthMillimetres))
        {
            Debug.LogWarning(
                "Depth outside valid range."
            );

            return;
        }

        Vector3 rgbPoint;

        if (!TryGetRgbCameraPoint(
            depthPixel.x,
            depthPixel.y,
            depthMillimetres,
            out rgbPoint))
        {
            Debug.LogWarning(
                "Could not calculate RGB camera point."
            );

            return;
        }

        Debug.Log(
            $"Depth pixel: {depthPixel}"
        );

        Debug.Log(
            $"Depth: {depthMillimetres} mm"
        );

        Debug.Log(
            $"RGB camera position: {rgbPoint}"
        );

        Vector2 projectorPixel;

        if (!TryMapDepthPixelToProjectorPixel(
            depthPixel.x,
            depthPixel.y,
            depthMillimetres,
            out projectorPixel))
        {
            Debug.LogWarning(
                "Mapping failed or point outside projector bounds."
            );

            return;
        }

        Debug.Log(
            $"Projector pixel: {projectorPixel}"
        );

        Debug.Log(
            "DEPTH MAPPING TEST SUCCESSFUL"
        );

        Debug.Log(
            "========================================"
        );
    }


    // =========================================================
    // PROJECTOR PIXEL -> NORMALIZED UV
    // =========================================================

    public Vector2 ProjectorPixelToUV(
        Vector2 pixel)
    {
        if (calibration == null)
            return Vector2.zero;

        return new Vector2(
            pixel.x / calibration.projector.width,
            pixel.y / calibration.projector.height
        );
    }
}
