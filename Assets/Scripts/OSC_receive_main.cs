using UnityEngine;
using UnityEngine.Video;
using System;
using UnityEngine.VFX;
using UnityEngine.Rendering.HighDefinition.Attributes;
using UnityEngine.UI;
using TMPro;



namespace extOSC
{
    public class OSC_receive_main : MonoBehaviour
    {
        #region Public Vars
        // public SceneTransition transition; // assign in Inspector

        [Header("OSC Settings")]
        public OSCReceiver Receiver;

        [Header("Setup Related")]
        public string nextSceneAddr = "/nextScene"; // 0 = previous scene, 1 = next scene
        public string floorMoveUpAddr = "/floor/moveUp";
        public string floorScaleAddr = "/floor/setScale";

        [SerializeField] private Slider sliderXleft;        // assign in Inspector
        [SerializeField] private Slider sliderYleft;        // assign in Inspector
        [SerializeField] private Slider sliderXright;        // assign in Inspector
        [SerializeField] private Slider sliderYright;        // assign in Inspector
        [SerializeField] private Slider opacitySlider;        // assign in Inspector
        [SerializeField] private TMP_InputField leftRot_txtField;    // assign in Inspector
        [SerializeField] private TMP_InputField rightRot_txtField;    // assign in Inspector
        // Camera setup OSC Addresses
        public string showUIAddr = "/UI/show";
        public string opacityUIAddr = "/UI/opacity";
        public string fineTuneAddr = "/UI/fineTune";
        public string camLockAddr = "/cam/lock";
        public string leftLockAddr = "/leftGears/lock";
        public string rightLockAddr = "/rightGears/lock";
        public string shortCutsAddr = "/show/shortcuts";
        public string camUpAddr = "/cam/up";
        public string camDownAddr = "/cam/down";
        public string camLeftAddr = "/cam/left";
        public string camBackAddr = "/cam/back";
        public string camForthAddr = "/cam/forth";
        public string camRightAddr = "/cam/right";
        public string camTiltUpAddr = "/cam/tiltUp";
        public string camTiltDownAddr = "/cam/tiltDown";
        public string camPanLeftAddr = "/cam/panLeft";
        public string camPanRightAddr = "/cam/panRight";
        public string leftGearsUpAddr = "/leftGears/up";
        public string leftGearsDownAddr = "/leftGears/down";
        public string leftGearsLeftAddr = "/leftGears/left";
        public string leftGearsRightAddr = "/leftGears/right";
        public string rightGearsUpAddr = "/rightGears/up";
        public string rightGearsDownAddr = "/rightGears/down";
        public string rightGearsLeftAddr = "/rightGears/left";
        public string rightGearsRightAddr = "/rightGears/right";
        public string leftGearsSliderYAddr = "/leftGears/sliderY";
        public string leftGearsSliderXAddr = "/leftGears/sliderX";
        public string rightGearsSliderYAddr = "/rightGears/sliderY";
        public string rightGearsSliderXAddr = "/rightGears/sliderX";
        public String leftGearsRotateAddr = "/leftGears/rotate";
        public String rightGearsRotateAddr = "/rightGears/rotate";
        public String sssnSaveAddr = "/sssn/save";



        [Header("Video OSC Addresses")]
        public string videoActiveAddr = "/BGVideo/setActive";
        public string videoSpeedAddr = "/BGVideo/setSpeed";
        public string videoAlphaAddr = "/BGVideo/setAlpha";
        public string videoIntensityAddr = "/BGVideo/setIntensity";
        public string videoClipAddr = "/BGVideo/setClip";
        public string videoTransTimeAddr = "/BGVideo/setTransTime";

        [Header("Materials OSC Addresses")]
        public string gearsMatChangeAddr = "/materials/setGearsMat";
        public string ringsMatChangeAddr = "/materials/setRingsMat";
        public string BGMatChangeAddr = "/materials/setBGMat";
        public string gearsFadeAddr = "/materials/gearsFade";
        public string ringsFadeAddr = "/materials/ringsFade";
        public string BGFadeAddr = "/materials/BGFade";
        public string handsFadeAddr = "/materials/handsFade";
        public string portalScaleAddr = "/shader/portalScale";
        public string portalMoveAddr = "/shader/portalMove";



        [Header("VFX OSC Addresses")]
        public string VFXactivateAddr = "/VFX/activate";
        public string VFXtrailsAddr = "/VFX/burstTrails";
        public string VFXhandPartsFadeAddr = "/VFX/handPartsFade";
        public string VFXfloatingPartsFadeAddr = "/VFX/floatingPartsFade";
        public string VFXcrawlersFadeAddr = "/VFX/crawlersFade";
        public string VFXcrawlersRateAddr = "/VFX/crawlersRate";
        public string VFXcrawlersDistAddr = "/VFX/crawlersDist";

        [Header("OBJs + Materials")]
        public GameObject BGVideoPanel;
        private VideoPlayer BGvideoPlayer;
        public Material videoMaterial;
        public GameObject hands;
        public GameObject floorPanel;


        [Header("VFX")]
        public VisualEffect visualEffect;
        public GameObject VFXHandParticles;
        public GameObject VFXsurfaceRipple;

        [Header("Target Materials")]
        public Material targetMaterial0;
        public Material targetMaterial1;
        public Material targetMaterial2;
        public Material targetMaterial3;
        public Material targetMaterial4;
        public Material targetMaterial5;
        public Material targetMaterial6;
        public Material targetMaterial7;
        public Material targetMaterial8;
        public Material targetMaterial9;
        public Material targetMaterial10;
        public Material targetMaterial11;


        private GameObject[] gearsArr;
        private GameObject[] ringsArr;
        private static Material[] gearsMats;
        private static Material[] ringsMats;
        private static Material[] BGMats;
        private Renderer rend;
        private Renderer rend1;
        private static Material[] availableMats;
        private GameObject[] VFXobjs;





        [Header("Video Files in Resources/Videos/")]
        [SerializeField] private string fileName0;
        [SerializeField] private string fileName1;
        [SerializeField] private string fileName2;
        [SerializeField] private string fileName3;
        [SerializeField] private string fileName4;
        [SerializeField] private string fileName5;
        [SerializeField] private string fileName6;
        [SerializeField] private string fileName7;
        [SerializeField] private string fileName8;
        [SerializeField] private string fileName9;




        #endregion

        #region Unity Methods

        protected virtual void Start()
        {
            gearsArr = GameObject.FindGameObjectsWithTag("allGears");
            ringsArr = GameObject.FindGameObjectsWithTag("planetaryRings");
            VFXobjs = GameObject.FindGameObjectsWithTag("VFX");

            // foreach (var t in ringsArr)
            // {
            //     Debug.Log(t.name);
            // }

            availableMats = Resources.LoadAll<Material>("Materials"); // load all materials in the Resources/Materials folder into the array to be able to switch between them with OSC

            Receiver.Bind(nextSceneAddr, ReceivedNextScene);
            Receiver.Bind(floorMoveUpAddr, ReceivedFloorMoveUp);
            Receiver.Bind(floorScaleAddr, ReceivedFloorScale);
            Receiver.Bind(showUIAddr, ReceivedshowUI);
            Receiver.Bind(fineTuneAddr, ReceivedFineTune);
            Receiver.Bind(opacityUIAddr, ReceivedUIopacity);
            // camera setup

            Receiver.Bind(camLockAddr, ReceivedCamLock);
            Receiver.Bind(leftLockAddr, ReceivedLeftLock);
            Receiver.Bind(rightLockAddr, ReceivedRightLock);
            Receiver.Bind(shortCutsAddr, ReceivedShortCuts);
            Receiver.Bind(camUpAddr, ReceivedCamUp);
            Receiver.Bind(camDownAddr, ReceivedCamDown);
            Receiver.Bind(camLeftAddr, ReceivedCamLeft);
            Receiver.Bind(camRightAddr, ReceivedCamRight);
            Receiver.Bind(camForthAddr, ReceivedCamForth);
            Receiver.Bind(camBackAddr, ReceivedCamBack);
            Receiver.Bind(camTiltUpAddr, ReceivedCamTiltUp);
            Receiver.Bind(camTiltDownAddr, ReceivedCamTiltDown);
            Receiver.Bind(camPanLeftAddr, ReceivedCamPanLeft);
            Receiver.Bind(camPanRightAddr, ReceivedCamPanRight);


            // gearsets movements
            Receiver.Bind(leftGearsUpAddr, ReceivedLeftGearsUp);
            Receiver.Bind(leftGearsDownAddr, ReceivedLeftGearsDown);
            Receiver.Bind(leftGearsLeftAddr, ReceivedLeftGearsLeft);
            Receiver.Bind(leftGearsRightAddr, ReceivedLeftGearsRight);
            Receiver.Bind(rightGearsUpAddr, ReceivedRightGearsUp);
            Receiver.Bind(rightGearsDownAddr, ReceivedRightGearsDown);
            Receiver.Bind(rightGearsLeftAddr, ReceivedRightGearsLeft);
            Receiver.Bind(rightGearsRightAddr, ReceivedRightGearsRight);
            Receiver.Bind(leftGearsSliderYAddr, ReceivedLeftGearsSliderY);
            Receiver.Bind(leftGearsSliderXAddr, ReceivedLeftGearsSliderX);
            Receiver.Bind(rightGearsSliderYAddr, ReceivedRightGearsSliderY);
            Receiver.Bind(rightGearsSliderXAddr, ReceivedRightGearsSliderX);
            Receiver.Bind(leftGearsRotateAddr, ReceivedLeftGearsRotate);
            Receiver.Bind(rightGearsRotateAddr, ReceivedRightGearsRotate);
            Receiver.Bind(sssnSaveAddr, ReceivedSssnSave);

            // Video
            Receiver.Bind(videoActiveAddr, ReceivedVideoActive);
            Receiver.Bind(videoSpeedAddr, ReceivedVideoSpeed);
            Receiver.Bind(videoAlphaAddr, ReceivedVideoAlpha);
            Receiver.Bind(videoIntensityAddr, ReceivedVideoIntensity);
            Receiver.Bind(videoClipAddr, ReceivedVideoClip);
            Receiver.Bind(videoTransTimeAddr, ReceivedVideoTransTime);
            // Materials
            Receiver.Bind(gearsMatChangeAddr, ReceivedGearsMatChange);
            Receiver.Bind(ringsMatChangeAddr, ReceivedRingsMatChange);
            Receiver.Bind(BGMatChangeAddr, ReceivedBGMatChange);
            Receiver.Bind(gearsFadeAddr, ReceivedGearsFade);
            Receiver.Bind(ringsFadeAddr, ReceivedRingsFade);
            Receiver.Bind(BGFadeAddr, ReceivedBGFade);
            Receiver.Bind(handsFadeAddr, ReceivedHandsFade);
            Receiver.Bind(portalScaleAddr, ReceivedPortalScale);
            Receiver.Bind(portalMoveAddr, ReceivedPortalMove);


            // VFX
            Receiver.Bind(VFXactivateAddr, ReceivedVFXactivate);
            Receiver.Bind(VFXtrailsAddr, ReceivedVFXtrails);
            Receiver.Bind(VFXhandPartsFadeAddr, ReceivedVFXhandPartsFade);
            Receiver.Bind(VFXfloatingPartsFadeAddr, ReceivedVFXfloatingPartsFade);
            Receiver.Bind(VFXcrawlersFadeAddr, ReceivedVFXcrawlersFade);
            Receiver.Bind(VFXcrawlersRateAddr, ReceivedVFXcrawlersRate);
            Receiver.Bind(VFXcrawlersDistAddr, ReceivedVFXcrawlersDist);
        }

        #endregion

        #region Private Methods

        private void ReceivedVideoActive(OSCMessage message)
        {
            // Debug.LogFormat("Received: {0}", message);
            if (message.ToInt(out var value))
            {
                if (value == 0)
                {
                    BGVideoPanel.SetActive(false);
                }
                else if (value == 1)
                {
                    BGVideoPanel.SetActive(true);
                }
            }

        }
        private void ReceivedVideoSpeed(OSCMessage message)
        {
            // Debug.LogFormat("Received: {0}", message);
            if (message.ToFloat(out var value))
            {
                BGvideoPlayer = BGVideoPanel.GetComponentInChildren<VideoPlayer>();

                if (BGvideoPlayer != null)
                {
                    BGvideoPlayer.playbackSpeed = value;
                }
                else Debug.LogError("Failed to set new playback speed: " + value);
            }

        }

        private void ReceivedVideoAlpha(OSCMessage message)
        {
            // Debug.LogFormat("Received: {0}", message);
            if (message.ToFloat(out var value))
            {
                // ATTENTION! The shader's variables have to be addressed with a leading underscore "_" even if they are not named like that in ShaderGraph
                videoMaterial.SetFloat("_AlphaRemapMax", value);
                // rend = BGVideoPanel.GetComponent<Renderer>();
                // BGMats = rend.materials;
                // BGMats[0].SetFloat("_AlphaRemapMin", value);
                SceneTransition.bgVideoAlpha = value; // the static float to pass to other scenes, load it from there in SceneTransition.cs and set it to the shader's variable there as well
                // videoMaterial.SetFloat("_AlphaRemapMin", value);
                Debug.LogFormat("Received: set video alpha to {0}", value);


            }

        }
        private void ReceivedVideoIntensity(OSCMessage message)
        {
            // Debug.LogFormat("Received: {0}", message);
            if (message.ToFloat(out var value))
            {
                // ATTENTION! The shader's variables have to be addressed with a leading underscore "_" even if they are not named like that in ShaderGraph
                Color baseEmissive = Color.white; // or your color
                // rend = BGVideoPanel.GetComponent<Renderer>();
                // BGMats = rend.materials;
                // BGMats[0].SetColor("_EmissiveColor", baseEmissive * value);
                videoMaterial.SetColor("_EmissiveColor", baseEmissive * value);

                // ensure emission is enabled
                videoMaterial.EnableKeyword("_EMISSION");

                Debug.LogFormat("Received: set video emission intensity to {0}", value);
            }

        }

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

        private void ReceivedVideoClip(OSCMessage message)
        {
            BGvideoPlayer = BGVideoPanel.GetComponentInChildren<VideoPlayer>();
            // Debug.LogFormat("Received: {0}", message);
            if (message.ToInt(out var value))
            {
                if (value == 0)
                {
                    loadNewVideoClip(fileName0);
                }
                else if (value == 1)
                {
                    loadNewVideoClip(fileName1);

                }
                else if (value == 2)
                {
                    loadNewVideoClip(fileName2);
                }

                else if (value == 3)
                {
                    loadNewVideoClip(fileName3);
                }
                else if (value == 4)
                {
                    loadNewVideoClip(fileName4);
                }
                else if (value == 5)
                {
                    loadNewVideoClip(fileName5);
                }
                else if (value == 6)
                {
                    loadNewVideoClip(fileName6);
                }
                else if (value == 7)
                {
                    loadNewVideoClip(fileName7);
                }
                else if (value == 8)
                {
                    loadNewVideoClip(fileName8);
                }
                else if (value == 9)
                {
                    loadNewVideoClip(fileName9);
                }
            }
        }

        private void ReceivedVideoTransTime(OSCMessage message)
        {
            // Debug.LogFormat("Received: {0}", message);
            if (message.ToFloat(out var value))
            {
                // transition.fadeDuration = value;
                GetComponent<UIKeyboard>().transition.fadeDuration = value;
                Debug.LogFormat("fadeDuration set to: " + value);

            }

        }

        private void ReceivedNextScene(OSCMessage message)
        {
            // Debug.LogFormat("Received: {0}", message);
            if (message.ToInt(out var value))
            {
                if (value == 0)
                {
                    GetComponent<UIKeyboard>().transition.FadeToScene(GetComponent<UIKeyboard>().prevScene);
                }
                else if (value == 1)
                {
                    GetComponent<UIKeyboard>().transition.FadeToScene(GetComponent<UIKeyboard>().nextScene);

                }
            }
        }

        private void ReceivedFloorMoveUp(OSCMessage message)
        {
            // Debug.LogFormat("Received: {0}", message);
            if (message.ToFloat(out var value))
            {
                floorPanel.transform.localPosition = new Vector3(0, value, 0);

            }

        }

        private void ReceivedFloorScale(OSCMessage message)
        {
            // Debug.LogFormat("Received: {0}", message);
            if (message.ToFloat(out var value))
            {
                floorPanel.transform.localScale = new Vector3(value / 2, 1, value);
            }
        }
        private void ReceivedshowUI(OSCMessage message)
        {
            if (message.ToInt(out var value))
            {
                if (value == 0)
                {
                    GetComponent<UIKeyboard>().maintenanceMode = false;
                    GetComponent<UIKeyboard>().menu.SetActive(false);
                    // Debug.LogFormat("Received: set maintenanceMode to {0}", value);
                }
                else if (value == 1)
                {
                    GetComponent<UIKeyboard>().maintenanceMode = true;
                    GetComponent<UIKeyboard>().menu.SetActive(true);
                    // Debug.LogFormat("eceioved", value);
                }
            }
        }

        private void ReceivedFineTune(OSCMessage message)
        {
            if (message.ToInt(out var value))
            {
                if (value == 0)
                {
                    GetComponent<UIKeyboard>().fine.isOn = false;
                    // Debug.LogFormat("Received: set fineTuneMode to {0}", value);
                }
                else if (value == 1)
                {
                    GetComponent<UIKeyboard>().fine.isOn = true;
                    // Debug.LogFormat("eceioved", value);
                }
            }
        }

        private void ReceivedUIopacity(OSCMessage message)
        {
            if (message.ToFloat(out var value))
            {
                // GetComponent<UIKeyboard>().canvasGroup.alpha = value;
                opacitySlider.value = value;
            }
        }

        private void ReceivedCamLock(OSCMessage message)
        {
            if (message.ToInt(out var value))
            {
                if (value == 0)
                {
                    GetComponent<UIKeyboard>().camLock.isOn = true;
                }
                else if (value == 1)
                {
                    GetComponent<UIKeyboard>().camLock.isOn = false;
                }
            }
        }
        private void ReceivedLeftLock(OSCMessage message)
        {
            if (message.ToInt(out var value))
            {
                if (value == 0)
                {
                    GetComponent<UIKeyboard>().leftLock.isOn = true;
                }
                else if (value == 1)
                {
                    GetComponent<UIKeyboard>().leftLock.isOn = false;
                }
            }
        }
        private void ReceivedRightLock(OSCMessage message)
        {
            if (message.ToInt(out var value))
            {
                if (value == 0)
                {
                    GetComponent<UIKeyboard>().rightLock.isOn = true;
                }
                else if (value == 1)
                {
                    GetComponent<UIKeyboard>().rightLock.isOn = false;
                }
            }
        }

        private void ReceivedShortCuts(OSCMessage message)
        {
            if (message.ToInt(out var value))
            {
                if (value == 0)
                {
                    GetComponent<UIKeyboard>().showHelp.isOn = false;
                }
                else if (value == 1)
                {
                    GetComponent<UIKeyboard>().showHelp.isOn = true;
                }
            }
        }


        //////////////////// camera controls

        private void MainCamMoveAndRotate(string target, string action, string direction)
        {
            var UIkeyboardScriptConnector = GetComponent<UIKeyboard>();
            if (UIkeyboardScriptConnector == null || !UIkeyboardScriptConnector.maintenanceMode)
            {
                return;
            }

            Transform cameraTransform = UIkeyboardScriptConnector.camera.transform;
            string targetName = target.Trim();
            string actionName = action.Trim();
            string directionName = direction.Trim();

            if (!string.Equals(targetName, "cam", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (string.Equals(actionName, "move", StringComparison.OrdinalIgnoreCase))
            {
                Vector3 position = cameraTransform.position;

                switch (directionName.ToLowerInvariant())
                {
                    case "up":
                        position.y += UIkeyboardScriptConnector.step;
                        UIkeyboardScriptConnector.camYIn.text = position.y.ToString("F4");
                        break;
                    case "down":
                        position.y -= UIkeyboardScriptConnector.step;
                        UIkeyboardScriptConnector.camYIn.text = position.y.ToString("F4");
                        break;
                    case "left":
                        position.x += UIkeyboardScriptConnector.step;
                        UIkeyboardScriptConnector.camXIn.text = position.x.ToString("F4");
                        break;
                    case "right":
                        position.x -= UIkeyboardScriptConnector.step;
                        UIkeyboardScriptConnector.camXIn.text = position.x.ToString("F4");
                        break;
                    case "forth":
                        position.z -= UIkeyboardScriptConnector.step;
                        UIkeyboardScriptConnector.camZIn.text = position.z.ToString("F4");
                        break;
                    case "back":
                        position.z += UIkeyboardScriptConnector.step;
                        UIkeyboardScriptConnector.camZIn.text = position.z.ToString("F4");
                        break;
                    default:
                        return;
                }

                cameraTransform.position = position;
            }
            else if (string.Equals(actionName, "rotate", StringComparison.OrdinalIgnoreCase))
            {
                Vector3 rotation = cameraTransform.rotation.eulerAngles;

                switch (directionName.ToLowerInvariant())
                {
                    case "panleft":
                        rotation.y -= UIkeyboardScriptConnector.step * 10f;
                        UIkeyboardScriptConnector.camRotYIn.text = rotation.y.ToString("F4");
                        break;
                    case "panright":
                        rotation.y += UIkeyboardScriptConnector.step * 10f;
                        UIkeyboardScriptConnector.camRotYIn.text = rotation.y.ToString("F4");
                        break;
                    case "tiltup":
                        rotation.x -= UIkeyboardScriptConnector.step * 10f;
                        UIkeyboardScriptConnector.camRotXIn.text = rotation.x.ToString("F4");
                        break;
                    case "tiltdown":
                        rotation.x += UIkeyboardScriptConnector.step * 10f;
                        UIkeyboardScriptConnector.camRotXIn.text = rotation.x.ToString("F4");
                        break;
                    default:
                        return;
                }

                cameraTransform.rotation = Quaternion.Euler(rotation);
            }
        }

        private void ReceivedCamUp(OSCMessage message)
        {
            if (message.ToInt(out _))
            {
                MainCamMoveAndRotate("cam", "move", "up");
            }
        }

        private void ReceivedCamDown(OSCMessage message)
        {
            if (message.ToInt(out _))
            {
                MainCamMoveAndRotate("cam", "move", "down");
            }
        }

        private void ReceivedCamLeft(OSCMessage message)
        {
            if (message.ToInt(out _))
            {
                MainCamMoveAndRotate("cam", "move", "left");
            }
        }

        private void ReceivedCamRight(OSCMessage message)
        {
            if (message.ToInt(out _))
            {
                MainCamMoveAndRotate("cam", "move", "right");
            }
        }

        private void ReceivedCamForth(OSCMessage message)
        {
            if (message.ToInt(out _))
            {
                MainCamMoveAndRotate("cam", "move", "forth");
            }
        }

        private void ReceivedCamBack(OSCMessage message)
        {
            if (message.ToInt(out _))
            {
                MainCamMoveAndRotate("cam", "move", "back");
            }
        }

        private void ReceivedCamPanLeft(OSCMessage message)
        {
            if (message.ToInt(out _))
            {
                MainCamMoveAndRotate("cam", "rotate", "panLeft");
            }
        }

        private void ReceivedCamPanRight(OSCMessage message)
        {
            if (message.ToInt(out _))
            {
                MainCamMoveAndRotate("cam", "rotate", "panRight");
            }
        }

        private void ReceivedCamTiltUp(OSCMessage message)
        {
            if (message.ToInt(out _))
            {
                MainCamMoveAndRotate("cam", "rotate", "tiltUp");
            }
        }

        private void ReceivedCamTiltDown(OSCMessage message)
        {
            if (message.ToInt(out _))
            {
                MainCamMoveAndRotate("cam", "rotate", "tiltDown");
            }
        }

        // gearsets movements
        private void MainGearsMoveAndRotate(string gearSide, string action, string direction, float rotationValue = 0.0f)
        {
            GameObject UIcontrols = GameObject.Find("UI_ctrl_manager_scripts");
            if (UIcontrols == null)
            {
                return;
            }

            string side = gearSide.Trim();
            string actionName = action.Trim();
            string directionName = direction.Trim();

            if (!string.Equals(actionName, "move", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(actionName, "rotate", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            float step = 0.0f;
            Slider targetSliderX = null;
            Slider targetSliderY = null;

            if (string.Equals(actionName, "move", StringComparison.OrdinalIgnoreCase))
            {

                if (string.Equals(side, "leftGears", StringComparison.OrdinalIgnoreCase))
                {
                    step = UIcontrols.GetComponent<SliderToX>().step;
                    targetSliderX = sliderXleft;
                    targetSliderY = sliderYleft;
                }
                else if (string.Equals(side, "rightGears", StringComparison.OrdinalIgnoreCase))
                {
                    step = UIcontrols.GetComponent<SliderToX>().step;
                    targetSliderX = sliderXright;
                    targetSliderY = sliderYright;
                }
                else
                {
                    return;
                }

                if (targetSliderX == null || targetSliderY == null)
                {
                    return;
                }

                switch (directionName.ToLowerInvariant())
                {
                    case "up":
                        targetSliderY.value += step;
                        break;
                    case "down":
                        targetSliderY.value -= step;
                        break;
                    case "left":
                        targetSliderX.value += step;
                        break;
                    case "right":
                        targetSliderX.value -= step;
                        break;
                    default:
                        return;
                }
            }

            else if (string.Equals(actionName, "rotate", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(side, "leftGears", StringComparison.OrdinalIgnoreCase))
                {
                    // rotate left gears
                    leftRot_txtField.text = rotationValue.ToString("F4");
                    GetComponent<UIKeyboard>().OnLeftRotYInputChanged(rotationValue.ToString(""));
                }
                else if (string.Equals(side, "rightGears", StringComparison.OrdinalIgnoreCase))
                {
                    // rotate right gears
                    rightRot_txtField.text = rotationValue.ToString("F4");
                    GetComponent<UIKeyboard>().OnRightRotYInputChanged(rotationValue.ToString(""));
                }
                else
                {
                    return;
                }
            }
        }

        private void ReceivedLeftGearsUp(OSCMessage message)
        {
            if (message.ToInt(out _))
            {
                MainGearsMoveAndRotate("leftGears", "move", "up");
            }
        }

        private void ReceivedLeftGearsDown(OSCMessage message)
        {
            if (message.ToInt(out _))
            {
                MainGearsMoveAndRotate("leftGears", "move", "down");
            }
        }

        private void ReceivedLeftGearsLeft(OSCMessage message)
        {
            if (message.ToInt(out _))
            {
                MainGearsMoveAndRotate("leftGears", "move", "left");
            }
        }

        private void ReceivedLeftGearsRight(OSCMessage message)
        {
            if (message.ToInt(out _))
            {
                MainGearsMoveAndRotate("leftGears", "move", "right");
            }
        }

        private void ReceivedRightGearsUp(OSCMessage message)
        {
            if (message.ToInt(out _))
            {
                MainGearsMoveAndRotate("rightGears", "move", "up");
            }
        }

        private void ReceivedRightGearsDown(OSCMessage message)
        {
            if (message.ToInt(out _))
            {
                MainGearsMoveAndRotate("rightGears", "move", "down");
            }
        }

        private void ReceivedRightGearsLeft(OSCMessage message)
        {
            if (message.ToInt(out _))
            {
                MainGearsMoveAndRotate("rightGears", "move", "left");
            }
        }

        private void ReceivedRightGearsRight(OSCMessage message)
        {
            if (message.ToInt(out _))
            {
                MainGearsMoveAndRotate("rightGears", "move", "right");
            }
        }

        private void ReceivedLeftGearsRotate(OSCMessage message)
        {
            if (message.ToFloat(out var value))
            {
                MainGearsMoveAndRotate("leftGears", "rotate", "noDir", value);
            }
        }

        private void ReceivedRightGearsRotate(OSCMessage message)
        {
            if (message.ToFloat(out var value))
            {
                MainGearsMoveAndRotate("rightGears", "rotate", "noDir", value);
            }
        }

        private void ReceivedLeftGearsSliderY(OSCMessage message)
        {
            if (message.ToFloat(out var value))
            {
                sliderYleft.value = value;
            }
        }

        private void ReceivedLeftGearsSliderX(OSCMessage message)
        {
            if (message.ToFloat(out var value))
            {
                sliderXleft.value = value;
            }
        }

        private void ReceivedRightGearsSliderY(OSCMessage message)
        {
            if (message.ToFloat(out var value))
            {
                sliderYright.value = value;
            }
        }

        private void ReceivedRightGearsSliderX(OSCMessage message)
        {
            if (message.ToFloat(out var value))
            {
                sliderXright.value = value;
            }
        }
        private void ReceivedSssnSave(OSCMessage message)
        {
            if (message.ToInt(out var value))
            {
                SaveSystem.SaveData();
                UIKeyboard uiKeyboard = GetComponent<UIKeyboard>();
                uiKeyboard.savedInfoTxt.gameObject.SetActive(true);
                uiKeyboard.StartCoroutine(uiKeyboard.DeactivateTextAfterDelay(1.3f, uiKeyboard.savedInfoTxt));
            }
        }



        private void ReceivedGearsMatChange(OSCMessage message)
        {
            // Debug.LogFormat("Received: {0}", message);
            if (message.ToInt(out var value))
            {
                for (int i = 0; i < gearsArr.Length; i++)
                {
                    rend = gearsArr[i].GetComponent<Renderer>();
                    gearsMats = rend.materials;
                    if (value >= 0 && value < availableMats.Length)
                    {
                        gearsMats[0] = availableMats[value]; // change the first material of the gear to the selected one from the array
                        rend.sharedMaterials = gearsMats; // assign the modified materials array back to the renderer
                    }
                    else Debug.LogError("Invalid material index: " + value);
                }
            }
        }

        private void ReceivedRingsMatChange(OSCMessage message)
        {
            if (message.ToInt(out var value))
            {
                for (int i = 0; i < ringsArr.Length; i++)
                {
                    rend = ringsArr[i].GetComponent<Renderer>();
                    ringsMats = rend.materials;
                    if (value >= 0 && value < availableMats.Length)
                    {
                        ringsMats[0] = availableMats[value]; // change the first material of the ring to the selected one from the array
                        rend.sharedMaterials = ringsMats; // assign the modified materials array back to the renderer
                    }
                    else Debug.LogError("Invalid material index: " + value);
                }

            }
        }

        private void ReceivedBGMatChange(OSCMessage message)
        {

            if (message.ToInt(out var value))
            {
                rend = BGVideoPanel.GetComponent<Renderer>();
                BGMats = rend.materials;
                if (value >= 0 && value < availableMats.Length)
                {
                    BGMats[0] = availableMats[value]; // change the first material of the BGVideoPanel to the selected one from the array
                    rend.sharedMaterials = BGMats; // assign the modified materials array back to the renderer
                }
                else Debug.LogError("Invalid material index: " + value);
            }
        }


        private void ReceivedGearsFade(OSCMessage message)
        {
            // Renderer[] bobRenderers = FindObjectsOfType<Renderer>();

            if (message.ToFloat(out var value))
            {
                if (gearsArr != null || gearsArr.Length != 0)
                {
                    for (int i = 0; i < gearsArr.Length; i++)
                    {
                        rend = gearsArr[i].GetComponent<Renderer>();
                        gearsMats = rend.materials;
                        gearsMats[0].SetFloat("_Fade", value);

                    }
                }

            }
        }

        private void ReceivedRingsFade(OSCMessage message)
        {
            if (message.ToFloat(out var value))
            {
                if (ringsArr != null || ringsArr.Length != 0)
                {
                    for (int i = 0; i < ringsArr.Length; i++)
                    {
                        rend = ringsArr[i].GetComponent<Renderer>();
                        ringsMats = rend.materials;
                        ringsMats[0].SetFloat("_Fade", value);

                    }
                }

            }
        }

        private void ReceivedBGFade(OSCMessage message)
        {
            if (message.ToFloat(out var value))
            {
                rend = BGVideoPanel.GetComponent<Renderer>();
                BGMats = rend.materials;
                BGMats[0].SetFloat("_Fade", value);
            }
        }

        private void ReceivedHandsFade(OSCMessage message)
        {
            if (message.ToFloat(out var value))
            {
                rend = hands.GetComponent<Renderer>();
                Material[] handsMats = rend.materials;
                handsMats[0].SetFloat("_Fade", value);
            }
        }

        private void ReceivedPortalScale(OSCMessage message)
        {
            if (message.ToFloat(out var value))
            {
                rend = BGVideoPanel.GetComponent<Renderer>();
                BGMats = rend.materials;
                BGMats[0].SetFloat("_UVScale", value);

                if (gearsArr != null || gearsArr.Length != 0)
                {
                    for (int i = 0; i < gearsArr.Length; i++)
                    {
                        rend1 = gearsArr[i].GetComponent<Renderer>();
                        gearsMats = rend1.materials;
                        gearsMats[0].SetFloat("_UVScale", value);
                    }
                }
            }
        }

        private void ReceivedPortalMove(OSCMessage message)
        {
            if (message.ToFloat(out var value))
            {
                rend = BGVideoPanel.GetComponent<Renderer>();
                BGMats = rend.materials;
                BGMats[0].SetFloat("_UVPivotMove", value);

                if (gearsArr != null || gearsArr.Length != 0)
                {
                    for (int i = 0; i < gearsArr.Length; i++)
                    {
                        rend1 = gearsArr[i].GetComponent<Renderer>();
                        gearsMats = rend1.materials;
                        gearsMats[0].SetFloat("_UVPivotMove", value);
                    }
                }
            }
        }





        private void ReceivedVFXactivate(OSCMessage message)
        {
            // options to get values out of an incoming message array, e.g. from O-S-C
            // message.Values[0].IntValue
            // message.Values[0].FloatValue
            // message.Values[0].StringValue

            // hands on/off
            if (message.Values[0].IntValue == 0)
            {
                hands.SetActive(false);
            }
            if (message.Values[0].IntValue == 1)
            {
                hands.SetActive(true);
            }
            // hand trail burst  
            // if (message.Values[1].IntValue == 0)
            // {
            //     // hands.SetActive(false);
            // }
            if (message.Values[1].IntValue == 1 && !hands.GetComponent<MeshTrail>().isTrailActive && hands.activeSelf)
            {
                hands.GetComponent<MeshTrail>().isTrailActive = true;
                StartCoroutine(hands.GetComponent<MeshTrail>().ActivateTrail(hands.GetComponent<MeshTrail>().activeTime));
            }

            // hands particles on/off
            if (message.Values[2].IntValue == 0)
            {
                VFXHandParticles.SetActive(false);
            }
            if (message.Values[2].IntValue == 1)
            {
                VFXHandParticles.SetActive(true);
            }


            // surfaceRipple - particle traverse from planetary rings
            if (message.Values[3].IntValue == 0)
            {
                VFXsurfaceRipple.SetActive(false);
            }
            if (message.Values[3].IntValue == 1)
            {
                VFXsurfaceRipple.SetActive(true);
            }

            // crawlers - particle swarm on/off
            if (message.Values[4].IntValue == 0)
            {
                for (int i = 0; i < VFXobjs.Length; i++)
                {
                    VFXobjs[i].SetActive(false);
                }
            }
            if (message.Values[4].IntValue == 1)
            {
                for (int i = 0; i < VFXobjs.Length; i++)
                {
                    VFXobjs[i].SetActive(true);
                }
            }



            // for (int i = 0; i < message.Values.Count; i++)
            // {
            //     Debug.LogFormat(message.Values[i].ToString());
            // }

        }

        private void ReceivedVFXtrails(OSCMessage message)
        {
            if (message.ToInt(out var value))
            {
                if (value == 1 && !hands.GetComponent<MeshTrail>().isTrailActive)
                {
                    // Debug.LogFormat("Received: set burstTrails to {0}", value);
                    // Handle VFX trails logic here
                    hands.GetComponent<MeshTrail>().isTrailActive = true;
                    StartCoroutine(hands.GetComponent<MeshTrail>().ActivateTrail(hands.GetComponent<MeshTrail>().activeTime));
                }
                else Debug.LogError("BurstTrails error: sth. went wrong. ");
            }
        }

        private void ReceivedVFXhandPartsFade(OSCMessage message)
        {
            if (message.ToFloat(out var value))
            {
                VFXHandParticles.GetComponent<VisualEffect>().SetFloat("Fade", value);
            }
        }
        private void ReceivedVFXfloatingPartsFade(OSCMessage message)
        {
            if (message.ToFloat(out var value))
            {
                VFXsurfaceRipple.GetComponent<VisualEffect>().SetFloat("Fade", value);
            }
        }

        private void ReceivedVFXcrawlersFade(OSCMessage message)
        {
            if (message.ToFloat(out var value))
            {
                for (int i = 0; i < VFXobjs.Length; i++)
                {
                    VFXobjs[i].GetComponent<VisualEffect>().SetFloat("Fade", value);
                }
            }
        }
        private void ReceivedVFXcrawlersRate(OSCMessage message)
        {
            if (message.ToFloat(out var value))
            {
                for (int i = 0; i < VFXobjs.Length; i++)
                {
                    VFXobjs[i].GetComponent<VisualEffect>().SetFloat("SpawnRate", value);

                }
            }
        }

        private void ReceivedVFXcrawlersDist(OSCMessage message)
        {
            if (message.ToFloat(out var value))
            {

                for (int i = 0; i < VFXobjs.Length; i++)
                {
                    VFXobjs[i].GetComponent<VisualEffect>().SetFloat("StickDistance", value);
                }
            }
        }
        #endregion
    }
}



