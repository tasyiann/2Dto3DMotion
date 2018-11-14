using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;

[System.Serializable()]
public abstract class AlgorithmSetNeighbours {


    public abstract void SetNeighbours(OPPose opPose, int k, List<List<BvhProjection>> clusters, List<BvhProjection> representatives);


    public static void sortNeighbours(List<Neighbour> neighbours)
    {
        neighbours.Sort(delegate (Neighbour a, Neighbour b)
        {
            return a.distance.CompareTo(b.distance);
        });
    }


    public static int findClusterToSearch(OPPose opPose, List<BvhProjection> representatives)
    {
        float minDist = float.MaxValue;
        int minRepr = 0;
        for(int i=0; i<representatives.Count; i++)
        {
            float dist = representatives[i].Distance2D(opPose);
            if (dist < minDist)
            {
                minDist = dist;
                minRepr = i;
            }
        }
        return minRepr;
    }
}
