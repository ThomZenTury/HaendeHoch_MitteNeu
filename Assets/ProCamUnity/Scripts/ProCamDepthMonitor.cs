using UnityEngine;
using UnityEngine.UI;

public class ProCamDepthMonitor : MonoBehaviour
{
    [Header("OpenNI Source")]
    public ProCamOpenNI openNI;

    [Header("UI")]
    public RawImage depthDisplay;

    [Header("Display")]
    public bool invertDepth = false;
    public float displayMinMM = 400f;
    public float displayMaxMM = 4000f;

    [Header("Diagnostics")]
    public float updateInterval = 1f;

    private Texture2D depthTexture;
    private Color32[] pixels;

    private float timer;
    private int lastFrameWidth;
    private int lastFrameHeight;

    void Update()
    {
        if (openNI == null || !openNI.cameraInitialized)
            return;

        if (openNI.depthData == null)
            return;

        int width = openNI.depthWidth;
        int height = openNI.depthHeight;

        int expectedSize = width * height;

        if (openNI.depthData.Length < expectedSize)
            return;

        if (depthTexture == null ||
            width != lastFrameWidth ||
            height != lastFrameHeight)
        {
            InitializeTexture(width, height);
        }

        UpdateDepthTexture();

        timer += Time.deltaTime;

        if (timer >= updateInterval)
        {
            timer = 0f;
            PrintDiagnostics();
        }
    }

    void InitializeTexture(int width, int height)
    {
        lastFrameWidth = width;
        lastFrameHeight = height;

        depthTexture = new Texture2D(
            width,
            height,
            TextureFormat.RGB24,
            false
        );

        depthTexture.filterMode = FilterMode.Point;

        pixels = new Color32[width * height];

        if (depthDisplay != null)
        {
            depthDisplay.texture = depthTexture;
        }

        Debug.Log(
            $"Depth monitor initialized: {width} x {height}"
        );
    }

    void UpdateDepthTexture()
    {
        ushort[] depthData = openNI.depthData;

        int count = depthData.Length;

        for (int i = 0; i < lastFrameWidth * lastFrameHeight; i++)
        {
            float depth = depthData[i];

            float normalized = Mathf.InverseLerp(
                displayMinMM,
                displayMaxMM,
                depth
            );

            if (invertDepth)
                normalized = 1f - normalized;

            byte value = depth == 0
                ? (byte)0
                : (byte)(normalized * 255);

            pixels[i] = new Color32(
                value,
                value,
                value,
                255
            );
        }

        depthTexture.SetPixels32(pixels);
        depthTexture.Apply(false);
    }

    void PrintDiagnostics()
    {
        ushort[] data = openNI.depthData;

        long sum = 0;
        int valid = 0;

        ushort min = ushort.MaxValue;
        ushort max = 0;

        int count = lastFrameWidth * lastFrameHeight;

        for (int i = 0; i < count; i++)
        {
            ushort d = data[i];

            if (d == 0)
                continue;

            sum += d;
            valid++;

            if (d < min)
                min = d;

            if (d > max)
                max = d;
        }

        float average = valid > 0
            ? (float)sum / valid
            : 0f;

        // Debug.Log(
        //     "========== XTION DEPTH STATUS ==========\n" +
        //     $"Resolution: {lastFrameWidth} x {lastFrameHeight}\n" +
        //     $"Valid pixels: {valid} / {count}\n" +
        //     $"Minimum: {min} mm\n" +
        //     $"Maximum: {max} mm\n" +
        //     $"Average: {average:F1} mm\n" +
        //     "========================================"
        // );
    }
}
