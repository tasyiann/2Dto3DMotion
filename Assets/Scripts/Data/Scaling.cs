using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Winterdust;

public class Scaling {

    public enum LIMBS { LEFTARM, RIGHTARM, LEFTLEG, RIGHTLEG, LEFTHIP, RIGHTHIP }

    public static List<float> scalingLimbs = readScalingStandard();

    /* Scaling */
    public static float getGlobalScaleFactorBVH(BvhProjection projection)
    {

        Vector3[] joints = projection.joints;

        /* Find the global factor. Assume that global factor is the one that is closer to 1.0 */
        float factorLA = scalingLimbs[(int)LIMBS.LEFTARM] / Vector3.Distance(joints[2], joints[3]);
        float factorRA = scalingLimbs[(int)LIMBS.RIGHTARM] / Vector3.Distance(joints[5], joints[6]);
        float factorLL = scalingLimbs[(int)LIMBS.LEFTLEG] / Vector3.Distance(joints[8], joints[9]);
        float factorRL = scalingLimbs[(int)LIMBS.RIGHTLEG] / Vector3.Distance(joints[11], joints[12]);
        float factorLH = scalingLimbs[(int)LIMBS.LEFTHIP] / Vector3.Distance(joints[1], joints[8]);
        float factorRH = scalingLimbs[(int)LIMBS.RIGHTHIP] / Vector3.Distance(joints[1], joints[11]);
        float[] factors = { factorLA, factorRA, factorLL, factorRL, factorLH, factorRH };


        /* Find the one that is closer to 1. */
        float min = float.MaxValue; float globalFactor = 0;
        foreach (float f in factors)
        {
            float diff = Math.Abs(1 - f);
            if (diff < min)
            {
                min = diff;
                globalFactor = f;
            }
        }
        //Debug.Log("Returning " + +globalFactor);
        return globalFactor;
    }



    /* TODO : :: : :: : CHANGE THESE LEFT ARMS,RIGHTLEG,LFTHIP..*/
    public static float getGlobalScaleFactorOP(OPPose oppose)
    {
        Vector3[] joints = oppose.joints;

        /* Find the global factor. Assume that global factor is the one that is closer to 1.0 */
        float factorLA = scalingLimbs[(int)LIMBS.LEFTARM] / Vector3.Distance(joints[2], joints[3]);
        float factorRA = scalingLimbs[(int)LIMBS.RIGHTARM] / Vector3.Distance(joints[5], joints[6]);
        float factorLL = scalingLimbs[(int)LIMBS.LEFTLEG] / Vector3.Distance(joints[8], joints[9]);
        float factorRL = scalingLimbs[(int)LIMBS.RIGHTLEG] / Vector3.Distance(joints[11], joints[12]);
        float factorLH = scalingLimbs[(int)LIMBS.LEFTHIP] / Vector3.Distance(joints[1], joints[8]);
        float factorRH = scalingLimbs[(int)LIMBS.RIGHTHIP] / Vector3.Distance(joints[1], joints[11]);
        float[] factors = { factorLA, factorRA, factorLL, factorRL, factorLH, factorRH };

        /* Find the one that is closer to 1. */
        float min = float.MaxValue; float globalFactor = 0;
        foreach (float f in factors)
        {
            float diff = Math.Abs(1 - f);
            if (diff < min)
            {
                min = diff;
                globalFactor = f;
            }
        }
        return globalFactor;
    }


    public static void setNewScalingStandard(Vector3[] joints)
    {
        string fileName = "SkeletonStandard\\scaling.txt";

        List<float> limbs = new List<float>();

        limbs.Add(Vector3.Distance(joints[2], joints[3]));
        limbs.Add(Vector3.Distance(joints[5], joints[6]));
        limbs.Add(Vector3.Distance(joints[8], joints[9]));
        limbs.Add(Vector3.Distance(joints[11], joints[12]));
        limbs.Add(Vector3.Distance(joints[1], joints[8]));
        limbs.Add(Vector3.Distance(joints[1], joints[11]));

        string s = "";
        foreach (float limb in limbs)
            s += limb + "\n";
        //s += factorLA + " " + factorRA + " " + factorLL + " " + factorRL + " " + factorLH + " " + factorRH;
        System.IO.File.WriteAllText(@fileName, s);

        Debug.Log("New skeleton standard has been set, in "+fileName);
    }

    /* Reads from file the standard scaling. */
    public static List<float> readScalingStandard()
    {
        string fileName = "SkeletonStandard\\scaling.txt";
        List<float> limbs = new List<float>();

        using (TextReader reader = File.OpenText(fileName))
        {
            string line;
            while( (line = reader.ReadLine())!=null )
                limbs.Add(float.Parse(line));
        }
        Debug.Log("Scaling standards have been set.");
        return limbs;
    }





    /* Euclidean Distance. */
    private static float euclideanDistance(Vector2 a, Vector2 b)
    {
        float e = (float)Math.Sqrt(Math.Pow(a.x - b.x, 2) + Math.Pow(a.y - b.y, 2));
        return e;
    }
}
