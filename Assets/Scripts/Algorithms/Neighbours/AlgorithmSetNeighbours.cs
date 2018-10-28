using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;

[System.Serializable()]
public abstract class AlgorithmSetNeighbours {


    public abstract void SetNeighbours(OPPose opPose, int k, int fileIDCluster, List<List<BvhProjection>> clusters);


    public static void sortNeighbours(List<Neighbour> neighbours)
    {
        neighbours.Sort(delegate (Neighbour a, Neighbour b)
        {
            return a.distance.CompareTo(b.distance);
        });
    }

}
