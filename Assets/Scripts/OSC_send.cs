using UnityEngine;

namespace extOSC
{
    public class OSC_send : MonoBehaviour
    {

        #region Public Vars

        public string Address = "/example/1";

        [Header("OSC Settings")]
        public OSCTransmitter Transmitter;

        #endregion

        #region Unity Methods

        // protected virtual void Start()
        // {
        //     var message = new OSCMessage(Address);
        //     message.AddValue(OSCValue.String("Hello, world!"));

        //     Transmitter.Send(message);
        // }

        // #endregion    

        void Start()
        {
            var message = new OSCMessage(Address);
            message.AddValue(OSCValue.String("Unity running!"));

            Transmitter.Send(message);
        }

        void sendTimePassed(int valueIn)
        {
            var message = new OSCMessage(Address);
            // message.AddValue(OSCValue.String("Hello, world!"));
            message.AddValue(OSCValue.Int(valueIn));

            Transmitter.Send(message);
        }

        // Update is called once per frame
        void Update()
        {

        }
        #endregion
    }
}
