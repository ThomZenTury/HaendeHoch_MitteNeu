using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class ProCamOpenNI : MonoBehaviour
{
    // ========================================================
    // CAMERA STATUS
    // ========================================================

    [Header("Camera")]

    public bool cameraInitialized;

    public int depthWidth;
    public int depthHeight;

    public int colorWidth;
    public int colorHeight;


    // ========================================================
    // DEPTH DATA
    // ========================================================

    [Header("Depth Data")]

    public ushort[] depthData;

    public ulong frameTimestamp;

    public uint frameCount;

    public bool depthFrameUpdated;


    // ========================================================
    // RGB DATA
    // ========================================================

    [Header("RGB Data")]

    [Tooltip("RGB888 image buffer, 3 bytes per pixel.")]
    public byte[] colorData;

    public ulong colorFrameTimestamp;

    public uint colorFrameCount;

    public bool colorFrameUpdated;


    // ========================================================
    // DIAGNOSTICS
    // ========================================================

    [Header("Diagnostics")]

    public bool logFrames = false;

    private uint lastLoggedFrame = 0;

    private bool depthErrorReported = false;

    private bool colorErrorReported = false;


    // ========================================================
    // NATIVE FUNCTIONS
    // ========================================================

    [DllImport(
        "ProCamOpenNI",
        CallingConvention = CallingConvention.Cdecl
    )]
    private static extern int ProCam_Initialize();


    [DllImport(
        "ProCamOpenNI",
        CallingConvention = CallingConvention.Cdecl
    )]
    private static extern int ProCam_GetDepthWidth();


    [DllImport(
        "ProCamOpenNI",
        CallingConvention = CallingConvention.Cdecl
    )]
    private static extern int ProCam_GetDepthHeight();


    [DllImport(
        "ProCamOpenNI",
        CallingConvention = CallingConvention.Cdecl
    )]
    private static extern int ProCam_GetColorWidth();


    [DllImport(
        "ProCamOpenNI",
        CallingConvention = CallingConvention.Cdecl
    )]
    private static extern int ProCam_GetColorHeight();


    [DllImport(
        "ProCamOpenNI",
        CallingConvention = CallingConvention.Cdecl
    )]
    private static extern int ProCam_GetDepthFrame(
        [Out] ushort[] destination,
        int bufferLength,
        out ulong timestamp
    );


    [DllImport(
        "ProCamOpenNI",
        CallingConvention = CallingConvention.Cdecl
    )]
    private static extern int ProCam_GetColorFrame(
        [Out] byte[] destination,
        int bufferLength,
        out ulong timestamp
    );


    [DllImport(
        "ProCamOpenNI",
        CallingConvention = CallingConvention.Cdecl
    )]
    private static extern IntPtr ProCam_GetLastError();


    [DllImport(
        "ProCamOpenNI",
        CallingConvention = CallingConvention.Cdecl
    )]
    private static extern void ProCam_Shutdown();


    // ========================================================
    // INITIALIZATION
    // ========================================================

    void Start()
    {
        InitializeCamera();
    }


    public void InitializeCamera()
    {
        if (cameraInitialized)
            return;

        Debug.Log("Initializing Xtion through OpenNI2...");

        int result = ProCam_Initialize();

        if (result != 1)
        {
            Debug.LogError(
                "OpenNI2 initialization failed: " +
                GetNativeError() +
                " Error code: " + result
            );

            cameraInitialized = false;

            return;
        }


        // ----------------------------------------------------
        // GET DEPTH RESOLUTION
        // ----------------------------------------------------

        depthWidth = ProCam_GetDepthWidth();

        depthHeight = ProCam_GetDepthHeight();


        // ----------------------------------------------------
        // GET RGB RESOLUTION
        // ----------------------------------------------------

        colorWidth = ProCam_GetColorWidth();

        colorHeight = ProCam_GetColorHeight();


        if (depthWidth <= 0 ||
            depthHeight <= 0 ||
            colorWidth <= 0 ||
            colorHeight <= 0)
        {
            Debug.LogError(
                "Invalid camera resolution."
            );

            ProCam_Shutdown();

            return;
        }


        // ----------------------------------------------------
        // ALLOCATE BUFFERS
        // ----------------------------------------------------

        depthData = new ushort[
            depthWidth * depthHeight
        ];

        colorData = new byte[
            colorWidth * colorHeight * 3
        ];


        // ----------------------------------------------------
        // CAMERA READY
        // ----------------------------------------------------

        cameraInitialized = true;

        depthFrameUpdated = false;

        colorFrameUpdated = false;


        Debug.Log(
            "====================================\n" +
            "XTION INITIALIZED\n" +
            "====================================\n" +
            $"Depth: {depthWidth} x {depthHeight}\n" +
            $"RGB: {colorWidth} x {colorHeight}\n" +
            "RGB Format: RGB888\n" +
            "===================================="
        );
    }


    // ========================================================
    // FRAME ACQUISITION
    // ========================================================

    void Update()
    {
        depthFrameUpdated = false;

        colorFrameUpdated = false;

        if (!cameraInitialized)
            return;


        // ----------------------------------------------------
        // DEPTH FRAME
        // ----------------------------------------------------

        int depthResult = ProCam_GetDepthFrame(
            depthData,
            depthData.Length,
            out frameTimestamp
        );


        if (depthResult == 1)
        {
            frameCount++;

            depthFrameUpdated = true;
        }
        else if (depthResult < 0 &&
                 !depthErrorReported)
        {
            Debug.LogError(
                "OpenNI2 depth acquisition failed: " +
                GetNativeError() +
                " Error code: " + depthResult
            );

            depthErrorReported = true;
        }


        // ----------------------------------------------------
        // RGB FRAME
        // ----------------------------------------------------

        int colorResult = ProCam_GetColorFrame(
            colorData,
            colorData.Length,
            out colorFrameTimestamp
        );


        if (colorResult == 1)
        {
            colorFrameCount++;

            colorFrameUpdated = true;
        }
        else if (colorResult < 0 &&
                 !colorErrorReported)
        {
            Debug.LogError(
                "OpenNI2 RGB acquisition failed: " +
                GetNativeError() +
                " Error code: " + colorResult
            );

            colorErrorReported = true;
        }


        // ----------------------------------------------------
        // OPTIONAL FRAME LOGGING
        // ----------------------------------------------------

        if (logFrames &&
            frameCount > 0 &&
            frameCount % 30 == 0 &&
            frameCount != lastLoggedFrame)
        {
            lastLoggedFrame = frameCount;

            Debug.Log(
                $"Xtion Depth Frames: {frameCount} | " +
                $"RGB Frames: {colorFrameCount}"
            );
        }

    }


    // ========================================================
    // ERROR STRING
    // ========================================================

    private string GetNativeError()
    {
        IntPtr pointer = ProCam_GetLastError();

        if (pointer == IntPtr.Zero)
            return "Unknown OpenNI2 error.";

        return Marshal.PtrToStringAnsi(pointer);
    }


    // ========================================================
    // SHUTDOWN
    // ========================================================

    void OnApplicationQuit()
    {
        ShutdownCamera();
    }


    void OnDisable()
    {
        ShutdownCamera();
    }


    public void ShutdownCamera()
    {
        if (!cameraInitialized)
            return;

        ProCam_Shutdown();

        cameraInitialized = false;

        depthFrameUpdated = false;

        colorFrameUpdated = false;

        Debug.Log("Xtion OpenNI2 shutdown.");
    }
}
