using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Winterdust; // https://winterdust.itch.io/bvhimporterexporter
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.EventSystems;
using System.Runtime.Serialization;
using System.Globalization;

public class BvhReader
{

    private string bvhDirectory;
    public static List<BVH> bvhList = new List<BVH>();
    public static List<List<Vector3[]>> bvhRotations = new List<List<Vector3[]>>();
    private int degrees;
    private float importPercentage;
    string logFileString = "";


    public BvhReader(int d, int imperce, string dir)
    {
        degrees = d;
        importPercentage = imperce;
        bvhDirectory = dir;
    }






   
    public List<BvhProjection> parseBvhProjections(string bvhFileName)
    {
        List<BvhProjection> projections = new List<BvhProjection>();
        GameObject skeletonGO;
        BVH bvh = new BVH(bvhFileName, importPercentage);
        bvhList.Add(bvh);
        /* Log the bvh info. */
        logFileString += bvh.ToString() + "\n";

        int endFrame = bvh.frameCount;

        // Get the order of selected joints
        int[] order = getOrderOfJoints(bvh);


        //skeletonGO = bvh.makeDebugSkeleton(true, "00ff00");

        float scaleFactor = getScaleFactor(bvh, order);
        bvh.scale(scaleFactor);
        // << is there a function to reposition the bvh, and flatten it?

        /* We do NOT need them, because we use convertPositionsToRoot function */
        bvh.center();
        bvh.flattenAnimationForward()
            .flattenAnimationRight()
            .flattenAnimationUp();


        // Debug frame
        StringBuilder d = new StringBuilder();
        d.AppendLine(bvhFileName);
        int beginFrame = 0;
        for (int i = beginFrame; i < endFrame; i++)
        {
            Matrix4x4 matrix;
            int rotateByAngle = 0;
            /* Rotate by degrees, 360/degrees times. */
            for (int j = 0; j < 360 / degrees; j++)
            {
                Vector3[] joints = new Vector3[Enum.GetValues(typeof(EnumJoint)).Length];
                foreach (var val in Enum.GetValues(typeof(EnumJoint)))
                {
                    int index = (int)val;
                    matrix = bvh.allBones[order[index]].getWorldMatrix(ref bvh.allBones, i);
                    // Save the position of joint
                    joints[index] = new Vector3(matrix.m03, matrix.m13, matrix.m23);
                }
                BvhProjection newProjection = new BvhProjection(0, i, rotateByAngle, joints);
                newProjection.convertPositionsToRoot();
                newProjection.rotatePositions(rotateByAngle += degrees);
                newProjection.convertPositionsToRoot(); // << This should be extra. Fix it.
                projections.Add(newProjection);
            }
        }
        return projections;
    }




    public float getScaleFactor(BVH bvh, int[] order)
    {
        Vector3[] joints = new Vector3[Enum.GetValues(typeof(EnumJoint)).Length];
        /* Set the global scale factor from frame -1, which is the T-Pose of the bvh. */
        Matrix4x4 matrix;
        foreach (var val in Enum.GetValues(typeof(EnumJoint)))
        {
            int index = (int)val;
            matrix = bvh.allBones[order[index]].getWorldMatrix(ref bvh.allBones, -1);
            // Save the position of joint
            joints[index] = new Vector3(matrix.m03, matrix.m13, matrix.m23);
        }
        BvhProjection newProjection = new BvhProjection(-1,-1, 0, joints);
        return Scaling.getGlobalScaleFactorBVH(newProjection);
    }












    public static int[] getOrderOfJoints(BVH bvh)
    {
        bool flag = false;
        int[] order = new int[Enum.GetValues(typeof(EnumJoint)).Length];
        int k = 0;
        foreach (var val in Enum.GetValues(typeof(EnumJoint)))
        {
            for (int j = 0; j < bvh.boneCount; j++)
            {
                if (bvh.allBones[j].getName().CompareTo(val.ToString()) == 0)
                {
                    // if bone found
                    order[k] = j;
                    flag = true;
                }
            }
            if (!flag)
            {
                Debug.Log(bvh.alias + " IS NOT VALID!");
                throw new Exception("bvh file has incorrect joints");
            }
            k++;
        }
        return order;
    }



    // Get the corresponding bvh from the list
    public static BVH getBvh(List<BVH> bvhlist, string bvhname)
    {
        foreach (BVH bvh in bvhlist)
        {
            //Debug.Log("Comparing...: "+bvh.alias+" "+bvhname);
            if (bvh.alias.CompareTo(bvhname) == 0)
                return bvh;
        }
        return null;
    }
    // Get the corresponding bvh from the list
    public static BVH getBvh(string bvhname)
    {
        foreach (BVH bvh in bvhList)
        {
            //Debug.Log("Comparing...: "+bvh.alias+" "+bvhname);
            if (bvh.alias.CompareTo(bvhname) == 0)
                return bvh;
        }
        return null;
    }


    //// Get window rotations
    //public static List<Vector3[]> getWindowRotations(BVH bvh, int frame, int m)
    //{
    //    List<Vector3[]> window = new List<Vector3[]>();

    //    for (int i = m; i >= -m; i--)
    //    {
    //        if (frame - i < 0 || frame - i >= bvh.frameCount)
    //            window.Add(null);
    //        else
    //        {
    //            // FROM BVH FILE
    //            // For now.. just get the index of that bvh in the list. Same index used on bvhRotations.
    //            //int index = bvhList.FindIndex(x => x.pathToBvhFileee.CompareTo(bvh.pathToBvhFileee)==0);
    //            //window.Add(bvhRotations[index][frame - i]);

    //            // FROM WINTERDUST
    //            window.Add(getRotationsfromBvh(bvh, frame - i));
    //        }
                
    //    }
    //    return window;
    //}


    // Get positions joints from bvh
    private static Vector3[] getPositionsfromBvh(BVH bvh, int frame)
    {

        // Get the order of selected joints
        int[] order = BvhReader.getOrderOfJoints(bvh);
        Vector3[] joints = new Vector3[Enum.GetValues(typeof(EnumJoint)).Length];

        foreach (var val in Enum.GetValues(typeof(EnumJoint)))
        {
            int index = (int)val;
            Matrix4x4 matrix = bvh.allBones[order[index]].getWorldMatrix(ref bvh.allBones, frame);
            // Save the position of joint
            joints[index] = new Vector3(matrix.m03, matrix.m13, matrix.m23);
        }
        return joints;
    }

    //// Get rotations using WinterDust
    //public static Vector3[] getRotationsfromBvh(BVH bvh, int frame)
    //{

    //    // Get the order of selected joints
    //    int[] order = BvhReader.getOrderOfJoints(bvh);
    //    Vector3[] rotations = new Vector3[Enum.GetValues(typeof(EnumJoint)).Length];

    //    foreach (var val in Enum.GetValues(typeof(EnumJoint)))
    //    {
    //        int index = (int)val;
    //        rotations[(int)val] = bvh.allBones[order[index]].localFrameRotations[frame].eulerAngles;
    //    }
    //    return rotations;
    //}

    /* Get rotations LIMITED using file reading. 
     * First get all rotations, and then return an array with specific order. */
    public static Vector3[] getRotationsfromBvhFile(BVH bvh, int frame)
    {
        // Get the order of selected joints.
        int[] order = BvhReader.getOrderOfJoints(bvh);
        // Get the index in bvhRotations list.
        int index = bvhList.FindIndex(x => x.pathToBvhFileee.CompareTo(bvh.pathToBvhFileee) == 0);

        Vector3[] rotations = new Vector3[Enum.GetValues(typeof(EnumJoint)).Length];

        // Select only the one we use
        for (int i = 0; i < rotations.Length; i++)
        {
            rotations[i] = bvhRotations[index][frame][order[i]];
        }

        return rotations;
    }





    ///* Set the Rotations. NEW way of getting the rotations.*/
    //public static List<List<Vector3[]>> setRotationsOfBvhFiles()
    //{

    //    foreach (BVH bvh in bvhList)
    //    {
    //        /* Returns the relative path, which is nice. */
    //        string filename = bvh.pathToBvhFileee;

    //        /* Read bvh file. */
    //        string alltext = System.IO.File.ReadAllText(filename);                  // Get the whole text.
    //        string motion = alltext.Substring(alltext.LastIndexOf("Frame Time:"));  // Skip the Hierarchy part
    //        string[] lines = File.ReadAllLines(filename);
    //        int index = 0;
    //        while (!lines[index].StartsWith("Frame Time"))
    //            index++;
    //        index++; // skip that 'Frame Time' line
    //        /* So now, index points to the first line of rotations.*/

    //        List<Vector3[]> frames = new List<Vector3[]>();
    //        int iteration = 0;
    //        while (index < lines.Length)
    //        {
    //            // From 120fps we want only 24fps
    //            //
    //            //
    //            //
    //            //
    //            // This is an issue....
    //            //
    //            //
    //            // .. Na ta kanw export apo 120fps se 24fps??
    //            // ............................
    //            Vector3[] rotations = new Vector3[bvh.boneCount];

    //            /* Get the numbers.*/
    //            string[] numText = lines[index].Split(' ');

    //            /* Skip tanslation x,y,z of root, which is 0,1,2 */
    //            float x = 0, y = 0, z = 0;
    //            int boneIndex = 0;
    //            for (int j = 3; j <= bvh.boneCount * 3; j += 3)
    //            {
    //                z = float.Parse(numText[j], CultureInfo.InvariantCulture.NumberFormat);
    //                x = float.Parse(numText[j + 1], CultureInfo.InvariantCulture.NumberFormat);
    //                y = float.Parse(numText[j + 2], CultureInfo.InvariantCulture.NumberFormat);
    //                rotations[boneIndex] = new Vector3(x, y, z);
    //                boneIndex++;
    //            }
    //            frames.Add(rotations);
    //        }
    //        bvhRotations.Add(frames);
    //    }
    //    return bvhRotations;
    //}


    public static List<Vector3> getRotations(BVH bvh, int frame)
    {
        List<Vector3> list = new List<Vector3>();
        for (int i = 0; i < bvh.boneCount; i++)
        {
            list.Add(bvh.allBones[i].localFrameRotations[frame].eulerAngles);
        }
        return list;
    }


    public static float getDistanceOfWindows(List<Vector3[]> w1, List<Vector3[]> w2)
    {
        float dist = 0;
        for (int i = 0; i < w1.Count && i < w2.Count; i++)
        {
            // This is unfair, because lets say we compare small windows hence the dist is small.
            // But if we compare big windows, the dist will be bigger.
            if (w1 == null || w2 == null || w1[i] == null || w2[i] == null)
                continue;

            // Sum up the 3D distance
            dist += distanceOfjoints(w1[i], w2[i]);
        }
        return dist;
    }



    private static float distanceOfjoints(Vector3[] joints1, Vector3[] joints2)
    {
        float sum = 0;
        for (int i = 0; i < joints1.Length; i++)
            sum += Vector3.Distance(joints1[i], joints2[i]);
        return sum;
    }



    /* Write projections in text file. NEW 06/09/18. */
    public static void writeProjectionsInFile(string filename, List<BvhProjection> projections)
    {
        string s = "";
        foreach(BvhProjection p in projections)
        {
            s+=p.frameNum +" "+ p.angle;
            foreach(Vector3 jointPosition in p.joints)
            {
                s+=jointPosition.x + " " + jointPosition.y + " " + jointPosition.z + " ";
            }
            s+="\n";
        }
        System.IO.File.WriteAllText(filename, s);
    }


}







