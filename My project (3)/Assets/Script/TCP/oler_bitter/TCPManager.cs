using System;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using UnityEngine;



public class TCPManager : MonoBehaviour
{
    //tcp
    TcpClient Client;
    string serverIP = "127.0.0.1";
    int port = 8000;
    byte[] recevBuffer;

    void Start()
    {
        CheckWpfReceive(); 
    }

    void CheckWpfReceive()
    {
        Client = new TcpClient(serverIP, port); 
        NetworkStream stream;
        stream = Client.GetStream();

        recevBuffer = new byte[14]; // "Do you hear me" 길이 = 14
        stream.Read(recevBuffer, 0, recevBuffer.Length); // stream에 있던 바이트배열 내려서 새로 선언한 바이트배열에 넣기
        string msg = Encoding.UTF8.GetString(recevBuffer, 0, recevBuffer.Length); // byte[] to string
        Debug.Log(msg);
    }
}