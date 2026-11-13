using System;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class ProCamCalibration : MonoBehaviour
{
    [Header("Calibration JSON")]
    public TextAsset calibrationJson;

    [Header("Coordinate settings")]
    [Tooltip("Python calibration translations are in mm. Unity normally uses metres.")]
    public float unityUnitsPerMillimetre = 0.001f;

    [Header("Calibration status")]
    public bool calibrationLoaded;
    public string extrinsicsStatus;

    [Serializable]
    public class CameraIntrinsics
    {
        public int width;
        public int height;

        public float fx;
        public float fy;
        public float cx;
        public float cy;

        // k1, k2, p1, p2, k3
        public float[] distortion = new float[5];
    }

    public CameraIntrinsics rgb = new CameraIntrinsics();
    public CameraIntrinsics depth = new CameraIntrinsics();
    public CameraIntrinsics projector = new CameraIntrinsics();

    [Header("Coordinate transformations")]
    public Matrix4x4 rgbFromDepth;
    public Matrix4x4 depthFromRgb;

    public Matrix4x4 projectorFromRgb;
    public Matrix4x4 rgbFromProjector;

    [Header("Depth settings")]
    public float depthScale = 1.0f;

    [Header("Validation")]
    public bool projectorExtrinsicsValidated;
    public float projectorValidationRms;
    public float rgbIntrinsicRms;
    public float rgbdExtrinsicRms;

    public string projectorIntrinsicModel;

    private const int SchemaVersion = 1;

    void Start()
    {
        LoadCalibration();
    }

    public void LoadCalibration()
    {
        calibrationLoaded = false;

        if (calibrationJson == null)
        {
            Debug.LogError("ProCam calibration: No Calibration JSON assigned.");
            return;
        }

        try
        {
            JObject root = JObject.Parse(calibrationJson.text);

            // Schema validation
            int schemaVersion = Required<int>(root, "schema_version");

            if (schemaVersion != SchemaVersion)
            {
                throw new Exception(
                    "Unsupported calibration schema version: " + schemaVersion
                );
            }

            // Camera intrinsics
            rgb = LoadIntrinsics(
                RequiredToken(root, "rgb_camera"),
                "RGB camera"
            );

            depth = LoadIntrinsics(
                RequiredToken(root, "depth_camera"),
                "Depth camera"
            );

            projector = LoadIntrinsics(
                RequiredToken(root, "projector"),
                "Projector"
            );

            // Depth configuration
            JObject depthData = RequiredObject(root, "depth");

            depthScale = Required<float>(depthData, "scale");

            // Extrinsics
            JObject extrinsics = RequiredObject(root, "extrinsics");

            extrinsicsStatus = Required<string>(extrinsics, "status");

            rgbFromDepth = LoadMatrix(
                RequiredToken(extrinsics, "rgb_from_depth")
            );

            depthFromRgb = LoadMatrix(
                RequiredToken(extrinsics, "depth_from_rgb")
            );

            projectorFromRgb = LoadMatrix(
                RequiredToken(extrinsics, "projector_from_rgb")
            );

            rgbFromProjector = LoadMatrix(
                RequiredToken(extrinsics, "rgb_from_projector")
            );

            // Quality information
            JObject quality = RequiredObject(root, "quality");

            rgbIntrinsicRms = Optional<float>(
                quality, "rgb_intrinsic_rms_px", 0f
            );

            rgbdExtrinsicRms = Optional<float>(
                quality, "rgbd_extrinsic_rms_mm", 0f
            );

            projectorIntrinsicModel = Optional<string>(
                quality, "projector_intrinsic_model", "UNKNOWN"
            );

            JObject projectorQuality =
                quality["projector_extrinsics"] as JObject;

            projectorExtrinsicsValidated = false;
            projectorValidationRms = 0f;

            if (projectorQuality != null)
            {
                string status = Optional<string>(
                    projectorQuality, "status", "PROVISIONAL"
                );

                projectorExtrinsicsValidated = status == "VALIDATED";

                JObject validation =
                    projectorQuality["validation"] as JObject;

                if (validation != null)
                {
                    projectorValidationRms = Optional<float>(
                        validation,
                        "combined_rms_px",
                        0f
                    );
                }
            }

            // Transformation consistency
            ValidateTransformation(
                rgbFromDepth,
                depthFromRgb,
                "RGB / Depth"
            );

            ValidateTransformation(
                projectorFromRgb,
                rgbFromProjector,
                "RGB / Projector"
            );

            calibrationLoaded = true;

            Debug.Log("ProCamCalibration: Calibration initialization complete.");


            PrintCalibrationSummary();
        }
        catch (Exception e)
        {
            calibrationLoaded = false;

            Debug.LogError(
                "====================================\n" +
                "PROCAM CALIBRATION LOAD FAILED\n" +
                "====================================\n" +
                e
            );
        }
    }

    // =========================================================
    // INTRINSIC LOADER
    // =========================================================

    CameraIntrinsics LoadIntrinsics(
        JToken token,
        string cameraName
    )
    {
        JObject data = token as JObject;

        if (data == null)
        {
            throw new Exception(
                cameraName + ": Invalid JSON object."
            );
        }

        CameraIntrinsics result = new CameraIntrinsics();

        JObject resolution = RequiredObject(data, "resolution");

        result.width = Required<int>(resolution, "width");
        result.height = Required<int>(resolution, "height");

        JArray K = RequiredArray(data, "camera_matrix");

        if (K.Count != 3 ||
            !(K[0] is JArray) ||
            !(K[1] is JArray) ||
            !(K[2] is JArray))
        {
            throw new Exception(
                cameraName + ": Camera matrix must be 3x3."
            );
        }

        result.fx = (float)K[0][0];
        result.fy = (float)K[1][1];

        result.cx = (float)K[0][2];
        result.cy = (float)K[1][2];

        JArray distortion = RequiredArray(data, "distortion");

        if (distortion.Count < 5)
        {
            throw new Exception(
                cameraName +
                ": Expected at least five distortion coefficients."
            );
        }

        for (int i = 0; i < 5; i++)
        {
            result.distortion[i] = (float)distortion[i];
        }

        if (result.width <= 0 || result.height <= 0)
        {
            throw new Exception(
                cameraName + ": Invalid image resolution."
            );
        }

        return result;
    }

    // =========================================================
    // TRANSFORMATION LOADER
    // =========================================================

    Matrix4x4 LoadMatrix(JToken token)
    {
        JArray rows = token as JArray;

        if (rows == null || rows.Count != 4)
        {
            throw new Exception(
                "Transformation matrix must contain four rows."
            );
        }

        Matrix4x4 matrix = new Matrix4x4();

        for (int row = 0; row < 4; row++)
        {
            JArray columns = rows[row] as JArray;

            if (columns == null || columns.Count != 4)
            {
                throw new Exception(
                    "Transformation matrix must be 4x4."
                );
            }

            for (int col = 0; col < 4; col++)
            {
                matrix[row, col] = (float)columns[col];
            }
        }

        // Matrices already use Unity coordinate convention.
        // Only convert translation from millimetres to metres.

        matrix.m03 *= unityUnitsPerMillimetre;
        matrix.m13 *= unityUnitsPerMillimetre;
        matrix.m23 *= unityUnitsPerMillimetre;

        return matrix;
    }

    // =========================================================
    // MATRIX VALIDATION
    // =========================================================

    void ValidateTransformation(
        Matrix4x4 forward,
        Matrix4x4 inverse,
        string name
    )
    {
        Matrix4x4 identity = forward * inverse;

        float error = 0f;

        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                float expected = row == col ? 1f : 0f;

                error = Mathf.Max(
                    error,
                    Mathf.Abs(identity[row, col] - expected)
                );
            }
        }

        if (error > 0.01f)
        {
            throw new Exception(
                name +
                " transformation consistency check failed. " +
                "Maximum matrix error: " + error
            );
        }
    }

    // =========================================================
    // JSON HELPERS
    // =========================================================

    JToken RequiredToken(JObject obj, string key)
    {
        JToken token = obj[key];

        if (token == null || token.Type == JTokenType.Null)
        {
            throw new Exception(
                "Missing required JSON field: " + key
            );
        }

        return token;
    }

    JObject RequiredObject(JObject obj, string key)
    {
        JObject value = RequiredToken(obj, key) as JObject;

        if (value == null)
        {
            throw new Exception(
                "JSON field must be an object: " + key
            );
        }

        return value;
    }

    JArray RequiredArray(JObject obj, string key)
    {
        JArray value = RequiredToken(obj, key) as JArray;

        if (value == null)
        {
            throw new Exception(
                "JSON field must be an array: " + key
            );
        }

        return value;
    }

    T Required<T>(JObject obj, string key)
    {
        return RequiredToken(obj, key).ToObject<T>();
    }

    T Optional<T>(
        JObject obj,
        string key,
        T defaultValue
    )
    {
        JToken token = obj[key];

        if (token == null || token.Type == JTokenType.Null)
        {
            return defaultValue;
        }

        return token.ToObject<T>();
    }

    // =========================================================
    // PUBLIC TRANSFORMATION FUNCTIONS
    // =========================================================

    public Vector3 TransformRgbToDepth(Vector3 point)
    {
        return depthFromRgb.MultiplyPoint3x4(point);
    }

    public Vector3 TransformDepthToRgb(Vector3 point)
    {
        return rgbFromDepth.MultiplyPoint3x4(point);
    }

    public Vector3 TransformRgbToProjector(Vector3 point)
    {
        return projectorFromRgb.MultiplyPoint3x4(point);
    }

    public Vector3 TransformProjectorToRgb(Vector3 point)
    {
        return rgbFromProjector.MultiplyPoint3x4(point);
    }

    // =========================================================
    // STATUS OUTPUT
    // =========================================================

    void PrintCalibrationSummary()
    {
        Debug.Log(
            "====================================\n" +
            "PROCAM CALIBRATION LOADED\n" +
            "===================================="
        );

        Debug.Log($"RGB: {rgb.width} x {rgb.height}");
        Debug.Log($"Depth: {depth.width} x {depth.height}");
        Debug.Log($"Projector: {projector.width} x {projector.height}");

        Debug.Log($"RGB intrinsic RMS: {rgbIntrinsicRms:F4} px");
        Debug.Log($"RGB-D extrinsic RMS: {rgbdExtrinsicRms:F3} mm");

        Debug.Log($"Projector model: {projectorIntrinsicModel}");

        Debug.Log(
            $"Projector validation RMS: {projectorValidationRms:F3} px"
        );

        Debug.Log($"Extrinsics status: {extrinsicsStatus}");

        Debug.Log(
            $"Projector extrinsics validated: {projectorExtrinsicsValidated}"
        );

        Debug.Log("====================================");
    }
}
