using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class Server : MonoBehaviour
{
    public int m_Port = 8000;
    private TcpListener m_TcpListener;
    private List<TcpClient> m_Clients = new List<TcpClient>(new TcpClient[0]);
    private Thread m_ThrdtcpListener;
    private TcpClient m_Client;

    void Start()
    {
        m_ThrdtcpListener = new Thread(new ThreadStart(ListenForIncommingRequests));
        m_ThrdtcpListener.IsBackground = true;
        m_ThrdtcpListener.Start();
    }

    void Update()
    {
        for (int i = 0; i < m_Clients.Count; i++)
        {
            if (!m_Clients[i].Connected)
                m_Clients.RemoveAt(i);

            else
                SendMessage(m_Clients[i], "서버에서 보내는 값"); // 보내는 값
        }
    }

    void OnApplicationQuit()
    {
        m_ThrdtcpListener.Abort();

        if (m_TcpListener != null)
        {
            m_TcpListener.Stop();
            m_TcpListener = null;
        }
    }

    void ListenForIncommingRequests()
    {
        m_TcpListener = new TcpListener(IPAddress.Any, m_Port);
        m_TcpListener.Start();
        ThreadPool.QueueUserWorkItem(ListenerWorker, null);
    }

    void ListenerWorker(object token)
    {
        while (m_TcpListener != null)
        {
            m_Client = m_TcpListener.AcceptTcpClient();
            m_Clients.Add(m_Client);
            ThreadPool.QueueUserWorkItem(HandleClientWorker, m_Client);
        }
    }

    void HandleClientWorker(object token)
    {
        Byte[] bytes = new Byte[1024];
        using (var client = token as TcpClient)
        using (var stream = client.GetStream())
        {
            int length;

            while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                var incommingData = new byte[length];
                Array.Copy(bytes, 0, incommingData, 0, length);
                string clientMessage = Encoding.Default.GetString(incommingData);
                Debug.Log(clientMessage); // 받은 자료
            }

            if (m_Client == null)
            {
                return;
            }
        }
    }

    void SendMessage(object token, string message)
    {
        if (m_Client == null)            
            return;

        else
            Debug.Log(m_Clients.Count);

        var client = token as TcpClient;
        {
            try
            {
                NetworkStream stream = client.GetStream();
                if (stream.CanWrite)
                {
                    byte[] serverMessageAsByteArray = Encoding.Default.GetBytes(message);
                    stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);
                }
            }

            catch (SocketException ex)
            {
                Debug.Log(ex);
                return;
            }
        }
    }        
}