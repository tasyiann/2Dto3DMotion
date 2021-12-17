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


    public static List<Vector3> getRotations(BVH bvh, int frame)
    {
        List<Vector3> list = new List<Vector3>();
        for (int i = 0; i < bvh.boneCount; i++)
        {
            list.Add(bvh.allBones[i].localFrameRotations[frame].eulerAngles);
        }
        return list;
    }


}

