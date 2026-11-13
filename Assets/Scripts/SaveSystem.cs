using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

using TMPro;

[System.Serializable]
public class SaveData
{
    public string sssnName;
    public float mastRotLeft;
    public float mastRotRight;

    public float[] gearsLeftPosition;
    public float[] gearsLeftRotation;
    public float[] planetaryRingLeftPosition;
    public float[] planetaryRingLeftRotation;
    public float[] gearsRightPosition;
    public float[] gearsRightRotation;

    public float[] triangleRotLeft;
    public float[] triangleRotRight;
    public float[] trianglePosLeft;
    public float[] trianglePosRight;

    public float[] camPosition;
    public float[] camRotation;
    // public Camera cam;
    public float camSensorX;
    public float camSensorY;
    public float camShiftY;
    public float camFOV;


}



public class SaveSystem : MonoBehaviour
{
    // private static string filePath;

    // [SerializeField] public TMP_InputField camXIn;        // assign in Inspector
    // [SerializeField] public TMP_InputField camYIn;        // assign in Inspector
    // [SerializeField] public TMP_InputField camZIn;
    // [SerializeField] public static TMP_Dropdown saveFilesDropdown; // assign in Inspector

    void Start()
    {
        // filePath = Application.persistentDataPath + "/Save/UserData.json";
    }

    public static void SaveData()
    {

        SaveData data = new SaveData();
        // player.playerName = "Player1";
        // player.playerLevel = 5;
        // player.playerHealth = 100.0f;

        data.sssnName = GameObject.Find("SSSN_name_input").GetComponent<TMP_InputField>().text;

        data.mastRotLeft = SceneTransition.crossMasterRotationLeft;
        data.mastRotRight = SceneTransition.crossMasterRotationRight;

        data.gearsLeftPosition = new float[3];
        data.gearsLeftRotation = new float[3];
        data.planetaryRingLeftPosition = new float[3];
        data.planetaryRingLeftRotation = new float[3];
        data.triangleRotLeft = new float[3];
        data.triangleRotRight = new float[3];
        data.trianglePosLeft = new float[3];
        data.trianglePosRight = new float[3];
        data.gearsRightPosition = new float[3];
        data.gearsRightRotation = new float[3];
        data.camPosition = new float[3];
        data.camRotation = new float[3];

        data.gearsLeftPosition[0] = GameObject.Find("GearSetLeft").transform.position.x;
        data.gearsLeftPosition[1] = GameObject.Find("GearSetLeft").transform.position.y;
        data.gearsLeftPosition[2] = GameObject.Find("GearSetLeft").transform.position.z;
        data.gearsLeftRotation[0] = GameObject.Find("GearSetLeft").transform.rotation.eulerAngles.x;
        data.gearsLeftRotation[1] = GameObject.Find("GearSetLeft").transform.rotation.eulerAngles.y;
        data.gearsLeftRotation[2] = GameObject.Find("GearSetLeft").transform.rotation.eulerAngles.z;

        data.planetaryRingLeftPosition[0] = GameObject.Find("PlanetaryPlanetLeft").transform.position.x;
        data.planetaryRingLeftPosition[1] = GameObject.Find("PlanetaryPlanetLeft").transform.position.y;
        data.planetaryRingLeftPosition[2] = GameObject.Find("PlanetaryPlanetLeft").transform.position.z;
        data.planetaryRingLeftRotation[0] = GameObject.Find("PlanetaryPlanetLeft").transform.rotation.eulerAngles.x;
        data.planetaryRingLeftRotation[1] = GameObject.Find("PlanetaryPlanetLeft").transform.rotation.eulerAngles.y;
        data.planetaryRingLeftRotation[2] = GameObject.Find("PlanetaryPlanetLeft").transform.rotation.eulerAngles.z;

        data.trianglePosLeft[0] = GameObject.Find("TriangleLeft_Cube").transform.position.x;
        data.trianglePosLeft[1] = GameObject.Find("TriangleLeft_Cube").transform.position.y;
        data.trianglePosLeft[2] = GameObject.Find("TriangleLeft_Cube").transform.position.z;

        data.triangleRotLeft[0] = GameObject.Find("TriangleLeft_Cube").transform.rotation.eulerAngles.x;
        data.triangleRotLeft[1] = GameObject.Find("TriangleLeft_Cube").transform.rotation.eulerAngles.y;
        data.triangleRotLeft[2] = GameObject.Find("TriangleLeft_Cube").transform.rotation.eulerAngles.z;

        data.trianglePosRight[0] = GameObject.Find("TriangleRight_Cube").transform.position.x;
        data.trianglePosRight[1] = GameObject.Find("TriangleRight_Cube").transform.position.y;
        data.trianglePosRight[2] = GameObject.Find("TriangleRight_Cube").transform.position.z;

        data.triangleRotRight[0] = GameObject.Find("TriangleRight_Cube").transform.rotation.eulerAngles.x;
        data.triangleRotRight[1] = GameObject.Find("TriangleRight_Cube").transform.rotation.eulerAngles.y;
        data.triangleRotRight[2] = GameObject.Find("TriangleRight_Cube").transform.rotation.eulerAngles.z;


        data.gearsRightPosition[0] = GameObject.Find("GearSetRight").transform.position.x;
        data.gearsRightPosition[1] = GameObject.Find("GearSetRight").transform.position.y;
        data.gearsRightPosition[2] = GameObject.Find("GearSetRight").transform.position.z;
        data.gearsRightRotation[0] = GameObject.Find("GearSetRight").transform.rotation.eulerAngles.x;
        data.gearsRightRotation[1] = GameObject.Find("GearSetRight").transform.rotation.eulerAngles.y;
        data.gearsRightRotation[2] = GameObject.Find("GearSetRight").transform.rotation.eulerAngles.z;

        data.camPosition[0] = GameObject.Find("Main Camera").transform.position.x;
        data.camPosition[1] = GameObject.Find("Main Camera").transform.position.y;
        data.camPosition[2] = GameObject.Find("Main Camera").transform.position.z;
        data.camRotation[0] = GameObject.Find("Main Camera").transform.rotation.eulerAngles.x;
        data.camRotation[1] = GameObject.Find("Main Camera").transform.rotation.eulerAngles.y;
        data.camRotation[2] = GameObject.Find("Main Camera").transform.rotation.eulerAngles.z;

        data.camSensorX = GameObject.Find("Main Camera").GetComponent<Camera>().sensorSize.x;
        data.camSensorY = GameObject.Find("Main Camera").GetComponent<Camera>().sensorSize.y;

        data.camShiftY = GameObject.Find("Main Camera").GetComponent<Camera>().lensShift.y;

        data.camFOV = GameObject.Find("Main Camera").GetComponent<Camera>().fieldOfView;

        TMP_InputField sssnName = GameObject.Find("SSSN_name_input").GetComponent<TMP_InputField>();
        string name = sssnName.text;

        string json = JsonUtility.ToJson(data, true);
        string timestamp = DateTime.Now.ToString("dd.MM.yyyy_HH-mm-ss");

        TMP_InputField storagePathText = GameObject.Find("storagePath_txt").GetComponent<TMP_InputField>();
        storagePathText.text = Application.persistentDataPath + "/Save/";

        if (!Directory.Exists(Application.persistentDataPath + "/Save"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Save");
        }

        if (name != null && name != "")
        {
            File.WriteAllText(Application.persistentDataPath + "/Save/" + name + "_" + timestamp + ".json", json);
            Debug.Log("Data saved to: " + Application.persistentDataPath + "/Save/" + name + "_" + timestamp + ".json");
        }
        else
        {
            File.WriteAllText(Application.persistentDataPath + "/Save/NoName_" + timestamp + ".json", json);
            Debug.Log("Data saved to: " + Application.persistentDataPath + "/Save/NoName_" + timestamp + ".json");
        }

        Debug.Log("Saved Data: " + json);
    }

    public static void LoadData()
    {
        TMP_Dropdown saveFilesDropdown = GameObject.Find("Dropdown_savedFiles").GetComponent<TMP_Dropdown>();

        string fileName = saveFilesDropdown.options[saveFilesDropdown.value].text;


        if (File.Exists(Application.persistentDataPath + "/Save/" + fileName))
        {
            string json = File.ReadAllText(Application.persistentDataPath + "/Save/" + fileName);
            SaveData loadedData = JsonUtility.FromJson<SaveData>(json);
            Debug.Log("Data loaded from: " + Application.persistentDataPath + "/Save/" + fileName);
            Debug.Log("Loaded Data: " + json);

            string sssnName = loadedData.sssnName;

            float mastRotLeft = loadedData.mastRotLeft;
            float mastRotRight = loadedData.mastRotRight;

            float camShiftY = loadedData.camShiftY;

            float camFOV = loadedData.camFOV;

            Vector3 leftPos = new Vector3(
                    loadedData.gearsLeftPosition[0],
                    loadedData.gearsLeftPosition[1],
                    loadedData.gearsLeftPosition[2]
                );
            Vector3 leftRot = new Vector3(
                loadedData.gearsLeftRotation[0],
                loadedData.gearsLeftRotation[1],
                loadedData.gearsLeftRotation[2]
            );
            Vector3 planetaryRingLeftPos = new Vector3(
                loadedData.planetaryRingLeftPosition[0],
                loadedData.planetaryRingLeftPosition[1],
                loadedData.planetaryRingLeftPosition[2]
            );
            Vector3 planetaryRingLeftRot = new Vector3(
                loadedData.planetaryRingLeftRotation[0],
                loadedData.planetaryRingLeftRotation[1],
                loadedData.planetaryRingLeftRotation[2]
            );

            Vector3 trianglePosLeft = new Vector3(
                loadedData.trianglePosLeft[0],
                loadedData.trianglePosLeft[1],
                loadedData.trianglePosLeft[2]
            );
            Vector3 triangleRotLeft = new Vector3(
                loadedData.triangleRotLeft[0],
                loadedData.triangleRotLeft[1],
                loadedData.triangleRotLeft[2]
            );
            Vector3 trianglePosRight = new Vector3(
                loadedData.trianglePosRight[0],
                loadedData.trianglePosRight[1],
                loadedData.trianglePosRight[2]
            );
            Vector3 triangleRotRight = new Vector3(
                loadedData.triangleRotRight[0],
                loadedData.triangleRotRight[1],
                loadedData.triangleRotRight[2]
            );

            Vector3 rightPos = new Vector3(
                loadedData.gearsRightPosition[0],
                loadedData.gearsRightPosition[1],
                loadedData.gearsRightPosition[2]
            );
            Vector3 rightRot = new Vector3(
                loadedData.gearsRightRotation[0],
                loadedData.gearsRightRotation[1],
                loadedData.gearsRightRotation[2]
            );
            Vector3 camPos = new Vector3(
                loadedData.camPosition[0],
                loadedData.camPosition[1],
                loadedData.camPosition[2]
            );
            Vector3 camRot = new Vector3(
                loadedData.camRotation[0],
                loadedData.camRotation[1],
                loadedData.camRotation[2]
            );

            Vector2 camSensorXY = new Vector2(
                loadedData.camSensorX,
                loadedData.camSensorY
            );


            // fill all input fields with loaded values
            GameObject.Find("SSSN_name_input").GetComponent<TMP_InputField>().text = sssnName;
            GameObject.Find("CamXInput").GetComponent<TMP_InputField>().text = loadedData.camPosition[0].ToString();
            GameObject.Find("CamYInput").GetComponent<TMP_InputField>().text = loadedData.camPosition[1].ToString();
            GameObject.Find("CamZInput").GetComponent<TMP_InputField>().text = loadedData.camPosition[2].ToString();
            GameObject.Find("CamRotXIn").GetComponent<TMP_InputField>().text = loadedData.camRotation[0].ToString();
            GameObject.Find("CamRotXIn").GetComponent<TMP_InputField>().text = loadedData.camRotation[0].ToString();
            GameObject.Find("CamRotYIn").GetComponent<TMP_InputField>().text = loadedData.camRotation[1].ToString();
            GameObject.Find("CamSensor").GetComponent<TMP_InputField>().text = "(" + loadedData.camSensorX + "," + loadedData.camSensorY + ")";
            GameObject.Find("CamShiftInput").GetComponent<TMP_InputField>().text = loadedData.camShiftY.ToString();
            GameObject.Find("CamFOVInput").GetComponent<TMP_InputField>().text = loadedData.camFOV.ToString();

            GameObject.Find("Slider_left_X").GetComponent<Slider>().value = loadedData.gearsLeftPosition[0];
            GameObject.Find("Slider_left_Y").GetComponent<Slider>().value = loadedData.gearsLeftPosition[1];
            GameObject.Find("Slider_right_X").GetComponent<Slider>().value = loadedData.gearsRightPosition[0];
            GameObject.Find("Slider_right_Y").GetComponent<Slider>().value = loadedData.gearsRightPosition[1];
            GameObject.Find("LeftXInput").GetComponent<TMP_InputField>().text = loadedData.gearsLeftPosition[0].ToString();
            GameObject.Find("LeftYInput").GetComponent<TMP_InputField>().text = loadedData.gearsLeftPosition[1].ToString();
            GameObject.Find("RightXInput").GetComponent<TMP_InputField>().text = loadedData.gearsRightPosition[0].ToString();
            GameObject.Find("RightYInput").GetComponent<TMP_InputField>().text = loadedData.gearsRightPosition[1].ToString();
            GameObject.Find("LeftRotInput").GetComponent<TMP_InputField>().text = loadedData.mastRotLeft.ToString();
            GameObject.Find("RightRotInput").GetComponent<TMP_InputField>().text = loadedData.mastRotRight.ToString();

            SceneTransition.crossMasterRotationLeft = mastRotLeft; // update the static master rotation value in SceneTransition for the left gear set, so it can be accessed by other SCENES
            SceneTransition.crossMasterRotationRight = mastRotRight; // update the static master rotation value in SceneTransition for the right gear set, so it can be accessed by other SCENES

            GameObject.Find("GearSetLeft").transform.position = leftPos;
            GameObject.Find("GearSetLeft").transform.rotation = Quaternion.Euler(leftRot);
            GameObject.Find("PlanetaryPlanetLeft").transform.position = planetaryRingLeftPos;
            GameObject.Find("PlanetaryPlanetLeft").transform.rotation = Quaternion.Euler(planetaryRingLeftRot);

            GameObject.Find("TriangleLeft_Cube").transform.position = trianglePosLeft;
            GameObject.Find("TriangleLeft_Cube").transform.rotation = Quaternion.Euler(triangleRotLeft);
            GameObject.Find("TriangleRight_Cube").transform.position = trianglePosRight;
            GameObject.Find("TriangleRight_Cube").transform.rotation = Quaternion.Euler(triangleRotRight);

            GameObject.Find("GearSetRight").transform.position = rightPos;
            GameObject.Find("GearSetRight").transform.rotation = Quaternion.Euler(rightRot);
            GameObject.Find("Main Camera").transform.position = camPos;
            // camXIn.text = camPos.x.ToString("F4");
            // camYIn.text = camPos.y.ToString("F4");
            // camZIn.text = camPos.z.ToString("F4");
            GameObject.Find("Main Camera").transform.rotation = Quaternion.Euler(camRot);
            // camRotXIn.text = Quaternion.Euler(camRot).x.ToString("F4");
            // camRotYIn.text = Quaternion.Euler(camRot).y.ToString("F4");

            Camera cam = GameObject.Find("Main Camera").GetComponent<Camera>().GetComponent<Camera>();
            // cam.sensorSize = new Vector2(cam.sensorSize.x, PlayerPrefs.GetFloat("camSensorY"));
            cam.sensorSize = camSensorXY;

            cam.lensShift = new Vector2(cam.lensShift.x, camShiftY);

            cam.fieldOfView = camFOV;
        }
        else
        {
            Debug.LogWarning("Save file not found at: " + Application.persistentDataPath + "/Save/UserData.json");
        }
    }


}
