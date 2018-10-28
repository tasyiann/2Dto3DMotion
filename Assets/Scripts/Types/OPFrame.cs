using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable()]
public class OPFrame {

    public static int counter = 0;
    public int number;                  // Frame number
    public List<OPPose> figures;        // List of all figures appear in frame

    public OPFrame()
    {
        number = counter++;
        figures = new List<OPPose>();
    }
}
