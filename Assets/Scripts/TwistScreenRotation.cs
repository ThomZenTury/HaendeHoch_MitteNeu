using UnityEngine;

public class TwistScreenRotation : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 50f; // Rotation speed in degrees per second
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Time.deltaTime * rotationSpeed, 0, 0); // Rotate around the Z-axis at 50 degrees per second
    }
}
