using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Winterdust;

public class Scaling {

    public static int[,] pairs = ConfigureLIMBS.pairs;
    public static EnumBONES[] bonesDEFAULT = ConfigureLIMBS.bonesToUse_A;
    public static float extraScaling = 0.40f;
    public static List<float> boneSizesDEFAULT = readScalingStandard();
    public static readonly float scalingHeightFixed = 10f;
    public struct ScaleInfo
    {
        public float scaleFactor;
        public EnumBONES bones;
        public float error;
        public ScaleInfo(float sf, EnumBONES l, float e = 0f)
        {
            scaleFactor = sf;
            bones = l;
            error = e;
        }
    }

    public static void getGlobalScaleFactor_USING_LIMBS_TESTS(Vector3[] joints, EnumBONES[] bonesTest, List<float> bonesSizesTest, int[] bonePreferences, out EnumBONES limbUsed, out float globalFactor)
    {
        //debugBoneSizes(bonesDEFAULT, boneSizesDEFAULT, joints);
        getGlobalScaleFactor_USING_LIMBS_algorithm(joints, bonesTest, bonesSizesTest, out limbUsed, out globalFactor);   // Calculate global scaling factor
        bonePreferences[(int)limbUsed]++;                                                                                // Increament counter for this bone
    }


    public static void getGlobalScaleFactor_USING_LIMBS(Vector3[] joints, out EnumBONES limbUsed, out float globalFactor, float[] previousFactors = null)
    {
        //debugBoneSizes(bonesDEFAULT,boneSizesDEFAULT,joints);
        getGlobalScaleFactor_USING_LIMBS_algorithm(joints, bonesDEFAULT, boneSizesDEFAULT ,out limbUsed, out globalFactor, previousFactors);
    }



    private static void debugBoneSizes(EnumBONES[] bones, List<float> boneSizes, Vector3[] joints)
    {
        // Debug bones
        string s = "DEFAULT BONES SIZES:\n";
        for (int i = 0; i < bones.Length; i++)
        {
            s += bones[i].ToString() + ": " + boneSizes[i] + "\n";
        }

        s += "\n";

        foreach (EnumBONES bone in bones)
        {
            // TORSO_HEIGHT : we need to calculate the distance SPINE to ROOT.
            if ((int)bone == (int)EnumBONES.TORSO_HEIGHT)
            {
                Vector3 root = (joints[8] + joints[11]) / 2;
                s += bone.ToString() + ": " + Vector3.Distance(joints[pairs[(int)bone, 0]], root) + "\n";
            }
            else
            {
                s += bone.ToString() + ": " + Vector3.Distance(joints[pairs[(int)bone, 0]], joints[pairs[(int)bone, 1]]) + "\n";
            }
        }

        Debug.Log(s);

    }




    /// <summary>
    /// The scaling global factor is calculated by the ratio of ﬁgure’s bones 
    /// and the corresponding bones of a ﬁxed normalized skeleton. 
    /// In the equation we use bones that their length does not get deformed when being projected: (see LIMBS)
    /// 
    /// The value that is closer to 1,is set as the scaling global factor.
    /// </summary>
    /// <param name="joints">The 2D positions of the figure's joints.</param>
    /// <returns>The global scale factor</returns>
    private static void getGlobalScaleFactor_USING_LIMBS_algorithm(Vector3 [] joints, EnumBONES[] bones, List<float> bonesSizes, out EnumBONES limbUsed, out float globalFactor, float [] previousFactors = null)
    {
        ScaleInfo scalingFactorInfo = calculateScaleFactor(joints,bones,bonesSizes);    // Get info of the select scaling factor
        globalFactor = scalingFactorInfo.scaleFactor;                                   // Assign scaling factor (w/out interpolation)
        limbUsed = scalingFactorInfo.bones;                                             // Assign which limb is used
        globalFactor = interpolateResult(globalFactor, previousFactors);                // Interpolate result : Get average from last factors
    }



    private static ScaleInfo calculateScaleFactor(Vector3[] joints, EnumBONES[] bones, List<float> bonesSizes)
    {
        if (joints == null || joints.Length == 0) return new ScaleInfo(0,EnumBONES.TORSO_HEIGHT);
        // A list to save the error (size difference)
        List<ScaleInfo> scalingFactorInfos = new List<ScaleInfo>();

        foreach (EnumBONES bone in bones)
        {

            // Calculate scaling factor if we select the current bone.
            float boneSize = getBoneSize(bone,joints);
            float scalingFactor = bonesSizes[(int)bone]/boneSize;
            float diff = errorInScaling(joints,bones,bonesSizes,scalingFactor);
            scalingFactorInfos.Add(new ScaleInfo(scalingFactor, bone, diff));
        }

        // Select the one with the minimum error.
        float min = float.MaxValue; ScaleInfo minInfo = new ScaleInfo();
        foreach(ScaleInfo scale in scalingFactorInfos)
        {
            if (scale.error < min)
            {
                min = scale.error;
                minInfo = scale;
            }
        }
        // Debug.Log(scalingFactorInfos[0].bones.ToString()+": "+scalingFactorInfos[0].error+"\n"+ scalingFactorInfos[6].bones.ToString() + ": " + scalingFactorInfos[6].error);
        // return scalingFactorInfos[0];
        return minInfo;
    }


    private static float getBoneSize(EnumBONES bone, Vector3[] joints)
    {
        // Starting and ending joints of the bone.
        if (joints.Length <= pairs[(int)bone, 0])
            return 0;

        Vector3 jointA = joints[pairs[(int)bone, 0]];
        Vector3 jointB;

        // Calculate root, which is the ending bone of Torso Height.
        if ((int)bone == (int)EnumBONES.TORSO_HEIGHT)
            jointB = (joints[8] + joints[11]) / 2;
        else
            jointB = joints[pairs[(int)bone, 1]];

        // Calculate scaling factor if we select the current bone.
        return Vector3.Distance(jointA, jointB);
    }



    private static float errorInScaling(Vector3[] joints, EnumBONES[] bones, List<float> bonesSizes, float scalingFactor)
    {
        float errorSum = 0f;
        foreach(EnumBONES bone in bones)
        {
            //int b= (int)bone;
            //if (b >= 1 && b <= 5) continue; // except from 0 and 6 (UP_TORSO && TORSO_HEIGHT)

            // Starting and ending joints of the bone.
            Vector3 jointA = joints[pairs[(int)bone, 0]];
            Vector3 jointB;

            // Calculate root, which is the ending bone of Torso Height.
            if ((int)bone == (int)EnumBONES.TORSO_HEIGHT)
                jointB = (joints[8] + joints[11]) / 2;
            else
                jointB = joints[pairs[(int)bone, 1]];

            // Scale the bone, according to this scaling factor.
            Vector3 jointA_scaled = jointA * scalingFactor;
            Vector3 jointB_scaled = jointB * scalingFactor;
            float scaledBoneSize = Vector3.Distance(jointA_scaled, jointB_scaled);

            // Get difference of the scaled bone, and the default one.
            errorSum = Mathf.Abs(bonesSizes[(int)bone] - scaledBoneSize);
        }
        return errorSum;
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
        float height = Mathf.Abs(yMin - yMax);
        return scalingHeightFixed/height;
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


    public static void setNewScalingStandard()
    {
        string sourcepath = "TPoseSkeleton\\TPose.bvh";
        BVH bvh = new BVH(sourcepath);
        Vector3[] joints = ProjectionManager.getPositionsfromBvh(bvh, 0);
        
        string fileName = "SkeletonStandard\\scaling.txt";

        List<float> bones = new List<float>();
        // Set bone sizes
        foreach (EnumBONES bone in (EnumBONES[]) Enum.GetValues(typeof(EnumBONES)))
        {
            // Calculate scaling factor if we select the current bone.
            float boneSize = getBoneSize(bone, joints);
            bones.Add(boneSize * extraScaling);
        }

        // Write in file.
        string s = "";
        foreach (float bone in bones)
            s += bone + "\n";
        //s += factorLA + " " + factorRA + " " + factorLL + " " + factorRL + " " + factorLH + " " + factorRH;
        System.IO.File.WriteAllText(@fileName, s);
        Debug.Log("New skeleton standard (bone sizes) has been set, in "+fileName);
    }





    public static List<float> getBonesSizes(Vector3[] joints)
    {
        // Initialize list
        List<float> boneSizes = new List<float>();
        // Calculate bones sizes
        foreach (EnumBONES bone in (EnumBONES[])Enum.GetValues(typeof(EnumBONES)))
        {
            // Calculate scaling factor if we select the current bone.
            float boneSize = getBoneSize(bone, joints);
            boneSizes.Add( boneSize );
        }
        return boneSizes;
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
































    private static ScaleInfo selectScalingFactor(List<ScaleInfo> factors)
    {
        // >> Debug to find out what the hell is going on.
        string s = "";
        foreach (ScaleInfo si in factors)
        {
            s += si.bones.ToString() + ": " + si.scaleFactor + "\n";
        }


        float min = float.MaxValue;
        ScaleInfo globalScalingFactor = new ScaleInfo();
        foreach (ScaleInfo si in factors)
        {
            float diff = Math.Abs(1 - si.scaleFactor);
            if (diff < min)
            {
                min = diff;
                globalScalingFactor = si;
            }
        }


        // >> Debug
        s += "selected one: " + globalScalingFactor.bones.ToString() + ": " + globalScalingFactor.scaleFactor + "\n";
        Debug.Log(s);

        return globalScalingFactor;
    }









    private static List<ScaleInfo> calculateAllScalingFactors(EnumBONES[] bones, Vector3[] joints)
    {
        List<ScaleInfo> factors = new List<ScaleInfo>();

        // Calculate possible global factors.
        foreach (EnumBONES bone in bones)
        {
            float scalingFactor;
            // TORSO_HEIGHT : we need to calculate the distance SPINE to ROOT.
            if ((int)bone == (int)EnumBONES.TORSO_HEIGHT)
            {
                Vector3 root = (joints[8] + joints[11]) / 2;
                scalingFactor = boneSizesDEFAULT[(int)bone] / Vector3.Distance(joints[pairs[(int)bone, 0]], root);
            }
            else
            {
                scalingFactor = boneSizesDEFAULT[(int)bone] / Vector3.Distance(joints[pairs[(int)bone, 0]], joints[pairs[(int)bone, 1]]);
            }
            factors.Add(new ScaleInfo(scalingFactor, bone));
        }
        return factors;
    }

















}


















