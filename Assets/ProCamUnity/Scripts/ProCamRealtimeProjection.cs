using UnityEngine;

public class ProCamRealtimeProjection : MonoBehaviour
{
    [Header("References")]
    public ProCamOpenNI openNI;
    public ProCamCalibration calibration;
    [Header("DepthMapper")]
    public ProCamDepthProjectorMapper mapper;

    [Header("Projection")]
    public Renderer targetRenderer;

    [Header("Processing")]
    [Range(1, 8)]
    public int samplingInterval = 2;

    [Range(1, 60)]
    public int updateFrequency = 15;

    [Header("Depth")]
    public float minDepthMM = 400f;
    public float maxDepthMM = 4000f;

    [Header("Display")]
    public bool showDepth = true;

    private Texture2D projectionTexture;
    private Color32[] pixels;

    private float updateTimer;

    private int projectorWidth;
    private int projectorHeight;

    void Start()
    {
        StartCoroutine(InitializeProjection());
    }

    System.Collections.IEnumerator InitializeProjection()
    {
        Debug.Log("Waiting for ProCam calibration...");

        while (calibration == null ||
               !calibration.calibrationLoaded)
        {
            yield return null;
        }

        while (openNI == null ||
               !openNI.cameraInitialized)
        {
            yield return null;
        }

        if (mapper == null || targetRenderer == null)
        {
            Debug.LogError("Missing mapper or target renderer.");
            yield break;
        }

        projectorWidth = calibration.projector.width;
        projectorHeight = calibration.projector.height;

        if (projectorWidth <= 0 || projectorHeight <= 0)
        {
            Debug.LogError(
                $"Invalid projector resolution: " +
                $"{projectorWidth} x {projectorHeight}"
            );

            yield break;
        }

        projectionTexture = new Texture2D(
            projectorWidth,
            projectorHeight,
            TextureFormat.RGBA32,
            false
        );

        projectionTexture.filterMode = FilterMode.Point;
        projectionTexture.wrapMode = TextureWrapMode.Clamp;

        pixels = new Color32[
            projectorWidth * projectorHeight
        ];

        ClearTexture();

        targetRenderer.material.mainTexture = projectionTexture;

        Debug.Log(
            $"Realtime projection initialized: " +
            $"{projectorWidth} x {projectorHeight}"
        );
    }


    void Update()
    {
        if (projectionTexture == null || pixels == null)
            return;

        if (openNI == null ||
            calibration == null ||
            mapper == null)
            return;

        if (!openNI.cameraInitialized ||
            !calibration.calibrationLoaded)
            return;

        updateTimer += Time.deltaTime;

        if (updateTimer < 1f / updateFrequency)
            return;

        updateTimer = 0f;

        ProcessDepthFrame();
    }


    void ProcessDepthFrame()
    {
        if (openNI == null ||
            mapper == null ||
            !openNI.cameraInitialized)
            return;

        int width = openNI.depthWidth;
        int height = openNI.depthHeight;

        ushort[] depthData = openNI.depthData;

        if (depthData == null ||
            depthData.Length < width * height)
            return;

        if (pixels == null ||
            pixels.Length != projectorWidth * projectorHeight)
            return;

        System.Array.Clear(pixels, 0, pixels.Length);

        for (int y = 0; y < height; y += samplingInterval)
        {
            for (int x = 0; x < width; x += samplingInterval)
            {
                int index = y * width + x;

                float depthMM = depthData[index];

                if (depthMM < minDepthMM ||
                    depthMM > maxDepthMM)
                    continue;

                Vector2 projectorPixel;

                if (!mapper.TryMapDepthPixelToProjectorPixel(
                    x,
                    y,
                    depthMM,
                    out projectorPixel))
                    continue;

                int px = Mathf.RoundToInt(projectorPixel.x);
                int py = Mathf.RoundToInt(projectorPixel.y);

                if (px < 0 || px >= projectorWidth ||
                    py < 0 || py >= projectorHeight)
                    continue;

                float brightness = Mathf.InverseLerp(
                    maxDepthMM,
                    minDepthMM,
                    depthMM
                );

                byte value = (byte)(brightness * 255f);

                int textureIndex =
                    (projectorHeight - 1 - py) * projectorWidth + px;

                pixels[textureIndex] = new Color32(
                    value,
                    value,
                    value,
                    255
                );
            }
        }

        projectionTexture.SetPixels32(pixels);
        projectionTexture.Apply(false);
    }


    void ClearPixels()
    {
        if (pixels == null)
            return;

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Color32(0, 0, 0, 255);
        }
    }


    void ClearTexture()
    {
        ClearPixels();

        projectionTexture.SetPixels32(pixels);
        projectionTexture.Apply(false);
    }

    void OnDestroy()
    {
        if (projectionTexture != null)
        {
            Destroy(projectionTexture);
        }
    }
}
