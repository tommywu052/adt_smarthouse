using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Unity3dAzure.WebSockets
{
    
    [Serializable]
    //{"Temperature":29,"Humidity":43,"Flame":610,"Light":177,"Distance":16.6496,"Smoke":79,"PIRState":false}
    public class HomeSensors
    {
        public int Temperature;
        public int Humidity;
        public int Flame;
        public int Light;
        public float Distance;
        public int Smoke;
        public string LCD_Msg;

        public string RGB_out;
        public string RGB_in;

        public int Servo;

        public Boolean PIRState;
        
    }
        
    // Updates TextMesh component text with received data
    [RequireComponent(typeof(TextMesh))]
    public class TextMeshReceiver : DataReceiver
    {
        //public FanController m_FanController;
        public DoorController m_DoorController;
        private DoorController p_door;
        protected string text;
        protected bool needsUpdated = false;
        protected string pname;

        protected HomeSensors mySensors;

        public TextMesh textMesh;
        private float secondToClose = 10.0f;
        private Coroutine autoClose;
        
        
        bool isOpenDoor;

        void Awake()
        {
            textMesh = gameObject.GetComponent<TextMesh>();
            pname = textMesh.transform.parent.name;
            //p_door = m_DoorController;
            //m_DoorController.Open();
            //m_FanController.TurnOn();
            //gameObject.GetComponent<FanController>().TurnOn();
            Debug.Log("Player's Parent: " + pname);
            //InvokeRepeating("_UpdateDoorState", 0.0f, 1.0f);
        }

        void Update()
        {
            if (!needsUpdated)
            {
                return;
            }
            // update TextMesh component on this gameObject

            //string pname = textMesh.transform.parent.name;
            //Debug.Log("TextMesh's Parent: " +pname);
            //if (pname.Equals("thermometer")) {
            //Debug.Log("TextMesh's Parent: equals thermometer");
            textMesh.text = text;
            needsUpdated = false;
            //} else {
            //  Debug.Log("TextMesh's Parent: equals lcdpanel");
            //  needsUpdated = false;
            //}

            _UpdateDoorState();
        }

        private void _UpdateDoorState()
        {
            if (isOpenDoor && !m_DoorController.IsOpened)
            {
                m_DoorController.Open();
                if (autoClose != null)
                    StopCoroutine(autoClose);
                autoClose = StartCoroutine(AutoClose());
            }
            else if (!isOpenDoor && m_DoorController.IsOpened)
            {
                m_DoorController.Close();
            }
        }

        private IEnumerator AutoClose()
        {
            yield return new WaitForSecondsRealtime(secondToClose);
            isOpenDoor = false;
            _UpdateDoorState();
            autoClose = null;
        }

        // Override this method in your own subclass to process the received event data
        override public void OnReceivedData(object sender, EventArgs args)
        {
            //return;
            if (args == null)
            {
                return;
            }
            var myArgs = args as TextEventArgs;
            if (myArgs == null)
            {
                return;
            }

            mySensors = JsonUtility.FromJson<HomeSensors>(myArgs.Text);
            //Debug.Log("=========Flame===========");
            //Debug.Log(mySensors.Flame);
            //Debug.Log("AAAAAAAAAAAAA");
            //isOpenDoor ^= true;
            if (pname == " thermometer")
            {
                if (myArgs.Text.Contains("LCD_Msg")) return;
                text = "T:"+mySensors.Temperature.ToString()+"\nH:"+mySensors.Humidity.ToString();
                needsUpdated = true;
                if (myArgs.Text.Contains("Servo"))
                {
                    Debug.Log("Open Door");
                    isOpenDoor ^= true;
                }
            } else if (pname == "Flame")
            {
                
                if (myArgs.Text.Contains("LCD_Msg")) return;

                //Debug.Log(myArgs.Text);
                //mySensors = JsonUtility.FromJson<HomeSensors>(myArgs.Text);
                
                if(mySensors.Flame > 950)
                text = "No Fire";
                else
                text = "Fire!!";
                //text = mySensors.Flame.ToString();
                needsUpdated = true;
                
            } else if (pname == "Smoke")
            {
                
                if (myArgs.Text.Contains("LCD_Msg")) return;

                //Debug.Log(myArgs.Text);
                //mySensors = JsonUtility.FromJson<HomeSensors>(myArgs.Text);
                
                if(mySensors.Smoke <90)
                text = "Normal";
                else
                text = "Alert";

                //text = mySensors.Smoke.ToString();
                needsUpdated = true;
                
            } else if (pname == "Light")
            {
                
                if (myArgs.Text.Contains("LCD_Msg")) return;

                //Debug.Log(myArgs.Text);
                //mySensors = JsonUtility.FromJson<HomeSensors>(myArgs.Text);
                
                if(mySensors.Light < 500)
                text = "Light ON";
                else {
                text = "Light Off";
                isOpenDoor ^= true;
                }
                //text = mySensors.Light.ToString();
                needsUpdated = true;
                
            } else if (pname == "buzzer_r")
            {
                
                if (myArgs.Text.Contains("LCD_Msg")) return;

                //Debug.Log(myArgs.Text);
                //mySensors = JsonUtility.FromJson<HomeSensors>(myArgs.Text);
                
                if(mySensors.Distance <30 )
                text = "Near By";
                else if(mySensors.Distance >30 && mySensors.Distance < 200)
                text = "Keep";
                else
                text = "Far Away";
                //text = Mathf.RoundToInt(mySensors.Distance).ToString();
                needsUpdated = true;
                
            } else if (pname == "PIRState")
            {
                
                if (myArgs.Text.Contains("LCD_Msg")) return;

                //Debug.Log(myArgs.Text);
                //mySensors = JsonUtility.FromJson<HomeSensors>(myArgs.Text);
                
                if(mySensors.PIRState)
                    text = "People";
                else
                    text = "Empty";
                //text = mySensors.Light.ToString();
                needsUpdated = true;
                
            } else if (pname == "RGB_out")
            {
                
                if (myArgs.Text.Contains("LCD_Msg")) return;

                //Debug.Log(myArgs.Text);
                //mySensors = JsonUtility.FromJson<HomeSensors>(myArgs.Text);
                
                //if(mySensors.RGB_out)
                //    text = "People";
                //else
                //    text = "Empty";
                text = mySensors.RGB_out;
                needsUpdated = true;
                
            }
            else
            {
                if (myArgs.Text.Contains("LCD_Msg"))
                {
                    
                    text = mySensors.LCD_Msg;
                    Debug.Log("=======LCDMSG======="+mySensors.LCD_Msg);
                    //text = myArgs.Text.Split(':')[1];
                    needsUpdated = true;
                }
                else
                {
                    needsUpdated = false;
                }

            }
        }
    }

}
