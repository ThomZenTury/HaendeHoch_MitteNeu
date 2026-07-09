//from here:
// https://www.youtube.com/watch?v=7vvycc2iX6E&list=PLpPd_BKEUoYhN8CiOoNLTSVh-7U5yjg3n&index=43

using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class MeshTrail : MonoBehaviour
{
    public float activeTime = 2f; // Duration for which the trail is active

    [Header("Mesh Related")]
    public float meshFrameRate = 0.5f; // Time interval between mesh captures
    public float meshDestroyDelay = 3f; // Time after which the spawned mesh is destroyed
    public Transform positionToSpawn;

    [Header("Shader Related")]
    public Material mat;
    public String shaderVarRef;
    public float shaderVarRate = 0.05f;
    public float shaderVarRefreshRate = 0.02f;

    public bool isTrailActive;
    private SkinnedMeshRenderer[] skinnedMeshRenderers;

    // Update is called once per frame
    void Update()
    {
        // scene transition trigger
        if (Keyboard.current.spaceKey.wasPressedThisFrame && !isTrailActive)
        {
            isTrailActive = true;
            StartCoroutine(ActivateTrail(activeTime));

        }
    }

    public IEnumerator ActivateTrail(float timeActive)
    {
        while (timeActive > 0)
        {
            timeActive -= meshFrameRate;

            if (skinnedMeshRenderers == null)
            {
                skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            }

            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                GameObject gObj = new GameObject();
                gObj.transform.SetPositionAndRotation(positionToSpawn.position, positionToSpawn.rotation);

                MeshRenderer mr = gObj.AddComponent<MeshRenderer>();
                MeshFilter mf = gObj.AddComponent<MeshFilter>();

                Mesh mesh = new Mesh();
                skinnedMeshRenderers[i].BakeMesh(mesh);

                mf.mesh = mesh;
                mr.material = mat;

                StartCoroutine(AnimateMaterialFloat(mr.material, 0, shaderVarRate, shaderVarRefreshRate));

                Destroy(gObj, meshDestroyDelay);
            }

            yield return new WaitForSeconds(meshFrameRate);

        }

        isTrailActive = false;
    }

    IEnumerator AnimateMaterialFloat(Material mat, float goal, float rate, float refreshRate)
    {
        float valueToAnimate = mat.GetFloat(shaderVarRef);


        while (valueToAnimate > goal)
        {
            valueToAnimate -= rate;
            mat.SetFloat(shaderVarRef, valueToAnimate);
            yield return new WaitForSeconds(refreshRate);
        }
    }
}
