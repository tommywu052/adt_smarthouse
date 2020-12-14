using System;
using UnityEngine;

namespace Unity3dAzure.WebSockets
{
    [RequireComponent(typeof(FanController))]
    public class FanDataReceiver : DataReceiver
    {
        private readonly string dataName = "Relay2";
        private FanController fanController;
        private bool needsUpdated = false;
        bool isFanTurnOn = false;

        private void Start()
        {
            fanController = GetComponent<FanController>();
        }

        void Update()
        {
            if (!needsUpdated)
            {
                return;
            }
            needsUpdated = false;
            _UpdateFanState();
        }

        private void _UpdateFanState()
        {
            if (isFanTurnOn && fanController.IsStop)
            {
                fanController.TurnOn();
            }
            else if (!isFanTurnOn && !fanController.IsStop)
            {
                fanController.TurnOff();
            }
        }
        override public void OnReceivedData(object sender, EventArgs args)
        {
            if (args == null) return;
            var myArgs = args as TextEventArgs;
            //Debug.Log("Fans Relay2 update =="+myArgs.Text);
            if (myArgs == null || !myArgs.Text.Contains(dataName)) return;
            Debug.Log("======Enter Fan On Receive ======"+myArgs.Text.Split(':')[1]);
            var _state = myArgs.Text.Split(':')[1];
            Debug.Log("Enter Fan update =="+_state);
            //if (_state == "0") isFanTurnOn = false;
            //if (_state == "1") isFanTurnOn = true;
            Debug.Log("Enter Fan Status =="+isFanTurnOn);
            Debug.Log("Isstop:"+fanController.IsStop);
            if(isFanTurnOn)
            isFanTurnOn = false;
            else
            isFanTurnOn = true;
            
            needsUpdated = true;
        }
    }
}