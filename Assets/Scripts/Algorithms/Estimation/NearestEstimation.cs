using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Winterdust;

[System.Serializable()]
public class NearestEstimation : AlgorithmEstimation {

    public override Neighbour SetEstimation(OPPose current, int m=0, List<List<Rotations>> rotationFiles=null)
    {
        if (current.neighbours.Count != 0)
            return current.neighbours[0];
        else
            return null; 
    }

}
