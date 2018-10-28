
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Winterdust;
using System;
using System.IO;
using System.Globalization;

[System.Serializable()]
public class PrevFrameWindow3D : AlgorithmEstimation
{
    public override Neighbour GetEstimation(OPPose previous, OPPose current, int m=0, List<List<Rotations>> rotationFiles=null)
    {
        // Case 1: Zero neighbours found for this op pose.
        if (current.neighbours.Count == 0)
            return null;

        // Case 2: Previous is null.
        if (previous == null || previous.selectedN == null)
            return current.neighbours[0];

        // Case 3: Previous selectedN is not null.
        // Create the window of previous.
        List<List<Vector3>> prevWindow = createWindow(rotationFiles, previous.selectedN.projection, m);

        float min = float.MaxValue;
        Neighbour minNeighbour = null;
        foreach(Neighbour n in current.neighbours)
        {
            // Create the window of n
            List<List<Vector3>> window = createWindow(rotationFiles,n.projection, m);
            float distance = distanceOfWindows(prevWindow,window);
            // Save the minimum distance.
            if (distance < min)
            {
                min = distance;
                minNeighbour = n;
            }
        }

        return minNeighbour;
    }


    private float distanceOfWindows(List<List<Vector3>> w1, List<List<Vector3>> w2)
    {
        float dist = 0;
        for (int i = 0; i < w1.Count && i < w2.Count; i++)
        {
            // If any of the window spot is null, then we can not compare that spot.
            if (w1[i] == null || w2[i] == null)
                continue;

            // For each joint, get the distance
            for (int j = 0; j < w1[i].Count; j++)
            {
                dist += DistanceRotations(w1[i][j], w2[i][j]);
            }
        }
        return dist;
    }

    private static string disp(Quaternion q)
    {
        return q.x + " " + q.y + " " + q.z + " " + q.w;
    }

    public static float DistanceRotations(Vector3 r1, Vector3 r2)
    {
        Quaternion qA = Quaternion.Euler(r1);
        Quaternion qB = Quaternion.Euler(r2);
        float normqA = norm(qA);
        float normqB = norm(qB);
        Quaternion qA_norm = new Quaternion( qA.x / normqA, qA.y / normqA, qA.z / normqA, qA.w / normqA);
        Quaternion qB_norm = new Quaternion( qB.x / normqB, qB.y / normqB, qB.z / normqB, qB.w / normqB);
        Quaternion quatinv = Quaternion.Inverse(qA);
        Quaternion multiplication = qB_norm*quatinv;
        Quaternion qlog = quatlog(multiplication); 
        float qLogNorm = norm(qlog);
        float result = Mathf.Pow(qLogNorm, 2);
        
        /*
        // Debug calculations
        string s = "";
        s += "EulA\n" + r1.x + " " + r1.y + " " + r1.z + "\n";
        s += "EulB\n" + r2.x + " " + r2.y + " " + r2.z + "\n";
        s += "QuatA\n " + disp(qA) + "\n";
        s += "QuatB\n " + disp(qB) + "\n";
        s += "QuatA.norm\n " + disp(qA) + "\n";
        s += "QuatB.norm\n " + disp(qB) + "\n";
        s += "Quatinv\n " + disp(quatinv) + "\n";
        s += "QuatMultiplication\n " + disp(multiplication) + "\n";
        s += "QuatMultiplication.Real\n not applied \n";
        s += "QuatLog\n " + disp(qlog) + "\n";
        s += "QuatLog.Real\n not applied\n";
        s += "QuatLogReal.Norm \n " + qLogNorm + "\n";
        s += " QuatLogRealNormPow2(result) \n " + result + "\n";
        Debug.Log(s);
        */
        if (!HasValue(result)) return 0;
        return result;
    }



    // Or IsNanOrInfinity
    public static bool HasValue(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }


    private static Quaternion quatlog(Quaternion q)
    {
        float alpha = Mathf.Acos(q.w);
        float sina = Mathf.Sin(alpha);
        return new Quaternion(q.x*alpha/sina, q.y*alpha/sina, q.z*alpha/sina,0);
    }

    private static float norm(Quaternion q)
    {
        return Mathf.Sqrt(Mathf.Pow(q.w, 2) + Mathf.Pow(q.x, 2) + Mathf.Pow(q.y, 2) + Mathf.Pow(q.z, 2));
    }


    private List<List<Vector3>> createWindow(List<List<Rotations>> rotationFiles,BvhProjection projection, int m)
    {
        List<List<Vector3>> window = new List<List<Vector3>>();
        int mainFrameNum = projection.frameNum;
        for (int i=m; i>=-m; i--)
        {
            // Check if that frame exists in the rotationFile.
            if (mainFrameNum - m < 0 || mainFrameNum - m >= rotationFiles[projection.FileID].Count)
                window.Add(null);
            else
                window.Add(rotationFiles[projection.FileID][mainFrameNum-m].getComparableRotations());
        }
        return window;
    }



}
