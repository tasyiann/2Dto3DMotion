using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;           // String
using System.Globalization;
public class CompareFiles : MonoBehaviour
{

    int max = 6;

    void Start()
    {
        string[] array1, array2;
        FileStream resultsFile = new FileStream("RESULTSDIF", FileMode.Append, FileAccess.Write);
        StreamWriter sw = new StreamWriter(resultsFile);
        StreamReader srOrig = File.OpenText("Compare\\Original");
        StreamReader srPoser = File.OpenText("Compare\\Poser");

            for (int k = 0; k < max; k++)
            {
                array1 = getArray(srOrig);
                array2 = getArray(srPoser);
                for (int i = 0; i < array1.Length && i<array2.Length; i++)
                {
                    if (array1[i].Length == 0 || array2[i].Length == 0) continue;
                    float a=0.0f, b=0.0f;
                try
                {
                    a = float.Parse(array1[i], CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    Debug.Log("In original file, in line "+k+": "+a);
                }

                try
                {
                    b = float.Parse(array2[i], CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    Debug.Log("In poser file, in line " + k + ": " + b);
                }
                 
                float diff = Mathf.Abs( a-b );
                sw.Write(diff + " ");
                }
                sw.Write("\n");
            }

        sw.Close();
    }


    private string[] getArray( StreamReader sr)
    {
        string[] array=null;
        string tuple = sr.ReadLine();
        array = tuple.Split(' ');
        return array;
    }

}
