using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[Serializable]
[VolumeComponentMenu("Post-processing/Projector Lens Distortion")]
public sealed class ProjectorLensDistortion : CustomPostProcessVolumeComponent
{
    // =============================================
    // ENABLE / DISABLE
    // =============================================

    [Header("Distortion")]

    public BoolParameter enableDistortion =
        new BoolParameter(true);

    public ClampedFloatParameter strength =
        new ClampedFloatParameter(1f, 0f, 1f);


    // =============================================
    // PROJECTOR CAMERA INTRINSICS
    // =============================================

    [Header("Projector Intrinsics")]

    public FloatParameter fx =
        new FloatParameter(1265.8626f);

    public FloatParameter fy =
        new FloatParameter(1255.15936f);

    public FloatParameter cx =
        new FloatParameter(965.74793f);

    public FloatParameter cy =
        new FloatParameter(-118.757617f);


    // =============================================
    // PROJECTOR RESOLUTION
    // =============================================

    [Header("Projector Resolution")]

    public ClampedIntParameter imageWidth =
        new ClampedIntParameter(1920, 1, 8192);

    public ClampedIntParameter imageHeight =
        new ClampedIntParameter(1080, 1, 8192);


    // =============================================
    // DISTORTION COEFFICIENT
    // =============================================

    [Header("D_simple Radial Distortion")]

    public FloatParameter k1 =
        new FloatParameter(0.17089973f);


    // =============================================
    // INTERNAL
    // =============================================

    private Material material;

    private const string ShaderName =
        "Hidden/ProjectorLensDistortion";


    // =============================================
    // HDRP INJECTION POINT
    // =============================================

    public override CustomPostProcessInjectionPoint injectionPoint =>
        CustomPostProcessInjectionPoint.AfterPostProcess;

    public override bool visibleInSceneView => true;


    // =============================================
    // SETUP
    // =============================================

    public override void Setup()
    {
        Shader shader = Shader.Find(ShaderName);

        if (shader == null)
        {
            Debug.LogError(
                "ProjectorLensDistortion: Shader not found: " +
                ShaderName
            );

            return;
        }

        material = CoreUtils.CreateEngineMaterial(shader);

        Debug.Log(
            "ProjectorLensDistortion initialized."
        );
    }


    // =============================================
    // ACTIVATION
    // =============================================

    public bool IsActive()
    {
        return enableDistortion.value &&
               material != null &&
               strength.value > 0f &&
               fx.value > 0f &&
               fy.value > 0f;
    }


    // =============================================
    // RENDER
    // =============================================

    public override void Render(
        CommandBuffer cmd,
        HDCamera camera,
        RTHandle source,
        RTHandle destination)
    {
        if (material == null)
            return;

        // Source image
        material.SetTexture("_MainTex", source);

        // Camera intrinsics
        material.SetFloat("_Fx", fx.value);
        material.SetFloat("_Fy", fy.value);

        material.SetFloat("_Cx", cx.value);
        material.SetFloat("_Cy", cy.value);

        // Image resolution
        material.SetFloat(
            "_ImageWidth",
            imageWidth.value
        );

        material.SetFloat(
            "_ImageHeight",
            imageHeight.value
        );

        // Radial distortion
        material.SetFloat("_K1", k1.value);

        // Correction strength
        material.SetFloat(
            "_Strength",
            strength.value
        );

        // Execute fullscreen shader
        HDUtils.DrawFullScreen(
            cmd,
            material,
            destination
        );
    }


    // =============================================
    // CLEANUP
    // =============================================

    public override void Cleanup()
    {
        CoreUtils.Destroy(material);
        material = null;
    }
}
