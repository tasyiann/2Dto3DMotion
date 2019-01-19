using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Each Person is a defined person in video, that carries the frames he appears in video. </summary>
public class Person {

    public int id;                                      
    public List<OPPose> figures = new List<OPPose>();     // Poses in ascending order accoring to timeline.

    public Person(int idNum)
    {
        id = idNum;
    }

    public override string ToString()
    {
        string s = "";
       foreach(OPPose pose in figures)
        {
            s += "Person id: " + id + "\n" + pose + "\n----\n";
        }
        return s;
    }

}
