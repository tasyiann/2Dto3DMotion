using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Person {

    public int id;                                      // Person's id
    public List<OPPose> poses = new List<OPPose>();    // Poses in ascending order accoring to timeline.

    public Person(int idNum)
    {
        id = idNum;
    }

    public override string ToString()
    {
        string s = "";
       foreach(OPPose pose in poses)
        {
            s += "Person id: " + id + "\n" + pose + "\n----\n";
        }
        return s;
    }

}
