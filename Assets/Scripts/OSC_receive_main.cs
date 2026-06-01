using UnityEngine;
using UnityEngine.Video;
using System;


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
        [SerializeField] private string fileName5;
        [SerializeField] private string fileName6;
        [SerializeField] private string fileName7;
        [SerializeField] private string fileName8;
        [SerializeField] private string fileName9;
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



