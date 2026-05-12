using UnityEngine;

namespace extOSC
{
    public class OSC_receive_gears : MonoBehaviour
    {
        #region Public Vars

        [Header("OSC Settings")]
        public OSCReceiver Receiver;

        public string degreesAddr = "/setDegrees";
        public string secondsAddr = "/setSeconds";
        public string moveAddr = "/autoRotate";
        public string resetAddr = "/reset";
        public string brightAddr = "/shader/brightness";
        public string outlineAddr = "/shader/outline";
        public string distortionAddr = "/shader/distortion";
        public GameObject gearSet;

        public Material materialMain;

        #endregion

        #region Unity Methods

        protected virtual void Start()
        {

            Receiver.Bind(degreesAddr, ReceivedDegrees);
            Receiver.Bind(secondsAddr, ReceivedSeconds);
            Receiver.Bind(moveAddr, ReceivedMove);
            Receiver.Bind(resetAddr, ReceivedReset);
            Receiver.Bind(brightAddr, ReceivedShaderBrightness);
            Receiver.Bind(outlineAddr, ReceivedShaderOutline);
            Receiver.Bind(distortionAddr, ReceivedShaderDistortion);
        }

        #endregion

        #region Private Methods

        private void ReceivedDegrees(OSCMessage message)
        {
            // Debug.LogFormat("Received: {0}", message);
            if (message.ToFloat(out var value))
            {
                Debug.Log(value.ToString());
                gearSet.GetComponent<GearRotations>().moveToDegree = value;
            }

        }
        private void ReceivedSeconds(OSCMessage message)
        {
            // Debug.LogFormat("Received: {0}", message);
            if (message.ToFloat(out var value))
            {
                gearSet.GetComponent<GearRotations>().duration = value;
            }

        }
        private void ReceivedMove(OSCMessage message)
        {
            // Debug.LogFormat("Received: {0}", message);
            if (message.ToInt(out var value))
            {
                bool myBool = (value != 0);
                gearSet.GetComponent<GearRotations>().autoRotate = myBool;
            }

        }
        private void ReceivedReset(OSCMessage message)
        {
            // Debug.LogFormat("Received: {0}", message);
            if (message.ToInt(out var value))
            {
                float prevMtoDeg = gearSet.GetComponent<GearRotations>().moveToDegree;
                float prevDur = gearSet.GetComponent<GearRotations>().duration;
                gearSet.GetComponent<GearRotations>().moveToDegree = 0f;
                gearSet.GetComponent<GearRotations>().duration = 0.1f;
                if (message.Address == "/resetL")
                    gearSet.GetComponent<GearRotations>().rotateLeftGearSet(gearSet.GetComponent<GearRotations>().masterRotation, gearSet.GetComponent<GearRotations>().radians, gearSet.GetComponent<GearRotations>().midPoint, Quaternion.Euler(0, 0, 0)); ;
                if (message.Address == "/resetR")
                    gearSet.GetComponent<GearRotations>().rotateRightGearSet(gearSet.GetComponent<GearRotations>().masterRotation, gearSet.GetComponent<GearRotations>().radians, gearSet.GetComponent<GearRotations>().midPoint, Quaternion.Euler(0, 180, 0)); ;
                gearSet.GetComponent<GearRotations>().moveToDegree = prevMtoDeg;
                gearSet.GetComponent<GearRotations>().duration = prevDur;
            }

        }

        ////////////////// SHADER STUFF ////////////////////
        /// 
        private void ReceivedShaderBrightness(OSCMessage message)
        {
            // Debug.LogFormat("Received: {0}", message);
            if (message.ToFloat(out var value))
            {
                // ATTENTION! The shader's variables have to be addressed with a leading underscore "_" even if they are not named like that in ShaderGraph
                materialMain.SetFloat("_BrightnessAmount", value);
                // Debug.Log("Getting brightness: " + value);

            }

        }
        private void ReceivedShaderOutline(OSCMessage message)
        {
            // Debug.LogFormat("Received: {0}", message);
            if (message.ToInt(out var value))
            {
                // ATTENTION! The shader's variables have to be addressed with a leading underscore "_" even if they are not named like that in ShaderGraph
                materialMain.SetInt("_Outline", value);
                // bool myBool = (value != 0);
                // gearSet.GetComponent<GearRotations>().autoRotate = myBool;

            }

        }

        private void ReceivedShaderDistortion(OSCMessage message)
        {
            // Debug.LogFormat("Received: {0}", message);
            if (message.ToFloat(out var value))
            {
                // ATTENTION! The shader's variables have to be addressed with a leading underscore "_" even if they are not named like that in ShaderGraph
                materialMain.SetFloat("_DistortionStrength", value);
                // Debug.Log("Getting brightness: " + value);

            }

        }

        #endregion
    }
}
