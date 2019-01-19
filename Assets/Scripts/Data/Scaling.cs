using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Winterdust;

public class Scaling {

    public enum LIMBS { LEFTARM, RIGHTARM, LEFTLEG, RIGHTLEG, LEFTHIP, RIGHTHIP }

    public static List<float> scalingLimbs = readScalingStandard();
    public static readonly float scalingHeightFixed = 10f;

    /// <summary>
    /// The scaling global factor is calculated by the ratio of ﬁgure’s bones 
    /// and the corresponding bones of a ﬁxed normalized skeleton. 
    /// In the equation we use bones that their length does not get deformed when being projected: 
    /// Left Arm, Right Arm, Left Body Hip, Right Body Hip.
    /// The value that is closer to 1,is set as the scaling global factor.
    /// </summary>
    /// <param name="joints">The 2D positions of the figure's joints.</param>
    /// <returns>The global scale factor</returns>
    public static float getGlobalScaleFactor_USING_LIMBS(Vector3 [] joints, float [] previousFactors = null)
    {
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

        // Interpolate result : Get average from last factors
        globalFactor = interpolateResult(globalFactor, previousFactors);
        return globalFactor;
    }

    /// <summary>
    /// The global scaling factor is calculated using the height of the figure.
    /// Finds the lowest and highest point in y-axis.
    /// </summary>
    /// <returns>The global scale factor</returns>
    public static float getGlobalScaleFactor_USING_HEIGHT(Vector3 [] joints, float [] previousFactors = null)
    {
        float yMin = float.MaxValue, yMax = float.MinValue;
        foreach (Vector3 j in joints)
        {
            if (j.y < yMin)
                yMin = j.y;
            if (j.y > yMax)
                yMax = j.y;
        }
        float height = Math.Abs(yMin - yMax);
        return interpolateResult(scalingHeightFixed/height,previousFactors);
    }


    private static float interpolateResult(float globalFactor, float[] previousFactors)
    {
        if (previousFactors == null)
            return globalFactor;
        foreach (float f in previousFactors)
        {
            globalFactor += f;
        }
        globalFactor /= (previousFactors.Length + 1);
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
}
