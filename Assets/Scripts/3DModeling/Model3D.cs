using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Winterdust;

/* Extension for transform. */
public static class TransformDeepChildExtension
{
    //Breadth-first search
    public static Transform FindDeepChild(this Transform aParent, string aName)
    {
        var result = aParent.Find(aName);
        if (result != null)
            return result;
        foreach (Transform child in aParent)
        {
            result = child.FindDeepChild(aName);
            if (result != null)
                return result;
        }
        return null;
    }
}




public class Model3D
{

    Transform model;
    List<Transform> joints;

    List<Vector3> offsets;
    GameObject hips;
    private float angle;

    int[,] pairs = new int[14, 2] { { 0, 0 }, { 0, 0 }, { 2, 3 }, { 3, 4 }, { 0, 0 },
        { 5, 6 }, { 6, 7 }, { 0, 0 }, { 8, 9 }, { 9, 10 }, { 1, 2 }, { 11, 12 }, { 12, 13 }, {1,2} };


    // int [,] bvhToModel = new int[] { }

    public Model3D(Transform model3d, float angle=0)
    {
        model = model3d;
        joints = setJoints();
        hips = GameObject.Find(model.name+"/Hips");
        this.angle = angle;
        offsets = setOffsets();
        //Debug.Log(debugOffsets());
    }

    private List<Transform> setJoints()
    {
        List<Transform> list = new List<Transform>();
        foreach (var val in Enum.GetValues(typeof(EnumJoint)))
        {
            //Debug.Log("Searching for..: " + val.ToString());
            /* EDITED: Neck. Get the movement of neck. */
            /* OLD: We want Spine, not Spine1. Because Spine is the Parent of Spine1, and moves better the model. */
            Transform result;
            if (val.ToString().CompareTo("Spine1") == 0)
            {
                result = model.FindDeepChild("Neck");
            }
          
            else
            {
                result = model.FindDeepChild(val.ToString());
            }

            if (!result)
            {
                Debug.Log("THE JOINT IS NULL >>>>>>>>>>>>>>>>>>>>>>>>>" + val.ToString());
            }
            else
            {
                //Debug.Log("JOINT OK <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<" + val.ToString());
            }
            list.Add(result);
            //Debug.Log("GO: " + val.ToString());
        }
        return list;
    }






    public List<Vector3> setOffsets()
    {
        List<Vector3> list = new List<Vector3>();
        foreach (Transform joint in joints)
        {
            list.Add(joint.transform.localPosition); 
        }
        return list;
    }




    private Quaternion getRotation(Quaternion qX, Quaternion qY, Quaternion qZ)
    {
        return Quaternion.Euler(new Vector3(qX.eulerAngles.x, qY.eulerAngles.y, qZ.eulerAngles.z));
    }


    public static Quaternion XLookRotation(Vector3 right, Vector3 up)
    {

        Quaternion rightToForward = Quaternion.Euler(0f, -90f, 0f);
        Quaternion forwardToTarget = Quaternion.LookRotation(right, up);

        return forwardToTarget * rightToForward;
    }




    public static Quaternion YLookRotation(Vector3 up, Vector3 upwards)
    {

        Quaternion UpToForward = Quaternion.Euler(90f, 0f, 0f); // Correct, it's 90 degrees to make the positive y, into positive forward.
        Quaternion forwardToTarget = Quaternion.LookRotation(up, upwards);

        return forwardToTarget * UpToForward;
    }





    /* Try modifying the rotations, using LookRotation function.
     * https://gamedev.stackexchange.com/questions/136712/locking-a-rotation-on-an-axisyaw-pitch-roll-based-on-a-parental-transform/136743#136743
       https://gamedev.stackexchange.com/questions/139515/lookrotation-make-x-axis-face-the-target-instead-of-z
     */
    public void moveSkeleton(Vector3[] newJoints)
    {

        // Get Root:
        Vector3 root = (newJoints[8] + newJoints[11]) / 2;

        // Hips: Works fine!
        Quaternion hips_x = YLookRotation((newJoints[1] - root), Vector3.up);
        Quaternion hips_yz = XLookRotation(newJoints[8] - newJoints[11],Vector3.up);
        hips.transform.rotation = getRotation(hips_x, hips_yz, hips_yz ); 

        //Neck: Not yet figure out how.
        //joints[1].rotation = YLookRotation((newJoints[0]-newJoints[1]), Vector3.up);
        //joints[1].rotation = Quaternion.FromToRotation(Vector3.up, newJoints[0]-newJoints[1]);

        // Arms: Look okay.
        joints[2].rotation = XLookRotation(newJoints[3] - newJoints[2], Vector3.up);
        joints[3].rotation = XLookRotation(newJoints[4] - newJoints[3], Vector3.up);
        
        joints[5].rotation = XLookRotation(-(newJoints[6] - newJoints[5]), Vector3.up);
        joints[6].rotation = XLookRotation(-(newJoints[7] - newJoints[6]), Vector3.up);


        // RightUpLeg: They need to be fixed. Use LookRotation please.
        // Just keep this for now:
        joints[8].rotation = Quaternion.FromToRotation(Vector3.down, (newJoints[9] - newJoints[8]).normalized);
        //joints[8].rotation = YLookRotation(-(newJoints[9] - newJoints[8]), Vector3.up);


        // RightLeg:
        //joints[9].rotation = Quaternion.FromToRotation(Vector3.down, (newJoints[10] - newJoints[9]).normalized);
        //Quaternion rx = Quaternion.FromToRotation(offsets[9], (newJoints[10] - newJoints[9]).normalized);
        //joints[9].rotation = getRotation(rx, Quaternion.identity, Quaternion.identity);
        joints[9].rotation = Quaternion.FromToRotation(Vector3.down, (newJoints[10] - newJoints[9]).normalized);


        // LeftUpLeg:
        joints[11].rotation = Quaternion.FromToRotation(Vector3.down, (newJoints[12] - newJoints[11]).normalized);
        //Quaternion joint11onxz = Quaternion.FromToRotation(Vector3.down, (newJoints[12] - newJoints[11]).normalized);
        //joints[11].rotation = getRotation(joint11onxz, Quaternion.identity, joint11onxz);

        //LeftLeg:
        joints[12].rotation = Quaternion.FromToRotation(Vector3.down, (newJoints[13] - newJoints[12]).normalized);


        // Feet: They Look okay.
        joints[10].rotation = XLookRotation((newJoints[2]-newJoints[1]),Vector3.up);
        joints[13].rotation = joints[10].rotation;
    }



    /// <summary>
    /// Used in BvhExport. We want to find a way of getting the root rotation.
    /// </summary>
    /// <param name="joints"></param>
    /// <returns></returns>
    public static Vector3 getRootRotation_Euler(Vector3 [] joints)
    {
        // Hips: Works fine!
        // Get Root:
        Vector3 rootPosition = (joints[8] + joints[11]) / 2;
        var x = YLookRotation((joints[1] - rootPosition), Vector3.up);
        var y = XLookRotation(joints[8] - joints[11], Vector3.up);
        var z = y;

        Vector3 xyz = new Vector3(x.eulerAngles.x, y.eulerAngles.y, z.eulerAngles.z);
        return xyz;
        //return BvhToUnityRotation(xyz,AxisOrder.ZYX);
    }

    public enum AxisOrder
    {
        XYZ, XZY, YXZ, YZX, ZXY, ZYX, None
    }


    static public Quaternion BvhToUnityRotation(Vector3 eulerAngles, AxisOrder rotationOrder)
    {
        // BVH's x+ axis is Unity's left (x-)
        var xRot = Quaternion.AngleAxis(-eulerAngles.x, Vector3.left);
        // Unity & BVH agree on the y & z axes
        var yRot = Quaternion.AngleAxis(-eulerAngles.y, Vector3.up);
        var zRot = Quaternion.AngleAxis(-eulerAngles.z, Vector3.forward);

        switch (rotationOrder)
        {
            // Reproduce rotation order (no need for parentheses - it's associative)
            case AxisOrder.XYZ: return xRot * yRot * zRot;
            case AxisOrder.XZY: return xRot * zRot * yRot;
            case AxisOrder.YXZ: return yRot * xRot * zRot;
            case AxisOrder.YZX: return yRot * zRot * xRot;
            case AxisOrder.ZXY: return zRot * xRot * yRot;
            case AxisOrder.ZYX: return zRot * yRot * xRot;
        }

        return Quaternion.identity;
    }


    private float timeCount = 0.0f;


    public void moveSkeletonLERP(Vector3[] newJoints)
    {
        //float timeCount = 0.0f;
        // Get Root:
        Vector3 root = (newJoints[8] + newJoints[11]) / 2;

        // Hips: Works fine!
        Quaternion hips_x = YLookRotation((newJoints[1] - root), Vector3.up);
        Quaternion hips_yz = XLookRotation(newJoints[8] - newJoints[11], Vector3.up);

        
        /* Set the rotations. */
        Quaternion hipsRot = hips.transform.rotation;
        List<Quaternion> jointsRot = new List<Quaternion>();
        foreach (var val in Enum.GetValues(typeof(EnumJoint)))
        {
            jointsRot.Add(joints[(int)val].rotation);
        }



        hips.transform.rotation = Quaternion.Lerp(hipsRot,getRotation(hips_x, hips_yz, hips_yz),timeCount);

        //Neck: Not yet figure out how.
        //joints[1].rotation = YLookRotation((newJoints[0]-newJoints[1]), Vector3.up);
        //joints[1].rotation = Quaternion.FromToRotation(Vector3.up, newJoints[0]-newJoints[1]);

        // Arms: Look okay.
        
        joints[2].rotation = Quaternion.Lerp(jointsRot[2],XLookRotation(newJoints[3] - newJoints[2], Vector3.up),timeCount);
        joints[3].rotation = Quaternion.Lerp(jointsRot[3],XLookRotation(newJoints[4] - newJoints[3], Vector3.up),timeCount);

        joints[5].rotation = Quaternion.Lerp(jointsRot[5],XLookRotation(-(newJoints[6] - newJoints[5]), Vector3.up),timeCount);
        joints[6].rotation = Quaternion.Lerp(jointsRot[6], XLookRotation(-(newJoints[7] - newJoints[6]), Vector3.up), timeCount) ;

     
        // RightUpLeg: They need to be fixed. Use LookRotation please.
        // Just keep this for now:
        joints[8].rotation = Quaternion.Lerp(jointsRot[8],Quaternion.FromToRotation(Vector3.down, (newJoints[9] - newJoints[8]).normalized),timeCount);
        //joints[8].rotation = YLookRotation(-(newJoints[9] - newJoints[8]), Vector3.up);


        // RightLeg:
        //joints[9].rotation = Quaternion.FromToRotation(Vector3.down, (newJoints[10] - newJoints[9]).normalized);
        //Quaternion rx = Quaternion.FromToRotation(offsets[9], (newJoints[10] - newJoints[9]).normalized);
        //joints[9].rotation = getRotation(rx, Quaternion.identity, Quaternion.identity);
        joints[9].rotation = Quaternion.Lerp(jointsRot[9],Quaternion.FromToRotation(Vector3.down, (newJoints[10] - newJoints[9]).normalized),timeCount);


        // LeftUpLeg:
        joints[11].rotation = Quaternion.Lerp(jointsRot[11],Quaternion.FromToRotation(Vector3.down, (newJoints[12] - newJoints[11]).normalized),timeCount);
        //Quaternion joint11onxz = Quaternion.FromToRotation(Vector3.down, (newJoints[12] - newJoints[11]).normalized);
        //joints[11].rotation = getRotation(joint11onxz, Quaternion.identity, joint11onxz);

        //LeftLeg:
        joints[12].rotation = Quaternion.Lerp(jointsRot[12],Quaternion.FromToRotation(Vector3.down, (newJoints[13] - newJoints[12]).normalized),timeCount);


        // Feet: They Look okay.
        joints[10].rotation = Quaternion.Lerp(jointsRot[10],XLookRotation((newJoints[2] - newJoints[1]), Vector3.up),timeCount);
        joints[13].rotation = joints[10].rotation;
    


        timeCount = timeCount + Time.deltaTime;
        timeCount = Mathf.Clamp01(timeCount);
        if (timeCount == 1) timeCount = 0;
        //Debug.Log(timeCount);
    }





    ///** Should I work with local Rotations? */
    //public void moveWithRot(Neighbour estimation)
    //{

    //    /* Get rotations of all joints, and the angle of the projection. */
    //    BVH bvh = BvhReader.getBvh(estimation.projection.bvhFileName);
    //    List<Quaternion> qlist = BvhReader.getRotations(bvh,estimation.projection.frameNum);
    //    float angle = estimation.projection.angle;

    //    /* Get the current rotations of the model, to do the lerp. */
    //    Quaternion hipsCurrRot = hips.transform.localRotation;
    //    List<Quaternion> jointsCurrRot = new List<Quaternion>();
    //    foreach (var val in Enum.GetValues(typeof(EnumJoint)))
    //    {
    //        jointsCurrRot.Add(joints[(int)val].localRotation);
    //    }

    //    // 1st step: Test just the root rotation.
    //   // hips.transform.rotation = Quaternion.Lerp(hipsCurrRot, qlist[0], timeCount );

    //    // Ok.. hips does not work
    //    // Check arms
    //    joints[(int)EnumJoint.LeftArm].transform.localRotation = Quaternion.Lerp(jointsCurrRot[(int)EnumJoint.LeftArm], qlist[5],timeCount);
    //    joints[(int)EnumJoint.RightArm].transform.localRotation = Quaternion.Lerp(jointsCurrRot[(int)EnumJoint.RightArm], qlist[8], timeCount);

    //    joints[(int)EnumJoint.LeftForeArm].transform.localRotation = Quaternion.Lerp(jointsCurrRot[(int)EnumJoint.LeftForeArm], qlist[6], timeCount);
    //    joints[(int)EnumJoint.RightForeArm].transform.localRotation = Quaternion.Lerp(jointsCurrRot[(int)EnumJoint.RightForeArm], qlist[9], timeCount);

    //    joints[(int)EnumJoint.RightHand].transform.localRotation = Quaternion.Lerp(jointsCurrRot[(int)EnumJoint.RightHand], qlist[10], timeCount);
    //    joints[(int)EnumJoint.LeftHand].transform.localRotation = Quaternion.Lerp(jointsCurrRot[(int)EnumJoint.LeftHand], qlist[7], timeCount);


    //    /*
    //    for (int i=0; i<14; i++)
    //    {
    //        Vector3 eul = rotations[i].eulerAngles;
    //        Quaternion newrot = Quaternion.Euler(eul.x, eul.y + angle, eul.z);
    //        joints[i].rotation = Quaternion.Lerp(jointsCurrRot[i],newrot,timeCount);
    //    }
    //   */

    //    timeCount = timeCount + Time.deltaTime;
    //    timeCount = Mathf.Clamp01(timeCount);
    //    if (timeCount == 1) timeCount = 0;
    //}






    /*
     * https://answers.unity.com/questions/32532/how-to-find-direction-between-two-points.html
     * 
     * *
     * */
    /* New positions as input. LeftForeArm. */



    public override string ToString()
    {
        string s = "Rotations:\n";
        foreach (var val in Enum.GetValues(typeof(EnumJoint)))
        {
            s += val.ToString() + ":" + Quaternion.ToEulerAngles(joints[(int)val].rotation)+"\n";
        }
        return s;
    }

    public string debugDirection(Vector3[] newJoints)
    {
        string s = "Directions:\n";
        string[] names = Enum.GetNames(typeof(EnumJoint));
        for (int i=0; i<14; i++)
        {
            s += names[i] + ": Actual:" + (joints[pairs[i,1]].position - joints[pairs[i, 0]].position).normalized + "Ideal:" + (newJoints[pairs[i, 1]] - newJoints[pairs[i, 0]]).normalized+"\n";
        }
        return s;
    }

    public string debugOffsets()
    {
        string s = "";
        for (int i=0; i<offsets.Count; i++)
        {
            s += i + ": " +offsets[i] +"\n";
        }

        return s;
    }




    // Assign rotations to the model.
    public void AssignRotationsToSkeleton(Vector3[] rotations)
    {
        for(int i=0; i<14; i++)
        {
            joints[i].rotation = Quaternion.Euler(rotations[i]);
        }
    }

}
