using System.Collections;
using System.Net;
using System.Net.Sockets;
using System;
using System.IO;
using UnityEngine;
using System.Text;

public class MatlabUnitySocket
{
    // Use this for initialization
    internal Boolean socketReady = false;
    public TcpClient mySocket;
    NetworkStream theStream;
    StreamWriter theWriter;
    StreamReader theReader;
    String Host = "localhost";
    Int32 Port = 55000;

    public void setupSocket()
    {
        try
        {
            mySocket = new TcpClient(Host, Port);
            theStream = mySocket.GetStream();
            theWriter = new StreamWriter(theStream);
            socketReady = true;
        }
        catch (Exception e)
        {
            Debug.Log("Socket error: " + e);
        }
        Debug.Log("Socket has been established.");
    }
}