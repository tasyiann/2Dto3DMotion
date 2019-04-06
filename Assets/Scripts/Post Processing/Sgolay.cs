using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

// In Microsoft® Visual Studio®, add a reference to your C# project
// to the MATLAB COM object. From the Project menu, select Add Reference.
// Select the COM tab in the Add Reference dialog box.
// Select the MATLAB application.


public class Sgolay
{
    MatlabUnitySocket mySocket;
    public Sgolay(MatlabUnitySocket mySocket)
    {
        this.mySocket = mySocket;
    }

    public void ApplySgolay(string path)
    {
        if (mySocket.socketReady)
        {
            Byte[] sendBytes = Encoding.UTF8.GetBytes(path);
            mySocket.mySocket.GetStream().Write(sendBytes, 0, sendBytes.Length);
            Debug.Log("Applying Sgolay Filtering via Matlab...");
        }
        else
        {
            Debug.LogWarning("Matlab Socket is not set up.");
        }
    }

    private void executeMatlabScript(string path)
    {
        
        // Create the MATLAB instance 
        MLApp.MLApp matlab = new MLApp.MLApp();
        // Change to the directory where the function is located 
        matlab.Execute(@"cd C:\Users\VR_Lab\Desktop\Thesis\Sgolay");
        // Define the output 
        object result = null;
        // Call the MATLAB function myfunc
        matlab.Feval("myfunc", 0, out result);
        // Display result 
        object[] res = result as object[];

    }
    
}
