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
    private float timeCount = 0.0f;

    //int[,] pairs = new int[14, 2] { { 0, 0 }, { 0, 0 }, { 2, 3 }, { 3, 4 }, { 0, 0 },
    //    { 5, 6 }, { 6, 7 }, { 0, 0 }, { 8, 9 }, { 9, 10 }, { 1, 2 }, { 11, 12 }, { 12, 13 }, {1,2} };


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
    public void moveSkeleton(Vector3[] newJointsPositions)
    {
        Quaternion[] rotations = calculateRawRotations(newJointsPositions);  // Calculate Raw Rotations.
        hips.transform.rotation = getHips(newJointsPositions);               // Set Hips Rotation.
        for(int i=0; i<rotations.Length; i++)                                // Set Rotations in joints.
        {
            if (rotations[i] == Quaternion.identity)                         // Skip, if rotation is identity.
                continue;
            joints[i].rotation = rotations[i];
        }
    }

  
    public void moveSkeleton_OneEuroFilter(Vector3[] newJointsPositions, OneEuroFilter<Quaternion>[] rotationFiltersJoints, OneEuroFilter<Quaternion> rotationFilterHips)
    {
        Quaternion[] rotations = calculateRawRotations(newJointsPositions);                             // Calculate Raw Rotations.
        hips.transform.rotation = rotationFilterHips.Filter(getHips(newJointsPositions));               // Set Hips Rotation.
        for (int i = 0; i < rotations.Length; i++)                           // Set Rotations in joints.
        {
            if (rotations[i] == Quaternion.identity)                         // Skip, if rotation is identity.
                continue;
            joints[i].rotation = rotationFiltersJoints[i].Filter(rotations[i]);
        }
    }


    public void moveSkeleton_IK_POSITIONS(Vector3[] newJointsPositions, OneEuroFilter<Vector3>[] positionsFiltersJoints, OneEuroFilter<Quaternion> rotationFilterHips)
    {
        hips.transform.rotation = rotationFilterHips.Filter(getHips(newJointsPositions));    // Set Hips Rotation.
        for (int i = 0; i < joints.Count; i++)                                               // Set positions in joints.
        {
            joints[i].position = positionsFiltersJoints[i].Filter(newJointsPositions[i]+hips.transform.position);
        }
    }


    public void moveSkeletonLERP(Vector3[] newJointsPositions)
    {

        /* Save current rotations. */
        Quaternion curr_hipsRot = hips.transform.rotation;
        List<Quaternion> curr_jointsRot = new List<Quaternion>();
        foreach (var val in Enum.GetValues(typeof(EnumJoint)))
        {
            curr_jointsRot.Add(joints[(int)val].rotation);
        }
        
        /* Set rotations with LERP. */
        Quaternion[] rotations = calculateRawRotations(newJointsPositions);                            // Calculate Raw Rotations.
        hips.transform.rotation = Quaternion.Lerp(curr_hipsRot,getHips(newJointsPositions),timeCount); // Set Hips Rotation with LEPR.
        for (int i = 0; i < rotations.Length; i++)                                                     // Set Rotations in joints with LERP.
        {
            if (rotations[i] == Quaternion.identity)                                                   // Skip, if rotation is identity.
                continue;
            joints[i].rotation = Quaternion.Lerp(curr_jointsRot[i],rotations[i],timeCount);
        }

        /* Keep track of time, because of Lerp. */
        timeCount = timeCount + Time.deltaTime;
        timeCount = Mathf.Clamp01(timeCount);
        if (timeCount == 1) timeCount = 0;
    }


    public Quaternion[] calculateRawRotations(Vector3[] newJoints)
    {
        Quaternion[] rotations = new Quaternion[14];
        /* HEAD           */ rotations[0] = Quaternion.identity;
        /* SPINE          */ rotations[1] = Quaternion.identity;
        /* RIGHT_ARM      */ rotations[2] = XLookRotation(newJoints[3] - newJoints[2], Vector3.up);
        /* RIGHT_FORE_ARM */ rotations[3] = XLookRotation(newJoints[4] - newJoints[3], Vector3.up);
        /* RIGHT_HAND     */ rotations[4] = Quaternion.identity;
        /* LEFT_ARM       */ rotations[5] = XLookRotation(-(newJoints[6] - newJoints[5]), Vector3.up);
        /* LEFT_FORE_ARM  */ rotations[6] = XLookRotation(-(newJoints[7] - newJoints[6]), Vector3.up);
        /* LEFT_HAND      */ rotations[7] = Quaternion.identity;
        /* RIGHT_UP_LEG   */ rotations[8] = Quaternion.FromToRotation(Vector3.down, (newJoints[9] - newJoints[8]).normalized);
        /* RIGHT_LEG      */ rotations[9] = Quaternion.FromToRotation(Vector3.down, (newJoints[10] - newJoints[9]).normalized);
        /* RIGHT_FOOT     */ rotations[10] = XLookRotation((newJoints[2] - newJoints[1]), Vector3.up);
        /* LEFT_UPLEG     */ rotations[11] = Quaternion.FromToRotation(Vector3.down, (newJoints[12] - newJoints[11]).normalized);
        /* LEFT_LEG       */ rotations[12] = Quaternion.FromToRotation(Vector3.down, (newJoints[13] - newJoints[12]).normalized);
        /* LEFT_FOOT      */ rotations[13] = joints[10].rotation;
        return rotations;
    }



    private Quaternion getHips(Vector3[] joints)
    {
        // Get Root:
        Vector3 root = (joints[8] + joints[11]) / 2;
        // Hips: Works fine!
        Quaternion hips_x = YLookRotation((joints[1] - root), Vector3.up);
        Quaternion hips_yz = XLookRotation(joints[8] - joints[11], Vector3.up);
        return getRotation(hips_x, hips_yz, hips_yz);
    }

































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
            s += val.ToString() + ":" + (joints[(int)val].rotation.eulerAngles)+"\n";
        }
        return s;
    }
    /*
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
    */
    public string debugOffsets()
    {
        string s = "";
        for (int i=0; i<offsets.Count; i++)
        {
            s += i + ": " +offsets[i] +"\n";
        }

        return s;
    }










}
