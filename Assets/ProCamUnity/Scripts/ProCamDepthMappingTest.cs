using UnityEngine;

public class ProCamDepthMappingTest : MonoBehaviour
{
    [Header("References")]
    public ProCamDepthProjectorMapper mapper;

    [Header("Test Input")]
    public Vector2 depthPixel = new Vector2(320, 240);

    public float depthMillimetres = 2000f;

    [Header("Debug")]
    public bool testOnStart = true;

    private void Start()
    {
        if (testOnStart)
            RunTest();
    }

    [ContextMenu("Run Depth Mapping Test")]
    public void RunTest()
    {
        if (mapper == null)
        {
            Debug.LogError(
                "ProCamDepthProjectorMapper not assigned."
            );

            return;
        }

        mapper.DebugMapping(
            depthPixel,
            depthMillimetres
        );
    }
}
