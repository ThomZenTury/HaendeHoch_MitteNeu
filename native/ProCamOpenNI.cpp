#include <OpenNI.h>

#include <cstdint>
#include <cstring>
#include <string>

#if defined(__GNUC__)
#define EXPORT extern "C" __attribute__((visibility("default")))
#else
#define EXPORT extern "C"
#endif

// ============================================================
// OPENNI2 GLOBAL OBJECTS
// ============================================================

static openni::Device device;

static openni::VideoStream depthStream;
static openni::VideoStream colorStream;

static bool initialized = false;
static bool openniInitialized = false;

static int depthWidth = 0;
static int depthHeight = 0;

static int colorWidth = 0;
static int colorHeight = 0;

static std::string lastError;


// ============================================================
// ERROR HANDLING
// ============================================================

static void SetError(const std::string& message)
{
    lastError = message;
}

EXPORT const char* ProCam_GetLastError()
{
    return lastError.c_str();
}


// ============================================================
// CLEANUP
// ============================================================

static void Cleanup()
{
    if (depthStream.isValid())
    {
        depthStream.stop();
        depthStream.destroy();
    }

    if (colorStream.isValid())
    {
        colorStream.stop();
        colorStream.destroy();
    }

    if (device.isValid())
        device.close();

    if (openniInitialized)
    {
        openni::OpenNI::shutdown();
        openniInitialized = false;
    }

    initialized = false;

    depthWidth = 0;
    depthHeight = 0;

    colorWidth = 0;
    colorHeight = 0;
}


// ============================================================
// FIND SUPPORTED VIDEO MODE
// ============================================================

static bool FindVideoMode(
    openni::Device& targetDevice,
    openni::SensorType sensor,
    int requestedWidth,
    int requestedHeight,
    openni::PixelFormat requestedFormat,
    openni::VideoMode& selectedMode)
{
    const openni::SensorInfo* sensorInfo =
        targetDevice.getSensorInfo(sensor);

    if (sensorInfo == nullptr)
        return false;

    const openni::Array<openni::VideoMode>& modes =
        sensorInfo->getSupportedVideoModes();

    for (int i = 0; i < modes.getSize(); ++i)
    {
        const openni::VideoMode& mode = modes[i];

        if (mode.getResolutionX() == requestedWidth &&
            mode.getResolutionY() == requestedHeight &&
            mode.getPixelFormat() == requestedFormat)
        {
            selectedMode = mode;
            return true;
        }
    }

    return false;
}


// ============================================================
// INITIALIZATION
// ============================================================

EXPORT int ProCam_Initialize()
{
    if (initialized)
        return 1;

    openni::Status rc = openni::OpenNI::initialize();

    if (rc != openni::STATUS_OK)
    {
        SetError(openni::OpenNI::getExtendedError());
        return -1;
    }

    openniInitialized = true;

    // --------------------------------------------------------
    // OPEN DEVICE
    // --------------------------------------------------------

    rc = device.open(openni::ANY_DEVICE);

    if (rc != openni::STATUS_OK)
    {
        SetError(openni::OpenNI::getExtendedError());
        Cleanup();
        return -2;
    }

    // --------------------------------------------------------
    // CHECK DEPTH SENSOR
    // --------------------------------------------------------

    if (!device.hasSensor(openni::SENSOR_DEPTH))
    {
        SetError("Device has no depth sensor.");
        Cleanup();
        return -3;
    }

    // --------------------------------------------------------
    // CHECK COLOR SENSOR
    // --------------------------------------------------------

    if (!device.hasSensor(openni::SENSOR_COLOR))
    {
        SetError("Device has no RGB color sensor.");
        Cleanup();
        return -4;
    }

    // --------------------------------------------------------
    // CREATE DEPTH STREAM
    // --------------------------------------------------------

    rc = depthStream.create(
        device,
        openni::SENSOR_DEPTH
    );

    if (rc != openni::STATUS_OK)
    {
        SetError(openni::OpenNI::getExtendedError());
        Cleanup();
        return -5;
    }

    // --------------------------------------------------------
    // SELECT DEPTH MODE
    // 640 x 480 DEPTH_1_MM
    // --------------------------------------------------------

    openni::VideoMode selectedDepthMode;

    if (!FindVideoMode(
        device,
        openni::SENSOR_DEPTH,
        640,
        480,
        openni::PIXEL_FORMAT_DEPTH_1_MM,
        selectedDepthMode))
    {
        SetError(
            "640x480 DEPTH_1_MM mode not supported."
        );

        Cleanup();
        return -6;
    }

    rc = depthStream.setVideoMode(selectedDepthMode);

    if (rc != openni::STATUS_OK)
    {
        SetError(openni::OpenNI::getExtendedError());
        Cleanup();
        return -7;
    }

    // --------------------------------------------------------
    // CREATE RGB STREAM
    // --------------------------------------------------------

    rc = colorStream.create(
        device,
        openni::SENSOR_COLOR
    );

    if (rc != openni::STATUS_OK)
    {
        SetError(openni::OpenNI::getExtendedError());
        Cleanup();
        return -8;
    }

    // --------------------------------------------------------
    // SELECT RGB MODE
    // 1280 x 1024 RGB888
    //
    // This must match the RGB intrinsic calibration.
    // --------------------------------------------------------

    openni::VideoMode selectedColorMode;

    if (!FindVideoMode(
        device,
        openni::SENSOR_COLOR,
        1280,
        1024,
        openni::PIXEL_FORMAT_RGB888,
        selectedColorMode))
    {
        SetError(
            "1280x1024 RGB888 mode not supported. "
            "Check supported Xtion color modes."
        );

        Cleanup();
        return -9;
    }

    rc = colorStream.setVideoMode(selectedColorMode);

    if (rc != openni::STATUS_OK)
    {
        SetError(openni::OpenNI::getExtendedError());
        Cleanup();
        return -10;
    }

    // --------------------------------------------------------
    // START DEPTH STREAM
    // --------------------------------------------------------

    rc = depthStream.start();

    if (rc != openni::STATUS_OK)
    {
        SetError(openni::OpenNI::getExtendedError());
        Cleanup();
        return -11;
    }

    // --------------------------------------------------------
    // START RGB STREAM
    // --------------------------------------------------------

    rc = colorStream.start();

    if (rc != openni::STATUS_OK)
    {
        SetError(openni::OpenNI::getExtendedError());
        Cleanup();
        return -12;
    }

    // --------------------------------------------------------
    // STORE RESOLUTIONS
    // --------------------------------------------------------

    depthWidth = 640;
    depthHeight = 480;

    colorWidth = 1280;
    colorHeight = 1024;

    initialized = true;

    lastError.clear();

    return 1;
}


// ============================================================
// RESOLUTION ACCESSORS
// ============================================================

EXPORT int ProCam_GetDepthWidth()
{
    return depthWidth;
}

EXPORT int ProCam_GetDepthHeight()
{
    return depthHeight;
}

EXPORT int ProCam_GetColorWidth()
{
    return colorWidth;
}

EXPORT int ProCam_GetColorHeight()
{
    return colorHeight;
}


// ============================================================
// DEPTH FRAME ACQUISITION
//
// Returns:
//  1 = frame received
//  0 = no frame available
// -1 = not initialized
// -2 = invalid buffer
// -3 = OpenNI2 error
// ============================================================

EXPORT int ProCam_GetDepthFrame(
    uint16_t* destination,
    int bufferLength,
    uint64_t* timestamp)
{
    if (!initialized)
        return -1;

    if (destination == nullptr)
        return -2;

    const int requiredPixels =
        depthWidth * depthHeight;

    if (bufferLength < requiredPixels)
        return -2;

    int changedIndex = -1;

    openni::VideoStream* streams[] =
    {
        &depthStream
    };

    openni::Status rc =
        openni::OpenNI::waitForAnyStream(
            streams,
            1,
            &changedIndex,
            1
        );

    if (rc == openni::STATUS_TIME_OUT)
        return 0;

    if (rc != openni::STATUS_OK)
    {
        SetError(openni::OpenNI::getExtendedError());
        return -3;
    }

    openni::VideoFrameRef frame;

    rc = depthStream.readFrame(&frame);

    if (rc != openni::STATUS_OK || !frame.isValid())
    {
        SetError(openni::OpenNI::getExtendedError());
        return -3;
    }

    const uint16_t* source =
        static_cast<const uint16_t*>(frame.getData());

    const int sourceStride =
        frame.getStrideInBytes() / sizeof(uint16_t);

    for (int y = 0; y < depthHeight; ++y)
    {
        std::memcpy(
            destination + y * depthWidth,
            source + y * sourceStride,
            depthWidth * sizeof(uint16_t)
        );
    }

    if (timestamp != nullptr)
        *timestamp = frame.getTimestamp();

    lastError.clear();

    return 1;
}


// ============================================================
// RGB FRAME ACQUISITION
//
// RGB888 = 3 bytes per pixel.
//
// Returns:
//  1 = frame received
//  0 = no frame available
// -1 = not initialized
// -2 = invalid buffer
// -3 = OpenNI2 error
// ============================================================

EXPORT int ProCam_GetColorFrame(
    uint8_t* destination,
    int bufferLength,
    uint64_t* timestamp)
{
    if (!initialized)
        return -1;

    if (destination == nullptr)
        return -2;

    const int requiredBytes =
        colorWidth * colorHeight * 3;

    if (bufferLength < requiredBytes)
        return -2;

    int changedIndex = -1;

    openni::VideoStream* streams[] =
    {
        &colorStream
    };

    openni::Status rc =
        openni::OpenNI::waitForAnyStream(
            streams,
            1,
            &changedIndex,
            1
        );

    if (rc == openni::STATUS_TIME_OUT)
        return 0;

    if (rc != openni::STATUS_OK)
    {
        SetError(openni::OpenNI::getExtendedError());
        return -3;
    }

    openni::VideoFrameRef frame;

    rc = colorStream.readFrame(&frame);

    if (rc != openni::STATUS_OK || !frame.isValid())
    {
        SetError(openni::OpenNI::getExtendedError());
        return -3;
    }

    if (frame.getVideoMode().getPixelFormat() !=
        openni::PIXEL_FORMAT_RGB888)
    {
        SetError("Unexpected RGB pixel format.");
        return -3;
    }

    const uint8_t* source =
        static_cast<const uint8_t*>(frame.getData());

    const int sourceStride =
        frame.getStrideInBytes();

    const int rowBytes =
        colorWidth * 3;

    if (sourceStride < rowBytes)
    {
        SetError("Invalid RGB frame stride.");
        return -3;
    }

    // Copy RGB data row by row.
    // Handles padded/non-contiguous frame memory.

    for (int y = 0; y < colorHeight; ++y)
    {
        std::memcpy(
            destination + y * rowBytes,
            source + y * sourceStride,
            rowBytes
        );
    }

    if (timestamp != nullptr)
        *timestamp = frame.getTimestamp();

    lastError.clear();

    return 1;
}


// ============================================================
// SHUTDOWN
// ============================================================

EXPORT void ProCam_Shutdown()
{
    Cleanup();

    lastError.clear();
}
