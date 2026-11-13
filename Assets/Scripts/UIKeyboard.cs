using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Video;
using TMPro;
using System;
using System.Collections;
using Unity.VectorGraphics;

public class UIKeyboard : MonoBehaviour
{
    [Header("Scene Transition")]
    public SceneTransition transition; // assign in Inspector
    public string prevScene = "prevScene";
    public string nextScene = "nextScene";

    [Header("OBJs + Materials")]
    public GameObject menu; // Assign in inspector

    [SerializeField]
    private GameObject BGVideoPanel; // Assign in inspector

    [SerializeField]
    public Camera camera; // Assign in inspector

    [SerializeField] private Transform planetary;     // assign planetary for left set
    [SerializeField] private Transform triangleLeft;     // assign planetary for left set
    [SerializeField] private Transform triangleRight;     // assign planetary for left set

    private GameObject[] stageItems;
    private GameObject twistedPlane;
    private GameObject[] shortCutsHelpTxts;

    private VideoPlayer BGvideoPlayer;


    [Header("Calibration Menu")]
    [SerializeField] private bool activateKeyCodeFinder;
    [SerializeField] public Toggle showHelp;
    [SerializeField] public Toggle fine;
    [SerializeField] public Toggle camLock;
    [SerializeField] public Toggle leftLock;
    [SerializeField] public Toggle rightLock;

    [SerializeField] public TMP_InputField camXIn;        // assign in Inspector
    [SerializeField] public TMP_InputField camYIn;        // assign in Inspector
    [SerializeField] public TMP_InputField camZIn;        // assign in Inspector
    [SerializeField] public TMP_InputField camRotXIn;        // assign in Inspector
    [SerializeField] public TMP_InputField camRotYIn;        // assign in Inspector
    [SerializeField] public TMP_InputField camSensorIn;        // assign in Inspector
    [SerializeField] public TMP_InputField camShiftIn;        // assign in Inspector
    [SerializeField] public TMP_InputField camFOVIn;        // assign in Inspector
    [SerializeField] private TMP_InputField LeftSpeedIn;        // assign in Inspector
    [SerializeField] private TMP_InputField RightSpeedIn;        // assign in Inspector
    [SerializeField] private TMP_InputField LeftRotYIn;        // assign in Inspector
    [SerializeField] private TMP_InputField RightRotYIn;        // assign in Inspector
    [SerializeField] private TMP_InputField LeftYIn;        // assign in Inspector
    [SerializeField] private TMP_InputField RightYIn;        // assign in Inspector
    [SerializeField] private TMP_InputField LeftXIn;        // assign in Inspector
    [SerializeField] private TMP_InputField RightXIn;        // assign in Inspector
    [SerializeField] private TMP_Dropdown savedFilesDropdown;        // if you look for the assigned actions
                                                                     // you have to look in Unity Inspector under
                                                                     // onValueChanged  configuration
    [SerializeField] private TMP_InputField storagePathText;        // assign in Inspector
    [SerializeField] public TMP_Text savedInfoTxt;        // assign in Inspector

    public bool maintenanceMode;
    // data to save
    private SaveData saveData;

    public bool toggleDropdown;

    public float step = 0.001f;

    public bool toggleLeftGears;
    public bool toggleRightGears;

    private float leftSpeed;
    private float rightSpeed;

    private String gearsLeftPosition;
    private String gearsRightPosition;
    private String gearsLeftRotation;
    private String gearsRightRotation;
    private String camPosition;
    private String camRotation;

    [Header("Video Files in Resources/Videos/")]
    public string fileName0;
    public string fileName1;
    public string fileName2;
    public string fileName3;
    public string fileName4;
    public string fileName5;
    public string fileName6;
    public string fileName7;
    public string fileName8;
    public string fileName9;


    void Start()
    {


        // deactivate all unneeded stage items by tags at start

        stageItems = GameObject.FindGameObjectsWithTag("Respawn");
        foreach (var item in stageItems)
        {
            if (item != null) item.SetActive(!item.activeSelf);
        }
        twistedPlane = GameObject.FindGameObjectWithTag("Finish");
        if (twistedPlane != null) twistedPlane.SetActive(!twistedPlane.activeSelf);

        // get initial values of gear sets and camera for UI input fields
        toggleLeftGears = GameObject.Find("GearSetLeft").GetComponent<GearRotations>().autoRotate;
        toggleRightGears = GameObject.Find("GearSetRight").GetComponent<GearRotations>().autoRotate;
        leftSpeed = GameObject.Find("GearSetLeft").GetComponent<GearRotations>().rotationSpeed;
        rightSpeed = GameObject.Find("GearSetRight").GetComponent<GearRotations>().rotationSpeed;

        camXIn.text = camera.transform.position.x.ToString();
        camYIn.text = camera.transform.position.y.ToString();
        camZIn.text = camera.transform.position.z.ToString();
        camRotXIn.text = camera.transform.rotation.eulerAngles.x.ToString();
        camRotYIn.text = camera.transform.rotation.eulerAngles.y.ToString();
        camSensorIn.text = camera.GetComponent<Camera>().sensorSize.ToString();
        camShiftIn.text = camera.GetComponent<Camera>().lensShift.y.ToString(); // Y = -0.365
        camFOVIn.text = camera.GetComponent<Camera>().fieldOfView.ToString(); // Y = -0.365
        LeftSpeedIn.text = leftSpeed.ToString();
        RightSpeedIn.text = rightSpeed.ToString();
        LeftRotYIn.text = GameObject.Find("GearSetLeft").transform.rotation.eulerAngles.y.ToString();
        RightRotYIn.text = GameObject.Find("GearSetRight").transform.rotation.eulerAngles.y.ToString();

        if (camXIn != null) camXIn.onEndEdit.AddListener(OnXInputChanged);
        if (camYIn != null) camYIn.onEndEdit.AddListener(OnYInputChanged);
        if (camZIn != null) camZIn.onEndEdit.AddListener(OnZInputChanged);

        if (camRotXIn != null) camRotXIn.onEndEdit.AddListener(OnRotXInputChanged);
        if (camRotYIn != null) camRotYIn.onEndEdit.AddListener(OnRotYInputChanged);
        if (camSensorIn != null) camSensorIn.onEndEdit.AddListener(OnSensorInputChanged);
        if (camShiftIn != null) camShiftIn.onEndEdit.AddListener(OnShiftInputChanged);
        if (camFOVIn != null) camFOVIn.onEndEdit.AddListener(OnFOVInputChanged);

        if (LeftSpeedIn != null) LeftSpeedIn.onEndEdit.AddListener(OnLeftSpeedInputChanged);
        if (RightSpeedIn != null) RightSpeedIn.onEndEdit.AddListener(OnRightSpeedInputChanged);

        if (LeftRotYIn != null) LeftRotYIn.onEndEdit.AddListener(OnLeftRotYInputChanged);
        if (RightRotYIn != null) RightRotYIn.onEndEdit.AddListener(OnRightRotYInputChanged);



    }

    void OnXInputChanged(string text)
    {
        if (camLock.isOn) return; // do not allow changes if camera lock is on
        else
        {
            if (float.TryParse(text, out float x))
            {
                Vector3 pos = camera.transform.position;
                pos.x = x;
                camera.transform.position = pos;
            }
        }

    }

    void OnYInputChanged(string text)
    {
        if (camLock.isOn) return; // do not allow changes if camera lock is on
        else
        {
            if (float.TryParse(text, out float y))
            {
                Vector3 pos = camera.transform.position;
                pos.y = y;
                camera.transform.position = pos;
            }
        }
    }

    void OnZInputChanged(string text)
    {
        if (camLock.isOn) return; // do not allow changes if camera lock is on
        else
        {
            if (float.TryParse(text, out float z))
            {
                Vector3 pos = camera.transform.position;
                pos.z = z;
                camera.transform.position = pos;
            }
        }
    }

    void OnRotXInputChanged(string text)
    {
        if (camLock.isOn) return; // do not allow changes if camera lock is on
        else
        {
            if (float.TryParse(text, out float rotX))
            {
                Vector3 rot = camera.transform.rotation.eulerAngles;
                rot.x = rotX;
                camera.transform.rotation = Quaternion.Euler(rot);
            }
        }
    }

    void OnRotYInputChanged(string text)
    {
        if (camLock.isOn) return; // do not allow changes if camera lock is on
        else
        {
            if (float.TryParse(text, out float rotY))
            {
                Vector3 rot = camera.transform.rotation.eulerAngles;
                rot.y = rotY;
                camera.transform.rotation = Quaternion.Euler(rot);
            }
        }
    }

    void OnSensorInputChanged(string text)
    {
        if (camLock.isOn) return; // do not allow changes if camera lock is on
        else
        {
            if (float.TryParse(text, out float sensorSizeX))
            {
                float ratio = 0.5625f; // 9/16 = ratio, inverted 16:9
                float sensorSizeY = sensorSizeX * ratio;
                Camera cam = camera.GetComponent<Camera>();
                cam.sensorSize = new Vector2(sensorSizeX, sensorSizeY);
            }
        }
    }
    void OnShiftInputChanged(string text)
    {
        if (camLock.isOn) return; // do not allow changes if camera lock is on
        else
        {
            if (float.TryParse(text, out float shiftY))
            {
                Camera cam = camera.GetComponent<Camera>();
                cam.lensShift = new Vector2(cam.lensShift.x, shiftY);
            }
        }
    }
    void OnFOVInputChanged(string text)
    {
        if (camLock.isOn) return; // do not allow changes if camera lock is on
        else
        {
            if (float.TryParse(text, out float fov))
            {
                Camera cam = camera.GetComponent<Camera>();
                cam.fieldOfView = fov;
            }
        }
    }

    void OnLeftSpeedInputChanged(string text)
    {
        if (float.TryParse(text, out float speed))
        {
            leftSpeed = speed;
            GameObject.Find("GearSetLeft").GetComponent<GearRotations>().rotationSpeed = leftSpeed;
        }
    }

    void OnRightSpeedInputChanged(string text)
    {
        if (float.TryParse(text, out float speed))
        {
            rightSpeed = speed;
            GameObject.Find("GearSetRight").GetComponent<GearRotations>().rotationSpeed = rightSpeed;
        }
    }

    public void OnLeftRotYInputChanged(string text)
    {
        if (float.TryParse(text, out float rotY))
        {
            Vector3 rot = GameObject.Find("GearSetLeft").transform.rotation.eulerAngles;
            rot.y = rotY;
            GameObject.Find("GearSetLeft").transform.rotation = Quaternion.Euler(rot);
            SceneTransition.crossMasterRotationLeft = rotY; // update the static master rotation value in SceneTransition for the left gear set, so it can be accessed by other SCENES

            if (planetary != null)
            {
                Vector3 planetaryRot = planetary.rotation.eulerAngles;
                planetaryRot.y = rotY;
                planetary.rotation = Quaternion.Euler(planetaryRot);
            }
            if (triangleLeft != null)
            {
                Vector3 triangleLeftRot = triangleLeft.rotation.eulerAngles;
                triangleLeftRot.y = rotY;
                triangleLeft.rotation = Quaternion.Euler(triangleLeftRot);
            }

        }

    }

    public void OnRightRotYInputChanged(string text)
    {

        if (float.TryParse(text, out float rotY))
        {
            Vector3 rot = GameObject.Find("GearSetRight").transform.rotation.eulerAngles;
            rot.y = rotY;
            GameObject.Find("GearSetRight").transform.rotation = Quaternion.Euler(rot);
            SceneTransition.crossMasterRotationRight = rotY; // update the static master rotation value in SceneTransition for the right gear set, so it can be accessed by other SCENES
        }
        if (triangleRight != null)
        {
            Vector3 triangleRightRot = triangleRight.rotation.eulerAngles;
            triangleRightRot.y = rotY;
            triangleRight.rotation = Quaternion.Euler(triangleRightRot);
        }

    }

    public void SaveCurrentSetup()
    {
        // SaveSystem.SaveSetup();
        // string json = JsonUtility.ToJson(saveData);
        SaveSystem.SaveData();
        savedInfoTxt.gameObject.SetActive(true);
        StartCoroutine(DeactivateTextAfterDelay(1.3f, savedInfoTxt));

    }

    public void LoadSetup()
    {

        SaveSystem.LoadData();

        // SaveData data = SaveSystem.LoadSetup();
        // if (data != null)
        // {
        //     // Apply the loaded data to the scene
        //     SceneTransition.crossMasterRotationLeft = data.mastRotLeft;
        //     SceneTransition.crossMasterRotationRight = data.mastRotRight;

        //     GameObject.Find("GearSetLeft").transform.position = new Vector3(data.gearsLeftPosition[0], data.gearsLeftPosition[1], data.gearsLeftPosition[2]);
        //     GameObject.Find("GearSetLeft").transform.rotation = Quaternion.Euler(data.gearsLeftRotation[0], data.gearsLeftRotation[1], data.gearsLeftRotation[2]);

        //     GameObject.Find("GearSetRight").transform.position = new Vector3(data.gearsRightPosition[0], data.gearsRightPosition[1], data.gearsRightPosition[2]);
        //     GameObject.Find("GearSetRight").transform.rotation = Quaternion.Euler(data.gearsRightRotation[0], data.gearsRightRotation[1], data.gearsRightRotation[2]);

        //     GameObject.Find("Main Camera").transform.position = new Vector3(data.camPosition[0], data.camPosition[1], data.camPosition[2]);
        //     GameObject.Find("Main Camera").transform.rotation = Quaternion.Euler(data.camRotation[0], data.camRotation[1], data.camRotation[2]);

        //     Camera cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        //     cam.sensorSize = new Vector2(cam.sensorSize.x, data.camSensorY);
        // }
    }
    // void savePlayerPrefs()
    // {
    //     float mastRotLeft = SceneTransition.crossMasterRotationLeft;
    //     float mastRotRight = SceneTransition.crossMasterRotationRight;

    //     float gearsLeftPositionX = GameObject.Find("GearSetLeft").transform.position.x;
    //     float gearsLeftPositionY = GameObject.Find("GearSetLeft").transform.position.y;
    //     float gearsLeftPositionZ = GameObject.Find("GearSetLeft").transform.position.z;

    //     float gearsLeftRotationX = GameObject.Find("GearSetLeft").transform.rotation.eulerAngles.x;
    //     float gearsLeftRotationY = GameObject.Find("GearSetLeft").transform.rotation.eulerAngles.y;
    //     float gearsLeftRotationZ = GameObject.Find("GearSetLeft").transform.rotation.eulerAngles.z;
    //     // gearsLeftRotation = GameObject.Find("GearSetLeft").transform.rotation.ToString();

    //     float gearsRightPositionX = GameObject.Find("GearSetRight").transform.position.x;
    //     float gearsRightPositionY = GameObject.Find("GearSetRight").transform.position.y;
    //     float gearsRightPositionZ = GameObject.Find("GearSetRight").transform.position.z;

    //     float gearsRightRotationX = GameObject.Find("GearSetRight").transform.rotation.eulerAngles.x;
    //     float gearsRightRotationY = GameObject.Find("GearSetRight").transform.rotation.eulerAngles.y;
    //     float gearsRightRotationZ = GameObject.Find("GearSetRight").transform.rotation.eulerAngles.z;
    //     // gearsRightRotation = GameObject.Find("GearSetRight").transform.rotation.ToString();

    //     float camPositionX = GameObject.Find("Main Camera").transform.position.x;
    //     float camPositionY = GameObject.Find("Main Camera").transform.position.y;
    //     float camPositionZ = GameObject.Find("Main Camera").transform.position.z;

    //     float camRotationX = GameObject.Find("Main Camera").transform.rotation.eulerAngles.x;
    //     float camRotationY = GameObject.Find("Main Camera").transform.rotation.eulerAngles.y;
    //     float camRotationZ = GameObject.Find("Main Camera").transform.rotation.eulerAngles.z;

    //     Camera cam = camera.GetComponent<Camera>();
    //     float camSensorY = cam.sensorSize.y;
    //     // camRotation = GameObject.Find("Main Camera").transform.rotation.ToString();

    //     PlayerPrefs.SetFloat("mastRotLeft", mastRotLeft);
    //     PlayerPrefs.SetFloat("mastRotRight", mastRotRight);

    //     PlayerPrefs.SetFloat("gearsLeftPositionX", gearsLeftPositionX);
    //     PlayerPrefs.SetFloat("gearsLeftPositionY", gearsLeftPositionY);
    //     PlayerPrefs.SetFloat("gearsLeftPositionZ", gearsLeftPositionZ);

    //     PlayerPrefs.SetFloat("gearsLeftRotationX", gearsLeftRotationX);
    //     PlayerPrefs.SetFloat("gearsLeftRotationY", gearsLeftRotationY);
    //     PlayerPrefs.SetFloat("gearsLeftRotationZ", gearsLeftRotationZ);


    //     PlayerPrefs.SetFloat("gearsRightPositionX", gearsRightPositionX);
    //     PlayerPrefs.SetFloat("gearsRightPositionY", gearsRightPositionY);
    //     PlayerPrefs.SetFloat("gearsRightPositionZ", gearsRightPositionZ);

    //     PlayerPrefs.SetFloat("gearsRightRotationX", gearsRightRotationX);
    //     PlayerPrefs.SetFloat("gearsRightRotationY", gearsRightRotationY);
    //     PlayerPrefs.SetFloat("gearsRightRotationZ", gearsRightRotationZ);


    //     PlayerPrefs.SetFloat("camPositionX", camPositionX);
    //     PlayerPrefs.SetFloat("camPositionY", camPositionY);
    //     PlayerPrefs.SetFloat("camPositionZ", camPositionZ);

    //     PlayerPrefs.SetFloat("camRotationX", camRotationX);
    //     PlayerPrefs.SetFloat("camRotationY", camRotationY);
    //     PlayerPrefs.SetFloat("camRotationZ", camRotationZ);

    //     PlayerPrefs.SetFloat("camSensorY", camSensorY);

    //     PlayerPrefs.Save();

    //     // Debug.Log("GLPos= " + gearsLeftPosition);
    //     // Debug.Log("GLRot= " + gearsLeftRotation);
    //     // Debug.Log("Values should be saved now.");
    // }

    // void loadPlayerPrefs()
    // {
    //     float mastRotLeft = PlayerPrefs.GetFloat("mastRotLeft");
    //     float mastRotRight = PlayerPrefs.GetFloat("mastRotRight");

    //     Vector3 leftPos = new Vector3(
    //             PlayerPrefs.GetFloat("gearsLeftPositionX"),
    //             PlayerPrefs.GetFloat("gearsLeftPositionY"),
    //             PlayerPrefs.GetFloat("gearsLeftPositionZ")
    //         );
    //     Vector3 leftRot = new Vector3(
    //         PlayerPrefs.GetFloat("gearsLeftRotationX"),
    //         PlayerPrefs.GetFloat("gearsLeftRotationY"),
    //         PlayerPrefs.GetFloat("gearsLeftRotationZ")
    //     );
    //     Vector3 rightPos = new Vector3(
    //         PlayerPrefs.GetFloat("gearsRightPositionX"),
    //         PlayerPrefs.GetFloat("gearsRightPositionY"),
    //         PlayerPrefs.GetFloat("gearsRightPositionZ")
    //     );
    //     Vector3 rightRot = new Vector3(
    //         PlayerPrefs.GetFloat("gearsRightRotationX"),
    //         PlayerPrefs.GetFloat("gearsRightRotationY"),
    //         PlayerPrefs.GetFloat("gearsRightRotationZ")
    //     );
    //     Vector3 camPos = new Vector3(
    //         PlayerPrefs.GetFloat("camPositionX"),
    //         PlayerPrefs.GetFloat("camPositionY"),
    //         PlayerPrefs.GetFloat("camPositionZ")
    //     );
    //     Vector3 camRot = new Vector3(
    //         PlayerPrefs.GetFloat("camRotationX"),
    //         PlayerPrefs.GetFloat("camRotationY"),
    //         PlayerPrefs.GetFloat("camRotationZ")
    //     );

    //     SceneTransition.crossMasterRotationLeft = mastRotLeft; // update the static master rotation value in SceneTransition for the left gear set, so it can be accessed by other SCENES
    //     SceneTransition.crossMasterRotationRight = mastRotRight; // update the static master rotation value in SceneTransition for the right gear set, so it can be accessed by other SCENES

    //     GameObject.Find("GearSetLeft").transform.position = leftPos;
    //     GameObject.Find("GearSetLeft").transform.rotation = Quaternion.Euler(leftRot);
    //     GameObject.Find("GearSetRight").transform.position = rightPos;
    //     GameObject.Find("GearSetRight").transform.rotation = Quaternion.Euler(rightRot);
    //     GameObject.Find("Main Camera").transform.position = camPos;
    //     camXIn.text = camPos.x.ToString("F4");
    //     camYIn.text = camPos.y.ToString("F4");
    //     camZIn.text = camPos.z.ToString("F4");
    //     GameObject.Find("Main Camera").transform.rotation = Quaternion.Euler(camRot);
    //     camRotXIn.text = Quaternion.Euler(camRot).x.ToString("F4");
    //     camRotYIn.text = Quaternion.Euler(camRot).y.ToString("F4");

    //     Camera cam = camera.GetComponent<Camera>();
    //     cam.sensorSize = new Vector2(cam.sensorSize.x, PlayerPrefs.GetFloat("camSensorY"));
    // }

    private void loadNewVideoClip(String fileName)
    {
        BGvideoPlayer = BGVideoPanel.GetComponentInChildren<VideoPlayer>();
        VideoClip clip = Resources.Load<VideoClip>("Videos/" + fileName); // no extension 

        if (clip != null && BGvideoPlayer != null)
        {
            BGvideoPlayer.source = UnityEngine.Video.VideoSource.VideoClip;
            BGvideoPlayer.clip = clip;
            BGvideoPlayer.Play();
            SceneTransition.bgVideoTitle = fileName; // set the static string to the name of the currently playing video clip, so it can be accessed by other SCENES   
        }
        else Debug.LogError("Failed to load or player is null: " + fileName);
    }


    void Update()
    {
        if (activateKeyCodeFinder)
            // helper script to find out the keycodes of pressed keys, useful for debugging and finding out the correct keycode for special keys like ß or ü on german keyboards
            if (Keyboard.current.anyKey.wasPressedThisFrame)
            {
                foreach (KeyControl key in Keyboard.current.allKeys)
                {
                    if (key.wasPressedThisFrame)
                    {
                        Debug.Log("Key pressed: " + key.name);
                    }
                }
            }


        // scene transition trigger
        // if (Keyboard.current.digit1Key.wasPressedThisFrame)
        // {
        //     if (transition != null)
        //     {
        //         transition.onSceneLoaded = () =>
        //         {
        //             // code to run after new scene is fully loaded
        //             //Debug.Log("Scene ready — now run command");
        //             loadPlayerPrefs(); // load saved values after scene transition

        //         };
        //         transition.FadeToScene(prevScene);
        //     }

        //     else
        //         Debug.LogWarning("SceneTransition reference not set.");
        // }
        // if (Keyboard.current.digit2Key.wasPressedThisFrame)
        // {
        //     if (transition != null)
        //     {
        //         transition.onSceneLoaded = () =>
        //         {
        //             // code to run after new scene is fully loaded
        //             //Debug.Log("Scene ready — now run command");
        //             loadPlayerPrefs(); // load saved values after scene transition

        //         };
        //         transition.FadeToScene(nextScene);
        //     }

        //     else
        //         Debug.LogWarning("SceneTransition reference not set.");
        // }

        // activate TMP textfields with CTRL + key combinations
        if (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.leftAltKey.isPressed && Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            camXIn.ActivateInputField();
            // Debug.Log("Activated camXIn input field");
        }
        if (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.leftAltKey.isPressed && Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            camZIn.ActivateInputField();
        }
        if (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.leftAltKey.isPressed && Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            camYIn.ActivateInputField();
        }
        if (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.leftAltKey.isPressed && Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            camRotXIn.ActivateInputField();
        }
        if (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.leftAltKey.isPressed && Keyboard.current.digit5Key.wasPressedThisFrame)
        {
            camRotYIn.ActivateInputField();
        }
        if (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.leftAltKey.isPressed && Keyboard.current.digit6Key.wasPressedThisFrame)
        {
            camSensorIn.ActivateInputField();
        }
        if (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.leftAltKey.isPressed && Keyboard.current.digit7Key.wasPressedThisFrame)
        {
            LeftYIn.ActivateInputField();
        }
        if (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.leftAltKey.isPressed && Keyboard.current.digit8Key.wasPressedThisFrame)
        {
            RightYIn.ActivateInputField();
        }
        if (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.leftAltKey.isPressed && Keyboard.current.digit9Key.wasPressedThisFrame)
        {
            LeftRotYIn.ActivateInputField();
        }
        if (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.leftAltKey.isPressed && Keyboard.current.digit0Key.wasPressedThisFrame)
        {
            RightRotYIn.ActivateInputField();
        }
        if (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.leftAltKey.isPressed && Keyboard.current.minusKey.wasPressedThisFrame)
        {
            LeftXIn.ActivateInputField();
        }
        if (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.leftAltKey.isPressed && Keyboard.current.equalsKey.wasPressedThisFrame)
        {
            RightXIn.ActivateInputField();
        }




        // toggle visibility of unneeded stage items by tag
        if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.f5Key.wasPressedThisFrame)
        {
            // De-/Activate all found GameObjects
            foreach (var item in stageItems)
            {
                if (item != null) item.SetActive(!item.activeSelf);
            }

        }

        // toggle visibility of twisted Plane
        if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.f6Key.wasPressedThisFrame)
        {
            if (twistedPlane != null) twistedPlane.SetActive(!twistedPlane.activeSelf);
        }


        ////////////////// UI stuff


        // toggle menu visibility with Escape key
        if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            maintenanceMode = !maintenanceMode;
            menu.SetActive(maintenanceMode);
            GetComponent<extOSC.OSC_send>().showUI(maintenanceMode);
        }

        //////////////////// gears control
        /// // reset to initial state of left gear sets
        if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.rKey.wasPressedThisFrame)
        {
            GameObject obj = GameObject.Find("GearSetLeft");
            //  calc the reset rotations etc.
            float prevMtoDeg = obj.GetComponent<GearRotations>().moveToDegree;
            float prevDur = obj.GetComponent<GearRotations>().duration;
            obj.GetComponent<GearRotations>().moveToDegree = 0f;
            obj.GetComponent<GearRotations>().duration = 0.1f;
            obj.GetComponent<GearRotations>().rotateLeftGearSet(-SceneTransition.crossMasterRotationLeft, obj.GetComponent<GearRotations>().radians, obj.GetComponent<GearRotations>().midPoint, Quaternion.Euler(0, SceneTransition.crossMasterRotationLeft, 0)); ;
            obj.GetComponent<GearRotations>().moveToDegree = prevMtoDeg;
            obj.GetComponent<GearRotations>().duration = prevDur;
            // then rotate the right gearset to the masterRotation stored in crossmasterRotationLeft
            // Vector3 drivingGearRot = obj.GetComponent<GearRotations>().drivingGear.rotation.eulerAngles;
            // // drivingGearRot.y = SceneTransition.crossMasterRotationLeft;
            // obj.GetComponent<GearRotations>().drivingGear.rotation = Quaternion.Euler(drivingGearRot);

            // obj.GetComponent<GearRotations>().drivingGear.rotation = Quaternion.Euler(0, SceneTransition.crossMasterRotationLeft, 0);

            if (planetary != null)
            {
                Vector3 planetaryRot = planetary.rotation.eulerAngles;
                planetaryRot.y = SceneTransition.crossMasterRotationLeft;
                planetary.rotation = Quaternion.Euler(planetaryRot);
            }


        }
        // reset to initial state of right gear sets
        if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.tKey.wasPressedThisFrame)
        {
            GameObject obj = GameObject.Find("GearSetRight");
            // obj.transform.Rotate(Vector3.up, 180f);
            float prevMtoDeg = obj.GetComponent<GearRotations>().moveToDegree;
            float prevDur = obj.GetComponent<GearRotations>().duration;
            obj.GetComponent<GearRotations>().moveToDegree = 0f;
            obj.GetComponent<GearRotations>().duration = 0.1f;
            obj.GetComponent<GearRotations>().rotateRightGearSet(-SceneTransition.crossMasterRotationRight, obj.GetComponent<GearRotations>().radians, obj.GetComponent<GearRotations>().midPoint, Quaternion.Euler(0, 180, 0)); ;
            obj.GetComponent<GearRotations>().moveToDegree = prevMtoDeg;
            obj.GetComponent<GearRotations>().duration = prevDur;
        }

        // toggle autoplay gearsLeft 
        if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.oKey.wasPressedThisFrame)
        {
            toggleLeftGears = !toggleLeftGears;
            GameObject.Find("GearSetLeft").GetComponent<GearRotations>().autoRotate = toggleLeftGears;
        }

        // toggle autoplay gearsRight
        if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.pKey.wasPressedThisFrame)
        {
            toggleRightGears = !toggleRightGears;
            GameObject.Find("GearSetRight").GetComponent<GearRotations>().autoRotate = toggleRightGears;
        }
        // toggle autoplay both gear sets
        // if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.leftBracketKey.wasPressedThisFrame)
        // {
        //     toggleLeftGears = !toggleLeftGears;
        //     toggleRightGears = !toggleRightGears;
        //     GameObject.Find("GearSetLeft").GetComponent<GearRotations>().autoRotate = toggleLeftGears;
        //     GameObject.Find("GearSetRight").GetComponent<GearRotations>().autoRotate = toggleRightGears;
        // }

        // reset left gear set to start position and rotation RELOADS THE WHOLE SCENE
        if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.homeKey.wasPressedThisFrame)
        {
            GameObject.Find("GearSetLeft").GetComponent<GearRotations>().resetGearSet(); // reloads the whole scene
        }
        // if (Keyboard.current.f6Key.wasPressedThisFrame)
        // {
        //     GameObject.Find("GearSetRight").GetComponent<GearRotations>().resetGearSet();
        // }


        /////////////////////////// save and load values to PlayerPrefs

        if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.f8Key.wasPressedThisFrame)
        {
            // savePlayerPrefs();
            SaveCurrentSetup();
        }

        if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.f10Key.wasPressedThisFrame)
        {
            // Load values from PlayerPrefs
            // loadPlayerPrefs();
            // LoadSetup();
            toggleDropdown = !toggleDropdown;


        }


        // shader FX keyboard triggers 
        // if (Keyboard.current.gKey.wasPressedThisFrame)
        // {
        //     GameObject.Find("GearSetLeft").GetComponent<FlashFX>().ApplyFlashFX();
        // }
        // if (Keyboard.current.gKey.wasPressedThisFrame)
        // {
        //     // GameObject.Find("GearSetLeft").GetComponent<FlashFX>()._lerpAmount = 0f;
        //     // GameObject.Find("GearSetLeft").GetComponent<FlashFX>().DOTween.To(GetLerpValue, SetLerpValue, 1f, _duration).OnUpdate(OnLerpUpdate).OnComplete(OnLerpComplete);
        //     // GameObject.Find("GearSetLeft").GetComponent<FlashFX>().TweenInOut();
        // }


        if (maintenanceMode)
        {

            if (savedFilesDropdown != null && savedFilesDropdown.gameObject.activeSelf != toggleDropdown) savedFilesDropdown.gameObject.SetActive(toggleDropdown);
            // if (storagePathText != null && storagePathText.gameObject.activeSelf != toggleDropdown) storagePathText.gameObject.SetActive(toggleDropdown);
            // make path info invisible but not deactivated, otherwisee NullPointerEexception
            if (storagePathText != null && toggleDropdown == false)
            {
                // Set the background color to transparent
                Color backgroundColor = storagePathText.GetComponent<Image>().color;
                backgroundColor.a = 0; // Set alpha to 0
                storagePathText.GetComponent<Image>().color = backgroundColor;

                // Set the text color to transparent
                Color textColor = storagePathText.textComponent.color;
                textColor.a = 0; // Set alpha to 0
                storagePathText.textComponent.color = textColor;
            }
            else if (storagePathText != null && toggleDropdown == true)
            {
                Color backgroundColor = storagePathText.GetComponent<Image>().color;
                backgroundColor.a = 1;
                storagePathText.GetComponent<Image>().color = backgroundColor;

                Color textColor = storagePathText.textComponent.color;
                textColor.a = 1;
                storagePathText.textComponent.color = textColor;
            }

            // toggle left and right lock gears

            if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.qKey.wasPressedThisFrame)
            {
                leftLock.isOn = !leftLock.isOn;
            }
            if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.eKey.wasPressedThisFrame)
            {
                rightLock.isOn = !rightLock.isOn;
            }

            // turn on/off shortcut help texts with H key
            if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.hKey.wasPressedThisFrame)
            {
                showHelp.isOn = !showHelp.isOn; // Toggle help on H key press
            }
            if (shortCutsHelpTxts == null || shortCutsHelpTxts.Length == 0)
            {
                shortCutsHelpTxts = GameObject.FindGameObjectsWithTag("shortCutsHelp");
                // Debug.Log("Found " + shortCutsHelpTxts.Length + " shortcut help texts in the scene.");
            }
            foreach (GameObject item in shortCutsHelpTxts)
            {
                if (item != null && item.activeSelf != showHelp.isOn) item.SetActive(showHelp.isOn);
            }


            // turn fine mode on/off with F key
            if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.fKey.wasPressedThisFrame)
            {
                fine.isOn = !fine.isOn; // Toggle fine mode on F key press
            }

            //////////////////// camera controls
            Vector3 p = camera.transform.position;
            Vector3 r = camera.transform.rotation.eulerAngles;

            // toggle camera lock on/off with Z key
            if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.yKey.wasPressedThisFrame) // it's the zKey on german keyboard !!!
            {
                camLock.isOn = !camLock.isOn; // Toggle camera lock on Z key press
            }

            if (!camLock.isOn)
            {
                if (fine.isOn) step = 0.001f; else step = 0.01f;

                if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.zKey.isPressed) // it's the yKey on german keyboard !!!
                {
                    p.x += step; // Move camera left while yKey is held down
                    camera.transform.position = p;
                    camXIn.text = camera.transform.position.x.ToString("F4");
                    // Debug.Log("Moving camera left");
                }
                if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.xKey.isPressed)
                {
                    p.x -= step; // Move camera right while vKey is held down
                    camera.transform.position = p;
                    camXIn.text = camera.transform.position.x.ToString("F4");
                }
                if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.cKey.isPressed)
                {
                    p.z -= step; // Move camera forward while bKey is held down
                    camera.transform.position = p;
                    camZIn.text = camera.transform.position.z.ToString("F4");

                }
                if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.vKey.isPressed)
                {
                    p.z += step; // Move camera backward while vKey is held down
                    camera.transform.position = p;
                    camZIn.text = camera.transform.position.z.ToString("F4");
                }

                if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.bKey.isPressed)
                {
                    p.y -= step; // Move camera down while nKey is held down
                    camera.transform.position = p;
                    camYIn.text = camera.transform.position.y.ToString("F4");
                }
                if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.nKey.isPressed)
                {
                    p.y += step; // Move camera up while mKey is held down
                    camera.transform.position = p;
                    camYIn.text = camera.transform.position.y.ToString("F4");

                }

                if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.mKey.isPressed)
                {
                    r.y -= step * 10f; // Rotate camera left while mKey is held down
                    camera.transform.rotation = Quaternion.Euler(r);
                    camRotYIn.text = camera.transform.rotation.eulerAngles.y.ToString("F4");
                }
                if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.commaKey.isPressed)
                {
                    r.y += step * 10f; // Rotate camera right while commaKey is held down
                    camera.transform.rotation = Quaternion.Euler(r);
                    camRotYIn.text = camera.transform.rotation.eulerAngles.y.ToString("F4");
                }
                if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.periodKey.isPressed)
                {
                    r.x -= step * 10f; // Rotate camera up while periodKey is held down
                    camera.transform.rotation = Quaternion.Euler(r);
                    camRotXIn.text = camera.transform.rotation.eulerAngles.x.ToString("F4");
                }
                if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.slashKey.isPressed)
                {
                    r.x += step * 10f; // Rotate camera down while slashKey is held down
                    camera.transform.rotation = Quaternion.Euler(r);
                    camRotXIn.text = camera.transform.rotation.eulerAngles.x.ToString("F4");
                }

                if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.pageUpKey.isPressed)
                {
                    float ratio = 0.5625f; // 9/16 = ratio, inverted 16:9
                    Camera cam = camera.GetComponent<Camera>();
                    cam.sensorSize += new Vector2(step * 10f, step * 10f * ratio); // Increase sensor size while fKey is held down
                    camSensorIn.text = cam.sensorSize.ToString("F4");
                }
                if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.pageDownKey.isPressed)
                {
                    float ratio = 0.5625f; // 9/16 = ratio, inverted 16:9
                    Camera cam = camera.GetComponent<Camera>();
                    cam.sensorSize -= new Vector2(step * 10f, step * 10f * ratio); // Decrease sensor size while pageDownKey is held down
                    camSensorIn.text = cam.sensorSize.ToString("F4");
                }
                // if (Keyboard.current.insertKey.isPressed)
                // {
                //     Camera cam = camera.GetComponent<Camera>();
                //     cam.sensorSize += new Vector2(step * 10f, 0f); // Increase sensor size while fKey is held down
                //     camSensorIn.text = cam.sensorSize.ToString("F4");
                // }
                // if (Keyboard.current.deleteKey.isPressed)
                // {
                //     Camera cam = camera.GetComponent<Camera>();
                //     cam.sensorSize -= new Vector2(step * 10f, 0f); // Decrease sensor size while pageDownKey is held down
                //     camSensorIn.text = cam.sensorSize.ToString("F4");
                // }

                if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.semicolonKey.isPressed)
                {

                    Camera cam = camera.GetComponent<Camera>();
                    cam.lensShift -= new Vector2(0, step * 0.1f); // Decrease sensor size while pageDownKey is held down
                    camShiftIn.text = cam.lensShift.y.ToString("F4");
                }
                if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.quoteKey.isPressed)
                {

                    Camera cam = camera.GetComponent<Camera>();
                    cam.lensShift += new Vector2(0, step * 0.1f); // Decrease sensor size while pageDownKey is held down
                    camShiftIn.text = cam.lensShift.y.ToString("F4");
                }

                if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.leftBracketKey.isPressed)
                {
                    Camera cam = camera.GetComponent<Camera>();
                    cam.fieldOfView -= step * 0.1f; // Decrease sensor size while pageDownKey is held down
                    camFOVIn.text = cam.fieldOfView.ToString("F4");
                }
                if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.rightBracketKey.isPressed)
                {
                    Camera cam = camera.GetComponent<Camera>();
                    cam.fieldOfView += step * 0.1f; // Decrease sensor size while pageDownKey is held down
                    camFOVIn.text = cam.fieldOfView.ToString("F4");
                }


            }


        }
        else // if not in maintenance mode
        {
            // Video stuff

            // Videos HAVE TO BE IN THIS EXACT FOLDER IN THE RESOURCES FOLDER, otherwise it doesn't work, even if the path is correct and the file is there.
            // Assets/Resources/Videos/SmokeThickAllOver.mp4 for example, but you have to write "SmokeThickAllOver" in the fileName0 variable, without extension and folder path. No idea why, but it works that way.

            if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.numpad0Key.wasPressedThisFrame)
            {
                loadNewVideoClip(fileName0);
            }
            if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.numpad1Key.wasPressedThisFrame)
            {
                loadNewVideoClip(fileName1);
            }
            if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.numpad2Key.wasPressedThisFrame)
            {
                loadNewVideoClip(fileName2);
            }
            if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.numpad3Key.wasPressedThisFrame)
            {
                loadNewVideoClip(fileName3);
            }
            if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.numpad4Key.wasPressedThisFrame)
            {
                loadNewVideoClip(fileName4);
            }
            if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.numpad5Key.wasPressedThisFrame)
            {
                loadNewVideoClip(fileName5);
            }
            if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.numpad6Key.wasPressedThisFrame)
            {
                loadNewVideoClip(fileName6);
            }
            if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.numpad7Key.wasPressedThisFrame)
            {
                loadNewVideoClip(fileName7);
            }
            if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.numpad8Key.wasPressedThisFrame)
            {
                loadNewVideoClip(fileName8);
            }
            if (Keyboard.current.leftAltKey.isPressed && Keyboard.current.numpad9Key.wasPressedThisFrame)
            {
                loadNewVideoClip(fileName9);
            }
        }
    }

    public IEnumerator DeactivateTextAfterDelay(float delay, TMP_Text text)
    {
        yield return new WaitForSeconds(delay);
        text.gameObject.SetActive(false); // Optionally use text.enabled = false
    }

}
