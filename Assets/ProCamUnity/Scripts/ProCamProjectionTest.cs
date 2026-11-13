using UnityEngine;

public class ProCamProjectionTest : MonoBehaviour
{
    [Header("References")]
    public ProCamProjectorMapper mapper;

    public Transform testTarget;

    private void Update()
    {
        if (mapper == null || testTarget == null)
            return;

        Vector2 uv;

        bool visible = mapper.TryProjectWorldPointToUV(
            testTarget.position,
            out uv
        );

        if (visible)
        {
            Debug.Log("Projector UV: " + uv);
        }
    }
}
