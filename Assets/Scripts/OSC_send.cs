using UnityEngine;
using System;
using UnityEngine.UI;

namespace extOSC
{
    public class OSC_send : MonoBehaviour
    {

        #region Public Vars

        public string showUIAddr = "/UI/show";
        public string lLockAddr = "/leftGears/lock";
        public string rLockAddr = "/rightGears/lock";
        public string camLockAddr = "/cam/lock";
        public string showHelpAddr = "/show/help";
        public string fineTuneAddr = "/UI/fineTune";
        public string leftSliderYAddr = "/leftGears/sliderY";
        public string leftSliderXAddr = "/leftGears/sliderX";
        public string rightSliderYAddr = "/rightGears/sliderY";
        public string rightSliderXAddr = "/rightGears/sliderX";
        public string opSliderAddr = "/UI/opacity";

        [Header("OSC Settings")]
        public OSCTransmitter Transmitter;

        #endregion

        #region Unity Methods

        public void showUI(bool isOn)
        {
            int val = Convert.ToInt32(isOn);
            var message = new OSCMessage(showUIAddr);
            message.AddValue(OSCValue.Int(val));
            Transmitter.Send(message);
        }
        public void LockLeft(Toggle toggle) // this enables possibility to add the toggle in
                                            // Unity Inspector under "onValueChanged", otherwise fixed value "0"
        {
            bool op = !toggle.isOn; // needs to be opposite for O-S-C buttons
            int val = Convert.ToInt32(op);
            var message = new OSCMessage(lLockAddr);
            message.AddValue(OSCValue.Int(val));
            Transmitter.Send(message);
        }
        public void LockRight(Toggle toggle) // this enables possibility to add the toggle in
                                             // Unity Inspector under "onValueChanged", otherwise fixed value "0"
        {
            bool op = !toggle.isOn; // needs to be opposite for O-S-C buttons
            int val = Convert.ToInt32(op);
            var message = new OSCMessage(rLockAddr);
            message.AddValue(OSCValue.Int(val));
            Transmitter.Send(message);
        }
        public void LockCam(Toggle toggle) // this enables possibility to add the toggle in
                                           // Unity Inspector under "onValueChanged", otherwise fixed value "0"
        {
            bool op = !toggle.isOn; // needs to be opposite for O-S-C buttons
            int val = Convert.ToInt32(op);
            var message = new OSCMessage(camLockAddr);
            message.AddValue(OSCValue.Int(val));
            Transmitter.Send(message);
        }
        public void showHelp(Toggle toggle) // this enables possibility to add the toggle in
                                            // Unity Inspector under "onValueChanged", otherwise fixed value "0"
        {
            int val = Convert.ToInt32(toggle.isOn);
            var message = new OSCMessage(showHelpAddr);
            message.AddValue(OSCValue.Int(val));
            Transmitter.Send(message);
        }
        public void fineTune(Toggle toggle) // this enables possibility to add the toggle in
                                            // Unity Inspector under "onValueChanged", otherwise fixed value "0"
        {
            int val = Convert.ToInt32(toggle.isOn);
            var message = new OSCMessage(fineTuneAddr);
            message.AddValue(OSCValue.Int(val));
            Transmitter.Send(message);
        }

        // UI sliders
        public void leftYslider(Slider slider)
        {
            // int val = Convert.ToInt32(toggle.isOn);
            var message = new OSCMessage(leftSliderYAddr);
            message.AddValue(OSCValue.Float(slider.value));
            Transmitter.Send(message);
        }
        public void leftXslider(Slider slider)
        {
            // int val = Convert.ToInt32(toggle.isOn);
            var message = new OSCMessage(leftSliderXAddr);
            message.AddValue(OSCValue.Float(slider.value));
            Transmitter.Send(message);
        }
        public void rightYslider(Slider slider)
        {
            // int val = Convert.ToInt32(toggle.isOn);
            var message = new OSCMessage(rightSliderYAddr);
            message.AddValue(OSCValue.Float(slider.value));
            Transmitter.Send(message);
        }
        public void rightXslider(Slider slider)
        {
            // int val = Convert.ToInt32(toggle.isOn);
            var message = new OSCMessage(rightSliderXAddr);
            message.AddValue(OSCValue.Float(slider.value));
            Transmitter.Send(message);
        }
        public void opacitySlider(Slider slider)
        {
            // int val = Convert.ToInt32(toggle.isOn);
            var message = new OSCMessage(opSliderAddr);
            message.AddValue(OSCValue.Float(slider.value));
            Transmitter.Send(message);
        }

        // protected virtual void Start()
        // {
        //     var message = new OSCMessage(Address);
        //     message.AddValue(OSCValue.String("Hello, world!"));

        //     Transmitter.Send(message);
        // }

        // #endregion

        void Start()
        {
            // var message = new OSCMessage(Address);
            // message.AddValue(OSCValue.String("Unity running!"));

            // Transmitter.Send(message);
        }

        void sendTimePassed(int valueIn)
        {
            // var message = new OSCMessage(Address);
            // // message.AddValue(OSCValue.String("Hello, world!"));
            // message.AddValue(OSCValue.Int(valueIn));

            // Transmitter.Send(message);
        }

        // Update is called once per frame
        void Update()
        {

        }
        #endregion
    }
}
