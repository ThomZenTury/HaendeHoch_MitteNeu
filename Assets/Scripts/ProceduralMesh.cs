using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]

public class ProceduralMesh : MonoBehaviour
{

    [Header("Scene Transition")]
    [SerializeField] private bool backface;
    [SerializeField] private float triangleWidth;
    [SerializeField] private float triangleHeight;

    Mesh mesh;

    Vector3[] vertices;
    int[] triangles;

    void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MakeMeshData();
        CreateMesh();
    }

    // Update is called once per frame
    void MakeMeshData()
    {
        // create an array of vertices
        // normally must be clockwise when looked at from front view 
        // must our scene is from back, so do the order of vertices CCW
        // additionally, the etriangle has to flipped/mirrored, so change the +/- in width
        if (backface == false) vertices = new Vector3[] { new Vector3(-triangleWidth / 2, 0, 0), new Vector3(triangleWidth / 2, 0, 0), new Vector3(-triangleWidth / 2, triangleHeight, 0) };
        else vertices = new Vector3[] { new Vector3(triangleWidth / 2, 0, 0), new Vector3(triangleWidth / 2, triangleHeight, 0), new Vector3(-triangleWidth / 2, 0, 0) };

        // create an array of inteegers
        triangles = new int[] { 0, 1, 2 };
    }

    void CreateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

    }
}
