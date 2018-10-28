using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Winterdust;
using System;

[System.Serializable()]
public abstract class AlgorithmEstimation {
    
    public abstract Neighbour GetEstimation(OPPose previous, OPPose current, int m = 0, List<List<Rotations>> rotationFiles = null);
}
