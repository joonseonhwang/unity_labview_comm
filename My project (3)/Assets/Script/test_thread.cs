using System.Collections;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using UnityEngine.UI;

public class test_thread : MonoBehaviour
{
    // Start is called before the first frame update
    UdpClient client;
    public  float timeRemaining = 0;
    public Text timeText;

    void Start()
    {
        client = new UdpClient(5500);
        StartCoroutine(LoopFunction(0.001f));
    }


    void Update()
    {
        timeRemaining += Time.deltaTime;
        timeText.text = timeRemaining.ToString();
    }

private IEnumerator LoopFunction(float waitTime)
{
    while (true)
    {
       // yield return new WaitForSecondsRealtime(waitTime);
       // yield return new WaitForSeconds(waitTime);
        //Second Log show passed waitTime (waitTime is float type value ) 
        UDPTest();
       // UDPsend();
       yield return null;


    }
}

private void UDPTest()
    {

        try
        {
           

            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 5200);
byte[] receiveBytes = client.Receive(ref remoteEndPoint);
            string receivedString = Encoding.ASCII.GetString(receiveBytes);
     print(receivedString);


            client.Connect("127.0.0.1", 5300);
            byte[] sendBytes = Encoding.ASCII.GetBytes(timeRemaining.ToString());
                 print(timeRemaining.ToString());

            client.Send(sendBytes, sendBytes.Length);

        }
        catch(Exception e)
        {
            print("Exception thrown " + e.Message);
        }
    }

private void UDPsend()
    {

        try
        {
           

            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 5200);
byte[] receiveBytes = client.Receive(ref remoteEndPoint);
            string receivedString = Encoding.ASCII.GetString(receiveBytes);
     print(receivedString);


            client.Connect("127.0.0.1", 5200);
            byte[] sendBytes = Encoding.ASCII.GetBytes(timeRemaining.ToString());
            client.Send(sendBytes, sendBytes.Length);

        }
        catch(Exception e)
        {
            print("Exception thrown " + e.Message);
        }
    }




}


