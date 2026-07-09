using UnityEngine;

public class SimpleRotateAround : MonoBehaviour

{
    public Transform m_pointA;
    public float radiusReduction = 1.0f;
    public Vector3 offset = Vector3.zero;

    public float Amplitude = 100.0f;
    public float Frequency = 2.0f;
    public float rotOffsetX = 0.0f;
    public float rotOffsetY = 0.0f;
    public float rotOffsetZ = 0.0f;
    private float m_angle;
    private Vector3 m_offset;
    Vector3 pointA;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Set positions for the A and B points
        pointA = new Vector3(0f, 0f, 0f);

    }

    // Update is called once per frame
    void Update()
    {
        // Get the time delta
        float delta = Time.deltaTime;
        // Use the time delta to get a circular position around pointTarget
        m_offset.x = Mathf.Cos(m_angle) + offset.x;
        m_offset.y = Mathf.Sin(m_angle) + offset.y;

        // Set the position of the two rotating points
        pointA = m_offset * radiusReduction;          // Set the position on the outer edge of the circle


        // Update the angle (must stay between -PI and +PI)
        m_angle += delta;
        if (m_angle > Mathf.PI)
            m_angle -= Mathf.PI * 2.0f;

        // Set the position of the gameobjects in the scene
        m_pointA.position = new Vector3(pointA.x, pointA.y, offset.z);

        // rotations
        float angleX = Mathf.Sin(Time.time * Frequency) * Amplitude + rotOffsetX;
        float angleY = Mathf.Sin(Time.time * Frequency) * Amplitude + rotOffsetY;
        float angleZ = Mathf.Sin(Time.time * Frequency) * Amplitude + rotOffsetZ;
        // m_pointA.rotation.y = angle;
        m_pointA.rotation = Quaternion.Euler(m_pointA.eulerAngles.x, angleY, angleZ);
        // Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
        // m_pointA.rotation = Quaternion.identity * rotation;
    }
}

