using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FanController : MonoBehaviour
{
    [SerializeField]
    GameObject m_FanBlade;
    //[SerializeField]
    bool m_IsReversed;
    [SerializeField]
    float m_MaxSpeed;
    [SerializeField]
    float m_Duratrion;

    float targetSpeed;
    [SerializeField]
    float currentSpeed;
    Coroutine SpinningCorountine;
    public bool IsStop = true;// { get; private set; }

    private string webSocketUri = "ws://127.0.0.1:8080";
    //private List<UnityKeyValue> headers;

    public bool IsOn
    {
        get
        {
            return SpinningCorountine != null;
        }
    }

    private void Start()
    {
        HouseManager.OnStateChanged += OnStateChanged;
         // Config Websocket
        //WebSocketUri = webSocketUri;
        //Headers = headers;
    }

    private void OnStateChanged(HouseManager.HouseState state)
    {
        switch (state)
        {
            case HouseManager.HouseState.Open:
                //TurnOn();
                break;
            case HouseManager.HouseState.Close:
                //TurnOff();
                break;
            default:
                break;
        }
    }
    public void TurnOn()
    {
        IsStop = false;
        targetSpeed = m_MaxSpeed;
        if (SpinningCorountine == null)
            SpinningCorountine = StartCoroutine(Spinning());
    }
    public void TurnOff()
    {
        IsStop = true;
        if (SpinningCorountine == null)
            SpinningCorountine = StartCoroutine(Spinning());
    }

    private IEnumerator Spinning()
    {
        float time = 0.0f;
        while (true)
        {
            if (IsStop || m_IsReversed)
            {
                time = time - Time.deltaTime;
                if (time < 0) time = IsStop ? 0.0f : Mathf.Abs(time);
            }
            else if (time < m_Duratrion)
            {
                time = time + Time.deltaTime;
            }

            if (time > m_Duratrion) time = m_Duratrion;



            int r = m_IsReversed ? -1 : 1;

            currentSpeed = Mathf.SmoothStep(0.0f, m_MaxSpeed, time / m_Duratrion);
            //Debug.Log($"Time : {time} CurrentSpeed : {currentSpeed}");
            var rotation = Quaternion.AngleAxis(r * 180, Vector3.forward);
            m_FanBlade.transform.rotation = Quaternion.Slerp(m_FanBlade.transform.rotation, m_FanBlade.transform.rotation * rotation, currentSpeed * Time.deltaTime);
            if (IsStop && Mathf.Approximately(currentSpeed, 0.0f))
            {
                SpinningCorountine = null;
                yield break;
            }
            yield return null;
        }
    }
}
