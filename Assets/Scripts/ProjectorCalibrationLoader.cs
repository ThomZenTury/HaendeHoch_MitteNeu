/* In Unity:
1. Create a folder called StreamingAssets inside your Unity Assets directory and place this file like so:

    Assets
        |
        ├── Scripts
        |     └── ProjectorCalibrationLoader.cs
        |
        ├── StreamingAssets
        |     └── UnityProjectorCalibration.xml
        |
        └── Scenes

2. In your Unity project, the Unity hierarchy should look like this:


    ObservingCamera
        |
        └── ProjectorCamera

3. Configure the Inspector

    1. Create an empty GameObject called CalibrationManager.
    2. Attach ProjectorCalibrationLoader.cs.
    3. Assign your UI Button to Load Calibration Button in Unity Inspector.
    4. Assign the observing camera to Observing Camera in Unity Inspector.
    5. Assign the projector camera to Projector Camera in Unity Inspector.
    6. Set the XML filename to UnityProjectorCalibration.xml.

*/
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Globalization;
using System.IO;
using System.Xml;

public class ProjectorCalibrationLoader : MonoBehaviour
{
    [Header("UI")]
    public Button loadCalibrationButton;

    [Header("Camera Objects")]
    public Camera observingCamera;
    public Camera projectorCamera;

    [Header("Calibration File")]
    public string xmlFileName = "UnityProjectorCalibration.xml";

    [Header("Xtion Measurement")]
    public bool applyXtionDistanceToObservingCamera = true;

    private void Start()
    {
        if (loadCalibrationButton != null)
        {
            loadCalibrationButton.onClick.AddListener(LoadCalibration);
        }
        else
        {
            Debug.LogError("Calibration button not assigned!");
        }
    }

    private void OnDestroy()
    {
        if (loadCalibrationButton != null)
        {
            loadCalibrationButton.onClick.RemoveListener(LoadCalibration);
        }
    }

    public void LoadCalibration()
    {
        if (observingCamera == null || projectorCamera == null)
        {
            Debug.LogError("Both cameras must be assigned!");
            return;
        }

        if (projectorCamera.transform.parent != observingCamera.transform)
        {
            Debug.LogError(
                "Projector camera must be a direct child of observing camera!"
            );
            return;
        }

        string filePath = Path.Combine(
            Application.streamingAssetsPath,
            xmlFileName
        );

        if (!File.Exists(filePath))
        {
            Debug.LogError("Calibration file not found: " + filePath);
            return;
        }

        try
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);

            XmlNode root = xmlDoc.SelectSingleNode(
                "/projectorCalibration"
            );

            if (root == null)
                throw new Exception("Invalid calibration XML!");

            // =====================================
            // 1. READ XTION MEASUREMENT
            // =====================================

            float xtionDistance = 0f;

            if (applyXtionDistanceToObservingCamera)
            {
                XmlNode xtionNode = root.SelectSingleNode(
                    "xtionMeasurement"
                );

                if (xtionNode == null)
                {
                    throw new Exception(
                        "Missing <xtionMeasurement> element!"
                    );
                }

                xtionDistance = ReadFloat(
                    xtionNode,
                    "distance_m"
                );

                if (xtionDistance < 0f ||
                    float.IsNaN(xtionDistance) ||
                    float.IsInfinity(xtionDistance))
                {
                    throw new Exception("Invalid Xtion distance!");
                }
            }

            // =====================================
            // 2. READ PROJECTOR POSITION
            // =====================================

            XmlNode positionNode = root.SelectSingleNode("position");

            Vector3 projectorPosition = new Vector3(
                ReadFloat(positionNode, "x"),
                ReadFloat(positionNode, "y"),
                ReadFloat(positionNode, "z")
            );

            // =====================================
            // 3. READ ROTATION MATRIX
            // =====================================

            XmlNode rotationNode = root.SelectSingleNode("rotation");

            Matrix4x4 rotationMatrix = Matrix4x4.identity;

            rotationMatrix.m00 = ReadFloat(rotationNode, "m00");
            rotationMatrix.m01 = ReadFloat(rotationNode, "m01");
            rotationMatrix.m02 = ReadFloat(rotationNode, "m02");

            rotationMatrix.m10 = ReadFloat(rotationNode, "m10");
            rotationMatrix.m11 = ReadFloat(rotationNode, "m11");
            rotationMatrix.m12 = ReadFloat(rotationNode, "m12");

            rotationMatrix.m20 = ReadFloat(rotationNode, "m20");
            rotationMatrix.m21 = ReadFloat(rotationNode, "m21");
            rotationMatrix.m22 = ReadFloat(rotationNode, "m22");

            // =====================================
            // 4. CONVERT MATRIX TO UNITY ROTATION
            // =====================================

            // Extract projector-local axes expressed
            // in the observing camera coordinate system.

            Vector3 projectorRight = new Vector3(
                rotationMatrix.m00,
                rotationMatrix.m10,
                rotationMatrix.m20
            );

            Vector3 projectorUp = new Vector3(
                rotationMatrix.m01,
                rotationMatrix.m11,
                rotationMatrix.m21
            );

            Vector3 projectorForward = new Vector3(
                rotationMatrix.m02,
                rotationMatrix.m12,
                rotationMatrix.m22
            );

            // Check matrix validity
            if (projectorForward.sqrMagnitude < 0.000001f ||
                projectorUp.sqrMagnitude < 0.000001f)
            {
                throw new Exception("Invalid rotation matrix axes!");
            }

            // Explicitly construct Unity quaternion
            Quaternion projectorRotation = Quaternion.LookRotation(
                projectorForward,
                projectorUp
            );

            // Correct projector optical-axis convention
            projectorRotation =
                projectorRotation * Quaternion.Euler(0f, 180f, 0f);

            // =====================================
            // 5. SET OBSERVING CAMERA POSITION
            // =====================================

            if (applyXtionDistanceToObservingCamera)
            {
                Vector3 observingPosition =
                    observingCamera.transform.position;

                // Set world Z position from Xtion measurement
                observingPosition.z = xtionDistance;

                observingCamera.transform.position =
                    observingPosition;

                Debug.Log(
                    "Xtion distance applied: " +
                    xtionDistance + " m"
                );
            }

            // =====================================
            // 6. APPLY PROJECTOR LOCAL TRANSFORM
            // =====================================

            // Position relative to observing camera
            projectorCamera.transform.localPosition =
                projectorPosition;

            // Rotation relative to observing camera
            projectorCamera.transform.localRotation =
                projectorRotation;

            // =====================================
            // 7. DEBUG OUTPUT
            // =====================================

            Debug.Log("======================================");
            Debug.Log("PROJECTOR CALIBRATION LOADED");
            Debug.Log("======================================");

            Debug.Log("Observing camera world position:");
            Debug.Log(observingCamera.transform.position);

            Debug.Log("Observing camera world rotation:");
            Debug.Log(observingCamera.transform.rotation.eulerAngles);

            Debug.Log("Projector local position:");
            Debug.Log(projectorCamera.transform.localPosition);

            Debug.Log("Projector local rotation:");
            Debug.Log(projectorCamera.transform.localRotation.eulerAngles);

            Debug.Log("Projector world position:");
            Debug.Log(projectorCamera.transform.position);

            Debug.Log("Projector world rotation:");
            Debug.Log(projectorCamera.transform.rotation.eulerAngles);

            Debug.Log("Projector forward:");
            Debug.Log(projectorCamera.transform.forward);

            Debug.Log("Projector right:");
            Debug.Log(projectorCamera.transform.right);

            Debug.Log("Projector up:");
            Debug.Log(projectorCamera.transform.up);

            Debug.Log("======================================");
        }
        catch (Exception e)
        {
            Debug.LogError(
                "Calibration loading failed: " + e.Message
            );
        }
    }

    private float ReadFloat(XmlNode node, string attribute)
    {
        if (node == null || node.Attributes[attribute] == null)
        {
            throw new Exception(
                "Missing XML attribute: " + attribute
            );
        }

        return float.Parse(
            node.Attributes[attribute].Value,
            CultureInfo.InvariantCulture
        );
    }
}
