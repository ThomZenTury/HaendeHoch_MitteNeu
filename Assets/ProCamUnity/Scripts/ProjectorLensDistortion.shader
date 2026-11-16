Shader "Hidden/ProjectorLensDistortion"
{
    HLSLINCLUDE

    #pragma target 4.5

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/CustomPostProcess.hlsl"

    // HDRP source texture
    TEXTURE2D_X(_MainTex);

    // Camera intrinsics
    float _Fx;
    float _Fy;

    float _Cx;
    float _Cy;

    // Calibration resolution
    float _ImageWidth;
    float _ImageHeight;

    // Radial distortion
    float _K1;

    // Correction strength
    float _Strength;


    // =============================================
    // DISTORTION CORRECTION
    // =============================================

    float4 CustomPostProcess(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        // Original output UV
        float2 uv = input.texcoord;

        // Convert Unity UV to OpenCV pixel coordinates.
        // OpenCV uses top-left image origin.

        float2 pixel;

        pixel.x = uv.x * _ImageWidth;

        pixel.y = (1.0 - uv.y) * _ImageHeight;


        // -----------------------------------------
        // Normalize using camera intrinsics
        // -----------------------------------------

        float x = (pixel.x - _Cx) / _Fx;

        float y = (pixel.y - _Cy) / _Fy;


        // -----------------------------------------
        // D_simple radial distortion
        // -----------------------------------------

        float r2 = x * x + y * y;

        float radial =
            1.0 + _K1 * r2;

        float distortedX = x * radial;

        float distortedY = y * radial;


        // -----------------------------------------
        // Apply correction strength
        // -----------------------------------------

        x = lerp(x, distortedX, _Strength);

        y = lerp(y, distortedY, _Strength);


        // -----------------------------------------
        // Convert back to pixel coordinates
        // -----------------------------------------

        float2 sourcePixel;

        sourcePixel.x = _Fx * x + _Cx;

        sourcePixel.y = _Fy * y + _Cy;


        // -----------------------------------------
        // Convert back to Unity UV
        // -----------------------------------------

        float2 sourceUV;

        sourceUV.x =
            sourcePixel.x / _ImageWidth;

        sourceUV.y =
            1.0 - sourcePixel.y / _ImageHeight;


        // -----------------------------------------
        // Image boundaries
        // -----------------------------------------

        float inside =
            step(0.0, sourceUV.x) *
            step(sourceUV.x, 1.0) *
            step(0.0, sourceUV.y) *
            step(sourceUV.y, 1.0);


        // -----------------------------------------
        // HDRP RTHandle-aware texture sampling
        // -----------------------------------------

        float2 sampleUV =
            ClampAndScaleUVForBilinearPostProcessTexture(
                sourceUV
            );

        float4 color =
            SAMPLE_TEXTURE2D_X(
                _MainTex,
                s_linear_clamp_sampler,
                sampleUV
            );


        // Black outside image boundaries
        color.rgb *= inside;

        return color;
    }

    ENDHLSL


    SubShader
    {
        Pass
        {
            Name "ProjectorLensDistortion"

            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment CustomPostProcess

            ENDHLSL
        }
    }

    Fallback Off
}
