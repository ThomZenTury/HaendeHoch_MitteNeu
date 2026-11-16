Shader "Hidden/ProjectorLensDistortion"
{
Properties
{
    _MainTex("Main Texture", 2DArray) = "" {}
    // _DebugView("Debug View", Float) = 0
}


    HLSLINCLUDE

    #pragma target 4.5

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/CustomPostProcess.hlsl"

    TEXTURE2D_X(_MainTex);

    float _Fx;
    float _Fy;

    float _Cx;
    float _Cy;

    float _ImageWidth;
    float _ImageHeight;

    float _K1;
    float _Strength;

    float _DebugView;


    float4 CustomPostProcess(Varyings input) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        float2 uv = input.texcoord;


        // =====================================
        // DEBUG VIEW 1: UV GRADIENT
        // =====================================

        if (_DebugView > 0.5 && _DebugView < 1.5)
        {
            return float4(uv.x, uv.y, 0, 1);
        }


        // =====================================
        // DEBUG VIEW 2: SOLID MAGENTA
        // =====================================

        if (_DebugView > 1.5)
        {
            return float4(1, 0, 1, 1);
        }


        // =====================================
        // NORMAL DISTORTION CORRECTION
        // =====================================

        // Convert UV to OpenCV pixel coordinates.

        float2 pixel;

        pixel.x = uv.x * _ImageWidth;
        pixel.y = (1.0 - uv.y) * _ImageHeight;


        // Normalize using projector intrinsics.

        float xd = (pixel.x - _Cx) / _Fx;
        float yd = (pixel.y - _Cy) / _Fy;


        // Inverse radial distortion.

        float x = xd;
        float y = yd;

        [unroll]
        for (int i = 0; i < 8; i++)
        {
            float r2 = x * x + y * y;

            float radial = 1.0 + _K1 * r2;

            x = xd / radial;
            y = yd / radial;
        }


        // Apply correction strength.

        x = lerp(xd, x, _Strength);
        y = lerp(yd, y, _Strength);


        // Convert back to pixel coordinates.

        float2 sourcePixel;

        sourcePixel.x = _Fx * x + _Cx;
        sourcePixel.y = _Fy * y + _Cy;


        // Convert to Unity UV.

        float2 sourceUV;

        sourceUV.x = sourcePixel.x / _ImageWidth;
        sourceUV.y = 1.0 - sourcePixel.y / _ImageHeight;


        // Check boundaries.

        float inside =
            step(0.0, sourceUV.x) *
            step(sourceUV.x, 1.0) *
            step(0.0, sourceUV.y) *
            step(sourceUV.y, 1.0);


        // HDRP RTHandle-aware sampling.

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


        return float4(color.rgb * inside, 1);
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
