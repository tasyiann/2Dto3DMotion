using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Winterdust;
using System;
public class BvhBoneNames : MonoBehaviour {

	// Use this for initialization
	void Start () {
        BVH bvh = new BVH("less\\OP_86_05.bvh");
        foreach (string s1 in bvh.getBoneNames())
        {
            Debug.Log(s1);
        }
        string s = "";
        foreach(int x in getOrderOfJoints(bvh))
        {
            s += x+" ";
        }
        Debug.Log(s);
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
}
