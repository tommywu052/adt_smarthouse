using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Unity3dAzure.WebSockets
{
    [RequireComponent(typeof(DoorController))]
    public class DoorDataReceiver : DataReceiver
    {
        private readonly string dataName = "Servo";
        private DoorController doorController;
        private bool needsUpdated = false;

        private float secondToClose = 10.0f;
        private Coroutine autoClose;
        
        bool isOpenDoor;

        private void Start()
        {
            doorController = GetComponent<DoorController>();
        }

        void Update()
        {
            if (!needsUpdated)
            {
                return;
            }
            needsUpdated = false;
            _UpdateDoorState();;
        }

        private void _UpdateDoorState()
        {
             if (isOpenDoor && !doorController.IsOpened)
            {
                doorController.Open();
                if (autoClose != null)
                   StopCoroutine(autoClose);
                autoClose = StartCoroutine(AutoClose());
            }
            else if (!isOpenDoor && doorController.IsOpened)
            {
                doorController.Close();
            }
        }

         private IEnumerator AutoClose()
        {
            yield return new WaitForSecondsRealtime(secondToClose);
            isOpenDoor = false;
            _UpdateDoorState();
            autoClose = null;
        }
        override public void OnReceivedData(object sender, EventArgs args)
        {
            if (args == null) return;
            var myArgs = args as TextEventArgs;
            //Debug.Log("Fans Relay2 update =="+myArgs.Text);
            if (myArgs == null || !myArgs.Text.Contains(dataName)) return;
            Debug.Log("======Enter Servo On Receive ======"+myArgs.Text.Split(':')[1]);
            var _state = myArgs.Text.Split(':')[1];
            Debug.Log("Enter Servo update =="+_state);
            isOpenDoor ^= true;
            //if (_state == "0") isFanTurnOn = false;
            //if (_state == "1") isFanTurnOn = true;
            //Debug.Log("Enter Fan Status =="+isFanTurnOn);
            //Debug.Log("Isstop:"+doorController.IsOpened);
            //if(isFanTurnOn)
            //isFanTurnOn = false;
            //else
            //isFanTurnOn = true;
            
            needsUpdated = true;
        }
    }
}