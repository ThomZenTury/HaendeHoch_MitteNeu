using System;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
[ExecuteInEditMode]
public class VFX_SpeedControl : MonoBehaviour
{
    [Range(0.0f, 3.4f)] public float SimulationTimeScale = 1.0f;

    private VisualEffect Graph;

    private void OnValidate()
    {
        Graph = gameObject.GetComponent<VisualEffect>();
    }

    private void Update()
    {
        Graph.playRate = SimulationTimeScale;
    }
}