using UnityEngine;
using System.Collections.Generic;

namespace Unity3dAzure.WebSockets {
public class DoorController : UnityWebSocket
{
    [SerializeField]
    private Animator m_DoorAnimator;
    [SerializeField]
    private string m_OpeningClipName;
    [SerializeField]
    private string m_ClosingClipName;

    private string webSocketUri = "ws://a9nodered.azurewebsites.net/ws/adtin";
    private List<UnityKeyValue> headers;


    public bool IsOpened { get; private set; }
    void Start()
    {
        HouseManager.OnStateChanged += OnStateChanged;
        // Config Websocket
        WebSocketUri = webSocketUri;
        Headers = headers;
        Connect();
        
    }

    private void OnStateChanged(HouseManager.HouseState state)
    {
        switch (state)
        {
            case HouseManager.HouseState.Open:
                Open();
                //string openCMD = 'xxxxxxx';
                SendText("{\"myCMD\":\"Hello Demo\"}");
                break;
            case HouseManager.HouseState.Close:
            default:
                Close();
                break;
        }
    }

    public void Open()
    {
        Debug.Log("Play Open Door");
        m_DoorAnimator.Play(m_OpeningClipName);
        IsOpened = true;
    }

    public void Close()
    {
        m_DoorAnimator.Play(m_ClosingClipName);
        IsOpened = false;
    }


}

}