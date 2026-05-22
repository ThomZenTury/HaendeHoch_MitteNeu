using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Video;
using TMPro;
using System;

public class UIKeyboard : MonoBehaviour
{

    public SceneTransition transition; // assign in Inspector
    public string targetScene = "NextScene";

    public GameObject menu; // Assign in inspector

    [SerializeField]
    private GameObject BGVideoPanel; // Assign in inspector

    [SerializeField]
    private Camera camera; // Assign in inspector
    [SerializeField] private Toggle fine;
    [SerializeField] private Toggle camLock;

    [SerializeField] private TMP_InputField camXIn;        // assign in Inspector
    [SerializeField] private TMP_InputField camYIn;        // assign in Inspector
    [SerializeField] private TMP_InputField camZIn;        // assign in Inspector
    [SerializeField] private TMP_InputField camRotXIn;        // assign in Inspector
    [SerializeField] private TMP_InputField camRotYIn;        // assign in Inspector
    [SerializeField] private TMP_InputField camSensorIn;        // assign in Inspector
    [SerializeField] private TMP_InputField LeftSpeedIn;        // assign in Inspector
    [SerializeField] private TMP_InputField RightSpeedIn;        // assign in Inspector
    [SerializeField] private TMP_InputField LeftRotYIn;        // assign in Inspector
    [SerializeField] private TMP_InputField RightRotYIn;        // assign in Inspector



    private bool maintenanceMode;

    private float step = 0.001f;

    public bool toggleLeftGears;
    public bool toggleRightGears;

    private float leftSpeed;
    private float rightSpeed;

    private GameObject[] stageItems;
    private GameObject twistedPlane;

    private String gearsLeftPosition;
    private String gearsRightPosition;
    private String gearsLeftRotation;
    private String gearsRightRotation;
    private String camPosition;
    private String camRotation;

    public string fileName0;
    public string fileName1;
    public string fileName2;
    public string fileName3;
    public string fileName4;
    private VideoPlayer BGvideoPlayer;

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

        if (LeftSpeedIn != null) LeftSpeedIn.onEndEdit.AddListener(OnLeftSpeedInputChanged);
        if (RightSpeedIn != null) RightSpeedIn.onEndEdit.AddListener(OnRightSpeedInputChanged);

        if (LeftRotYIn != null) LeftRotYIn.onEndEdit.AddListener(OnLeftRotYInputChanged);
        if (RightRotYIn != null) RightRotYIn.onEndEdit.AddListener(OnRightRotYInputChanged);



    }

    void OnXInputChanged(string text)
    {
        if (float.TryParse(text, out float x))
        {
            Vector3 pos = camera.transform.position;
            pos.x = x;
            camera.transform.position = pos;
        }
    }

    void OnYInputChanged(string text)
    {
        if (float.TryParse(text, out float y))
        {
            Vector3 pos = camera.transform.position;
            pos.y = y;
            camera.transform.position = pos;
        }
    }

    void OnZInputChanged(string text)
    {
        if (float.TryParse(text, out float z))
        {
            Vector3 pos = camera.transform.position;
            pos.z = z;
            camera.transform.position = pos;
        }
    }

    void OnRotXInputChanged(string text)
    {
        if (float.TryParse(text, out float rotX))
        {
            Vector3 rot = camera.transform.rotation.eulerAngles;
            rot.x = rotX;
            camera.transform.rotation = Quaternion.Euler(rot);
        }
    }

    void OnRotYInputChanged(string text)
    {
        if (float.TryParse(text, out float rotY))
        {
            Vector3 rot = camera.transform.rotation.eulerAngles;
            rot.y = rotY;
            camera.transform.rotation = Quaternion.Euler(rot);
        }
    }

    void OnSensorInputChanged(string text)
    {
        if (float.TryParse(text, out float sensorSizeY))
        {
            Camera cam = camera.GetComponent<Camera>();
            cam.sensorSize = new Vector2(cam.sensorSize.x, sensorSizeY);
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

    void OnLeftRotYInputChanged(string text)
    {
        if (float.TryParse(text, out float rotY))
        {
            Vector3 rot = GameObject.Find("GearSetLeft").transform.rotation.eulerAngles;
            rot.y = rotY;
            GameObject.Find("GearSetLeft").transform.rotation = Quaternion.Euler(rot);
        }
    }

    void OnRightRotYInputChanged(string text)
    {
        if (float.TryParse(text, out float rotY))
        {
            Vector3 rot = GameObject.Find("GearSetRight").transform.rotation.eulerAngles;
            rot.y = rotY;
            GameObject.Find("GearSetRight").transform.rotation = Quaternion.Euler(rot);
        }
    }

    void savePlayerPrefs()
    {
        float gearsLeftPositionX = GameObject.Find("GearSetLeft").transform.position.x;
        float gearsLeftPositionY = GameObject.Find("GearSetLeft").transform.position.y;
        float gearsLeftPositionZ = GameObject.Find("GearSetLeft").transform.position.z;

        float gearsLeftRotationX = GameObject.Find("GearSetLeft").transform.rotation.eulerAngles.x;
        float gearsLeftRotationY = GameObject.Find("GearSetLeft").transform.rotation.eulerAngles.y;
        float gearsLeftRotationZ = GameObject.Find("GearSetLeft").transform.rotation.eulerAngles.z;
        // gearsLeftRotation = GameObject.Find("GearSetLeft").transform.rotation.ToString();

        float gearsRightPositionX = GameObject.Find("GearSetRight").transform.position.x;
        float gearsRightPositionY = GameObject.Find("GearSetRight").transform.position.y;
        float gearsRightPositionZ = GameObject.Find("GearSetRight").transform.position.z;

        float gearsRightRotationX = GameObject.Find("GearSetRight").transform.rotation.eulerAngles.x;
        float gearsRightRotationY = GameObject.Find("GearSetRight").transform.rotation.eulerAngles.y;
        float gearsRightRotationZ = GameObject.Find("GearSetRight").transform.rotation.eulerAngles.z;
        // gearsRightRotation = GameObject.Find("GearSetRight").transform.rotation.ToString();

        float camPositionX = GameObject.Find("Main Camera").transform.position.x;
        float camPositionY = GameObject.Find("Main Camera").transform.position.y;
        float camPositionZ = GameObject.Find("Main Camera").transform.position.z;

        float camRotationX = GameObject.Find("Main Camera").transform.rotation.eulerAngles.x;
        float camRotationY = GameObject.Find("Main Camera").transform.rotation.eulerAngles.y;
        float camRotationZ = GameObject.Find("Main Camera").transform.rotation.eulerAngles.z;

        Camera cam = camera.GetComponent<Camera>();
        float camSensorY = cam.sensorSize.y;
        // camRotation = GameObject.Find("Main Camera").transform.rotation.ToString();

        PlayerPrefs.SetFloat("gearsLeftPositionX", gearsLeftPositionX);
        PlayerPrefs.SetFloat("gearsLeftPositionY", gearsLeftPositionY);
        PlayerPrefs.SetFloat("gearsLeftPositionZ", gearsLeftPositionZ);

        PlayerPrefs.SetFloat("gearsLeftRotationX", gearsLeftRotationX);
        PlayerPrefs.SetFloat("gearsLeftRotationY", gearsLeftRotationY);
        PlayerPrefs.SetFloat("gearsLeftRotationZ", gearsLeftRotationZ);


        PlayerPrefs.SetFloat("gearsRightPositionX", gearsRightPositionX);
        PlayerPrefs.SetFloat("gearsRightPositionY", gearsRightPositionY);
        PlayerPrefs.SetFloat("gearsRightPositionZ", gearsRightPositionZ);

        PlayerPrefs.SetFloat("gearsRightRotationX", gearsRightRotationX);
        PlayerPrefs.SetFloat("gearsRightRotationY", gearsRightRotationY);
        PlayerPrefs.SetFloat("gearsRightRotationZ", gearsRightRotationZ);


        PlayerPrefs.SetFloat("camPositionX", camPositionX);
        PlayerPrefs.SetFloat("camPositionY", camPositionY);
        PlayerPrefs.SetFloat("camPositionZ", camPositionZ);

        PlayerPrefs.SetFloat("camRotationX", camRotationX);
        PlayerPrefs.SetFloat("camRotationY", camRotationY);
        PlayerPrefs.SetFloat("camRotationZ", camRotationZ);

        PlayerPrefs.SetFloat("camSensorY", camSensorY);

        PlayerPrefs.Save();

        // Debug.Log("GLPos= " + gearsLeftPosition);
        // Debug.Log("GLRot= " + gearsLeftRotation);
        // Debug.Log("Values should be saved now.");
    }

    void loadPlayerPrefs()
    {
        Vector3 leftPos = new Vector3(
                PlayerPrefs.GetFloat("gearsLeftPositionX"),
                PlayerPrefs.GetFloat("gearsLeftPositionY"),
                PlayerPrefs.GetFloat("gearsLeftPositionZ")
            );
        Vector3 leftRot = new Vector3(
            PlayerPrefs.GetFloat("gearsLeftRotationX"),
            PlayerPrefs.GetFloat("gearsLeftRotationY"),
            PlayerPrefs.GetFloat("gearsLeftRotationZ")
        );
        Vector3 rightPos = new Vector3(
            PlayerPrefs.GetFloat("gearsRightPositionX"),
            PlayerPrefs.GetFloat("gearsRightPositionY"),
            PlayerPrefs.GetFloat("gearsRightPositionZ")
        );
        Vector3 rightRot = new Vector3(
            PlayerPrefs.GetFloat("gearsRightRotationX"),
            PlayerPrefs.GetFloat("gearsRightRotationY"),
            PlayerPrefs.GetFloat("gearsRightRotationZ")
        );
        Vector3 camPos = new Vector3(
            PlayerPrefs.GetFloat("camPositionX"),
            PlayerPrefs.GetFloat("camPositionY"),
            PlayerPrefs.GetFloat("camPositionZ")
        );
        Vector3 camRot = new Vector3(
            PlayerPrefs.GetFloat("camRotationX"),
            PlayerPrefs.GetFloat("camRotationY"),
            PlayerPrefs.GetFloat("camRotationZ")
        );

        GameObject.Find("GearSetLeft").transform.position = leftPos;
        GameObject.Find("GearSetLeft").transform.rotation = Quaternion.Euler(leftRot);
        GameObject.Find("GearSetRight").transform.position = rightPos;
        GameObject.Find("GearSetRight").transform.rotation = Quaternion.Euler(rightRot);
        GameObject.Find("Main Camera").transform.position = camPos;
        GameObject.Find("Main Camera").transform.rotation = Quaternion.Euler(camRot);

        Camera cam = camera.GetComponent<Camera>();
        cam.sensorSize = new Vector2(cam.sensorSize.x, PlayerPrefs.GetFloat("camSensorY"));
    }


    void Update()
    {

        // scene transition trigger
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            if (transition != null)
            {
                transition.onSceneLoaded = () =>
                {
                    // code to run after new scene is fully loaded
                    //Debug.Log("Scene ready — now run command");
                    loadPlayerPrefs(); // load saved values after scene transition

                };
                transition.FadeToScene(targetScene);
            }

            else
                Debug.LogWarning("SceneTransition reference not set.");
        }



        // toggle visibility of unneeded stage items by tag
        if (Keyboard.current.f5Key.wasPressedThisFrame)
        {
            // De-/Activate all found GameObjects
            foreach (var item in stageItems)
            {
                if (item != null) item.SetActive(!item.activeSelf);
            }

        }

        // toggle visibility of twisted Plane
        if (Keyboard.current.f6Key.wasPressedThisFrame)
        {
            if (twistedPlane != null) twistedPlane.SetActive(!twistedPlane.activeSelf);
        }


        ////////////////// UI stuff


        // toggle menu visibility with Escape key
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            maintenanceMode = !maintenanceMode;
            menu.SetActive(maintenanceMode);
        }

        //////////////////// gears control
        /// // reset to initial state of left gear sets
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            GameObject obj = GameObject.Find("GearSetLeft");
            float prevMtoDeg = obj.GetComponent<GearRotations>().moveToDegree;
            float prevDur = obj.GetComponent<GearRotations>().duration;
            obj.GetComponent<GearRotations>().moveToDegree = 0f;
            obj.GetComponent<GearRotations>().duration = 0.1f;
            obj.GetComponent<GearRotations>().rotateLeftGearSet(obj.GetComponent<GearRotations>().masterRotation, obj.GetComponent<GearRotations>().radians, obj.GetComponent<GearRotations>().midPoint, new Quaternion(0, 0, 0, 0)); ;
            obj.GetComponent<GearRotations>().moveToDegree = prevMtoDeg;
            obj.GetComponent<GearRotations>().duration = prevDur;
        }
        // reset to initial state of right gear sets
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            GameObject obj = GameObject.Find("GearSetRight");
            // obj.transform.Rotate(Vector3.up, 180f);
            float prevMtoDeg = obj.GetComponent<GearRotations>().moveToDegree;
            float prevDur = obj.GetComponent<GearRotations>().duration;
            obj.GetComponent<GearRotations>().moveToDegree = 0f;
            obj.GetComponent<GearRotations>().duration = 0.1f;
            obj.GetComponent<GearRotations>().rotateRightGearSet(obj.GetComponent<GearRotations>().masterRotation, obj.GetComponent<GearRotations>().radians, obj.GetComponent<GearRotations>().midPoint, Quaternion.Euler(0, 180, 0)); ;
            obj.GetComponent<GearRotations>().moveToDegree = prevMtoDeg;
            obj.GetComponent<GearRotations>().duration = prevDur;
        }

        // toggle autoplay gearsLeft 
        if (Keyboard.current.oKey.wasPressedThisFrame)
        {
            toggleLeftGears = !toggleLeftGears;
            GameObject.Find("GearSetLeft").GetComponent<GearRotations>().autoRotate = toggleLeftGears;
        }

        // toggle autoplay gearsRight
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            toggleRightGears = !toggleRightGears;
            GameObject.Find("GearSetRight").GetComponent<GearRotations>().autoRotate = toggleRightGears;
        }
        // toggle autoplay both gear sets
        if (Keyboard.current.leftBracketKey.wasPressedThisFrame)
        {
            toggleLeftGears = !toggleLeftGears;
            toggleRightGears = !toggleRightGears;
            GameObject.Find("GearSetLeft").GetComponent<GearRotations>().autoRotate = toggleLeftGears;
            GameObject.Find("GearSetRight").GetComponent<GearRotations>().autoRotate = toggleRightGears;


        }

        // reset left gear set to start position and rotation RELOADS THE WHOLE SCENE
        if (Keyboard.current.homeKey.wasPressedThisFrame)
        {
            GameObject.Find("GearSetLeft").GetComponent<GearRotations>().resetGearSet(); // reloads the whole scene
        }
        // if (Keyboard.current.f6Key.wasPressedThisFrame)
        // {
        //     GameObject.Find("GearSetRight").GetComponent<GearRotations>().resetGearSet();
        // }


        /////////////////////////// save and load values to PlayerPrefs

        if (Keyboard.current.f8Key.wasPressedThisFrame)
        {
            savePlayerPrefs();
        }

        if (Keyboard.current.f10Key.wasPressedThisFrame)
        {

            // Load values from PlayerPrefs
            loadPlayerPrefs();

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

            // turn fine mode on/off with F key
            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                fine.isOn = !fine.isOn; // Toggle fine mode on F key press
            }

            //////////////////// camera controls
            Vector3 p = camera.transform.position;
            Vector3 r = camera.transform.rotation.eulerAngles;

            // toggle camera lock on/off with Z key
            if (Keyboard.current.yKey.wasPressedThisFrame) // it's the zKey on german keyboard !!!
            {
                camLock.isOn = !camLock.isOn; // Toggle camera lock on Z key press
            }

            if (!camLock.isOn)
            {
                if (fine.isOn) step = 0.001f; else step = 0.01f;

                if (Keyboard.current.zKey.isPressed) // it's the yKey on german keyboard !!!
                {
                    p.x += step; // Move camera left while cKey is held down
                    camera.transform.position = p;
                    camXIn.text = camera.transform.position.x.ToString("F4");
                    // Debug.Log("Moving camera left");
                }
                if (Keyboard.current.xKey.isPressed)
                {
                    p.x -= step; // Move camera right while vKey is held down
                    camera.transform.position = p;
                    camXIn.text = camera.transform.position.x.ToString("F4");
                }
                if (Keyboard.current.cKey.isPressed)
                {
                    p.z -= step; // Move camera forward while bKey is held down
                    camera.transform.position = p;
                    camZIn.text = camera.transform.position.z.ToString("F4");

                }
                if (Keyboard.current.vKey.isPressed)
                {
                    p.z += step; // Move camera backward while vKey is held down
                    camera.transform.position = p;
                    camZIn.text = camera.transform.position.z.ToString("F4");
                }

                if (Keyboard.current.bKey.isPressed)
                {
                    p.y -= step; // Move camera down while nKey is held down
                    camera.transform.position = p;
                    camYIn.text = camera.transform.position.y.ToString("F4");
                }
                if (Keyboard.current.nKey.isPressed)
                {
                    p.y += step; // Move camera up while mKey is held down
                    camera.transform.position = p;
                    camYIn.text = camera.transform.position.y.ToString("F4");

                }

                if (Keyboard.current.mKey.isPressed)
                {
                    r.y -= step * 10f; // Rotate camera left while mKey is held down
                    camera.transform.rotation = Quaternion.Euler(r);
                    camRotYIn.text = camera.transform.rotation.eulerAngles.y.ToString("F4");
                }
                if (Keyboard.current.commaKey.isPressed)
                {
                    r.y += step * 10f; // Rotate camera right while commaKey is held down
                    camera.transform.rotation = Quaternion.Euler(r);
                    camRotYIn.text = camera.transform.rotation.eulerAngles.y.ToString("F4");
                }
                if (Keyboard.current.periodKey.isPressed)
                {
                    r.x -= step * 10f; // Rotate camera right while periodKey is held down
                    camera.transform.rotation = Quaternion.Euler(r);
                    camRotXIn.text = camera.transform.rotation.eulerAngles.x.ToString("F4");
                }
                if (Keyboard.current.slashKey.isPressed)
                {
                    r.x += step * 10f; // Rotate camera right while slashKey is held down
                    camera.transform.rotation = Quaternion.Euler(r);
                    camRotXIn.text = camera.transform.rotation.eulerAngles.x.ToString("F4");
                }

                if (Keyboard.current.pageUpKey.isPressed)
                {
                    Camera cam = camera.GetComponent<Camera>();
                    cam.sensorSize += new Vector2(0f, step * 10f); // Increase sensor size while fKey is held down
                    camSensorIn.text = cam.sensorSize.ToString("F4");
                }
                if (Keyboard.current.pageDownKey.isPressed)
                {
                    Camera cam = camera.GetComponent<Camera>();
                    cam.sensorSize -= new Vector2(0f, step * 10f); // Decrease sensor size while pageDownKey is held down
                    camSensorIn.text = cam.sensorSize.ToString("F4");
                }


            }


        }
        else // if not in maintenance mode
        {
            // Video stuff

            // Videos HAVE TO BE IN THIS EXACT FOLDER IN THE RESOURCES FOLDER, otherwise it doesn't work, even if the path is correct and the file is there.
            // Assets/Resources/Videos/SmokeThickAllOver.mp4 for example, but you have to write "SmokeThickAllOver" in the fileName0 variable, without extension and folder path. No idea why, but it works that way.

            if (Keyboard.current.numpad0Key.wasPressedThisFrame)
            {
                BGvideoPlayer = BGVideoPanel.GetComponentInChildren<VideoPlayer>();

                VideoClip clip = Resources.Load<VideoClip>("Videos/" + fileName0); // no extension 
                Debug.Log("Loaded clip: " + clip);
                Debug.Log("Player: " + BGvideoPlayer);

                if (clip != null && BGvideoPlayer != null)
                {
                    BGvideoPlayer.source = UnityEngine.Video.VideoSource.VideoClip;
                    BGvideoPlayer.clip = clip;
                    BGvideoPlayer.Play();
                    SceneTransition.bgVideoTitle = fileName0; // set the static string to the name of the currently playing video clip, so it can be accessed by other SCENES   

                }
                else Debug.LogError("Failed to load or player is null: " + fileName0);
            }
            if (Keyboard.current.numpad1Key.wasPressedThisFrame)
            {
                BGvideoPlayer = BGVideoPanel.GetComponentInChildren<VideoPlayer>();
                VideoClip clip = Resources.Load<VideoClip>("Videos/" + fileName1); // no extension 

                if (clip != null && BGvideoPlayer != null)
                {
                    BGvideoPlayer.source = UnityEngine.Video.VideoSource.VideoClip;
                    BGvideoPlayer.clip = clip;
                    BGvideoPlayer.Play();
                    SceneTransition.bgVideoTitle = fileName1; // set the static string to the name of the currently playing video clip, so it can be accessed by other SCENES   

                }
                else Debug.LogError("Failed to load or player is null: " + fileName1);
            }

            if (Keyboard.current.numpad2Key.wasPressedThisFrame)
            {
                BGvideoPlayer = BGVideoPanel.GetComponentInChildren<VideoPlayer>();
                VideoClip clip = Resources.Load<VideoClip>("Videos/" + fileName2); // no extension 

                if (clip != null && BGvideoPlayer != null)
                {
                    BGvideoPlayer.source = UnityEngine.Video.VideoSource.VideoClip;
                    BGvideoPlayer.clip = clip;
                    BGvideoPlayer.Play();
                    SceneTransition.bgVideoTitle = fileName2; // set the static string to the name of the currently playing video clip, so it can be accessed by other SCENES   
                }
                else Debug.LogError("Failed to load or player is null: " + fileName2);
            }

            if (Keyboard.current.numpad3Key.wasPressedThisFrame)
            {
                BGvideoPlayer = BGVideoPanel.GetComponentInChildren<VideoPlayer>();
                VideoClip clip = Resources.Load<VideoClip>("Videos/" + fileName3); // no extension 

                if (clip != null && BGvideoPlayer != null)
                {
                    BGvideoPlayer.source = UnityEngine.Video.VideoSource.VideoClip;
                    BGvideoPlayer.clip = clip;
                    BGvideoPlayer.Play();
                    SceneTransition.bgVideoTitle = fileName3; // set the static string to the name of the currently playing video clip, so it can be accessed by other SCENES   
                }
                else Debug.LogError("Failed to load or player is null: " + fileName3);
            }
        }
    }

}
