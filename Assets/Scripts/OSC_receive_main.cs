using UnityEngine;
using UnityEngine.Video;


namespace extOSC
{
    public class OSC_receive_main : MonoBehaviour
    {



        #region Public Vars

        [Header("OSC Settings")]
        public OSCReceiver Receiver;

        public string videoActiveAddr = "/BGVideo/setActive";
        public string videoAlphaAddr = "/BGVideo/setAlpha";
        public string videoIntensityAddr = "/BGVideo/setIntensity";
        public string videoClipAddr = "/BGVideo/setClip";
        public string floorMoveUpAddr = "/floor/moveUp";
        public string floorScaleAddr = "/floor/setScale";

        public GameObject BGVideoPanel;
        public Material videoMaterial;
        public GameObject floorPanel;


        [SerializeField] private string fileName0;
        [SerializeField] private string fileName1;
        [SerializeField] private string fileName2;
        [SerializeField] private string fileName3;
        [SerializeField] private string fileName4;
        private VideoPlayer BGvideoPlayer;

        #endregion

        #region Unity Methods

        protected virtual void Start()
        {

            Receiver.Bind(videoActiveAddr, ReceivedVideoActive);
            Receiver.Bind(videoAlphaAddr, ReceivedVideoAlpha);
            Receiver.Bind(videoIntensityAddr, ReceivedVideoIntensity);
            Receiver.Bind(videoClipAddr, ReceivedVideoClip);
            Receiver.Bind(floorMoveUpAddr, ReceivedFloorMoveUp);
            Receiver.Bind(floorScaleAddr, ReceivedFloorScale);
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

        private void ReceivedVideoAlpha(OSCMessage message)
        {
            // Debug.LogFormat("Received: {0}", message);
            if (message.ToFloat(out var value))
            {
                // ATTENTION! The shader's variables have to be addressed with a leading underscore "_" even if they are not named like that in ShaderGraph
                videoMaterial.SetFloat("_AlphaRemapMax", value);
                SceneTransition.bgVideoAlpha = value; // the static float to pas to other scenes, load it from there in SceneTransition.cs and set it to the shader's variable there as well
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
                videoMaterial.SetColor("_EmissiveColor", baseEmissive * value);

                // ensure emission is enabled
                videoMaterial.EnableKeyword("_EMISSION");

                Debug.LogFormat("Received: set video emission intensity to {0}", value);
            }

        }

        private void ReceivedVideoClip(OSCMessage message)
        {
            BGvideoPlayer = BGVideoPanel.GetComponentInChildren<VideoPlayer>();
            // Debug.LogFormat("Received: {0}", message);
            if (message.ToInt(out var value))
            {
                if (value == 0)
                {
                    Debug.Log("Trying to load clip: " + fileName0);
                    VideoClip clip = Resources.Load<VideoClip>("Videos/" + fileName0); // no extension 

                    if (clip != null && BGvideoPlayer != null)
                    {
                        BGvideoPlayer.source = UnityEngine.Video.VideoSource.VideoClip;
                        BGvideoPlayer.clip = clip;
                        BGvideoPlayer.Play();
                        SceneTransition.bgVideoTitle = fileName0; // set the static string to the name of the currently playing video clip, so it can be accessed by other SCENES   
                    }
                    // LogError holds up the whole program and prevents it from running, so I changed it to Debug.Log.
                    // else Debug.LogError("Failed to load or player is null: " + fileName0);
                    else Debug.Log("Failed to load or player is null: " + fileName0);
                }
                else if (value == 1)
                {
                    Debug.Log("Trying to load clip: " + fileName1);
                    VideoClip clip = Resources.Load<VideoClip>("Videos/" + fileName1); // no extension 

                    if (clip != null && BGvideoPlayer != null)
                    {
                        BGvideoPlayer.source = UnityEngine.Video.VideoSource.VideoClip;
                        BGvideoPlayer.clip = clip;
                        BGvideoPlayer.Play();
                        SceneTransition.bgVideoTitle = fileName1; // set the static string to the name of the currently playing video clip, so it can be accessed by other SCENES   
                    }
                    else Debug.Log("Failed to load or player is null: " + fileName1);

                }
                else if (value == 2)
                {
                    Debug.Log("Trying to load clip: " + fileName2);
                    VideoClip clip = Resources.Load<VideoClip>("Videos/" + fileName2); // no extension 

                    if (clip != null && BGvideoPlayer != null)
                    {
                        BGvideoPlayer.source = UnityEngine.Video.VideoSource.VideoClip;
                        BGvideoPlayer.clip = clip;
                        BGvideoPlayer.Play();
                        SceneTransition.bgVideoTitle = fileName2; // set the static string to the name of the currently playing video clip, so it can be accessed by other SCENES   
                    }
                    else Debug.Log("Failed to load or player is null: " + fileName2);
                }

                else if (value == 3)
                {
                    Debug.Log("Trying to load clip: " + fileName3);
                    VideoClip clip = Resources.Load<VideoClip>("Videos/" + fileName3); // no extension 

                    if (clip != null && BGvideoPlayer != null)
                    {
                        BGvideoPlayer.source = UnityEngine.Video.VideoSource.VideoClip;
                        BGvideoPlayer.clip = clip;
                        BGvideoPlayer.Play();
                        SceneTransition.bgVideoTitle = fileName3; // set the static string to the name of the currently playing video clip, so it can be accessed by other SCENES   
                    }
                    else Debug.Log("Failed to load or player is null: " + fileName3);
                }
                else if (value == 4)
                {
                    Debug.Log("Trying to load clip: " + fileName4);
                    VideoClip clip = Resources.Load<VideoClip>("Videos/" + fileName4); // no extension 

                    if (clip != null && BGvideoPlayer != null)
                    {
                        BGvideoPlayer.source = UnityEngine.Video.VideoSource.VideoClip;
                        BGvideoPlayer.clip = clip;
                        BGvideoPlayer.Play();
                        SceneTransition.bgVideoTitle = fileName4; // set the static string to the name of the currently playing video clip, so it can be accessed by other SCENES   
                    }
                    else Debug.Log("Failed to load or player is null: " + fileName4);
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
        #endregion
    }
}



