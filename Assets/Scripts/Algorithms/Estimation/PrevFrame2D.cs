
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Winterdust;
using System;
using System.IO;
using System.Globalization;

[System.Serializable()]
public class PrevFrame2D : AlgorithmEstimation
{
    public override Neighbour SetEstimation(OPPose current, int m = 0, List<List<Rotations>> rotationFiles = null)
    {
        

        // Case 1: Zero neighbours found for this op pose.
        if (current.neighbours.Count == 0)
            return null;

        OPPose previous = current.prevFigure;


        // Case 2: Previous is null.
        if (previous == null || previous.Estimation3D == null)
            return current.neighbours[0];

        // Case 3: Previous selectedN is not null.
        // Compare every N from k-NN to the prev selected one, and get the min.

        float min = float.MaxValue;
        Neighbour minNeighbour = null;
        foreach (Neighbour n in current.neighbours)
        {
            float distance = n.projection.Distance2D(previous.Estimation3D.projection);
            // Save the minimum distance.
            if (distance < min)
            {
                min = distance;
                minNeighbour = n;
            }
        }

        return minNeighbour;
    }




}
