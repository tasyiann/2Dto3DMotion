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
    public List<Transform> JointsGameObjects;
    List<Vector3> offsets;
    GameObject hips;
    private float angle;
    private float timeCount = 0.0f;


    public Model3D(Transform model3d, float angle = 0)
    {
        model = model3d;
        JointsGameObjects = setJoints(model);
        hips = GameObject.Find(model.name + "/Hips");
        this.angle = angle;
        offsets = setOffsets();
    }

    public static void correctlyNameJoints(Transform model, string name = "")
    {
        Transform result;
        foreach (var val in Enum.GetValues(typeof(EnumJoint)))
        {
            if ((int)val == (int)EnumJoint.Spine1)
            {
                result = model.FindDeepChild(name + "Neck");
                result.name = "Neck";
            }
            else
            {
                result = model.FindDeepChild(name + val.ToString());
                result.name = val.ToString();
            }
        }
        result = model.FindDeepChild(name + "Hips");
        result.name = "Hips";
    }

    public static List<Transform> setJoints(Transform model, string name = "")
    {
        Debug.Log("Getting transforms from model...");
        List<Transform> list = new List<Transform>();
        foreach (var val in Enum.GetValues(typeof(EnumJoint)))
        {
            //Debug.Log("Searching for..: " + val.ToString());
            /* EDITED: Neck. Get the movement of neck. */
            /* OLD: We want Spine, not Spine1. Because Spine is the Parent of Spine1, and moves better the model. */
            Transform result;
            if ((int)val == (int)EnumJoint.Spine1)
                result = model.FindDeepChild(name + "Neck");
            else
                result = model.FindDeepChild(name+val.ToString());
            if (!result)
            {
                Debug.Log("Joint not found :" + name+val.ToString());
            }
            list.Add(result);
        }
        // Print out the list
        string s = "";
        foreach(Transform g in list)
        {
            s += g.name + " ";
        }
        Debug.Log(s);
        return list;
    }

    public List<Vector3> setOffsets()
    {
        List<Vector3> list = new List<Vector3>();
        foreach (Transform joint in JointsGameObjects)
        {
            list.Add(joint.transform.localPosition); 
        }
        return list;
    }

    private static Quaternion getRotation(Quaternion qX, Quaternion qY, Quaternion qZ)
    {
        return Quaternion.Euler(new Vector3(qX.eulerAngles.x, qY.eulerAngles.y, qZ.eulerAngles.z));
    }

    public void moveSkeletonOTHERAXES(Vector3[] newJointsPositions, Quaternion AxesRotation)
    {
        Quaternion[] rotations = calculateRawRotations(newJointsPositions, hips.transform);  // Calculate Raw Rotations.
        for (int i = 0; i < rotations.Length; i++)              // Set Rotations in joints.
        {
            if (rotations[i] == Quaternion.identity)            // Skip, if rotation is identity.
                continue;
            JointsGameObjects[i].rotation = rotations[i];
        }
    }

    /* Try modifying the rotations, using LookRotation function.
     * https://gamedev.stackexchange.com/questions/136712/locking-a-rotation-on-an-axisyaw-pitch-roll-based-on-a-parental-transform/136743#136743
       https://gamedev.stackexchange.com/questions/139515/lookrotation-make-x-axis-face-the-target-instead-of-z
     */
    public void moveSkeleton(Vector3[] newJointsPositions)
    {
        Quaternion[] rotations = calculateRawRotations(newJointsPositions, hips.transform);  // Calculate Raw Rotations.
        for(int i=0; i<rotations.Length; i++)                                // Set Rotations in joints.
        {
            if (rotations[i] == Quaternion.identity)                         // Skip, if rotation is identity.
                continue;
            JointsGameObjects[i].rotation = rotations[i];
        }
    }


    public void moveSkeleton_OneEuroFilter(Vector3[] newJointsPositions, OneEuroFilter<Quaternion>[] rotationFiltersJoints, OneEuroFilter<Quaternion> rotationFilterHips)
    {
        Quaternion[] rotations = calculateRawRotations(newJointsPositions, hips.transform);         // Calculate Raw Rotations.
        hips.transform.rotation = rotationFilterHips.Filter(hips.transform.rotation);               // Set Hips Rotation.

        for (int i = 0; i < rotations.Length; i++)                           // Set Rotations in joints.
        {
            if (rotations[i] == Quaternion.identity)                         // Skip, if rotation is identity.
                continue;
            JointsGameObjects[i].rotation = rotationFiltersJoints[i].Filter(rotations[i]);
        }
    }

    public void moveSkeleton_IK_POSITIONS(Vector3[] newJointsPositions, OneEuroFilter<Vector3>[] positionsFiltersJoints, OneEuroFilter<Quaternion> rotationFilterHips)
    {
        hips.transform.rotation = rotationFilterHips.Filter(hips.transform.rotation);    // Set Hips Rotation.
        for (int i = 0; i < JointsGameObjects.Count; i++)                                               // Set positions in joints.
        {
            JointsGameObjects[i].position = positionsFiltersJoints[i].Filter(newJointsPositions[i]+hips.transform.position);
        }
    }

    public void moveSkeletonLERP(Vector3[] newJointsPositions)
    {

        /* Save current rotations. */
        Quaternion curr_hipsRot = hips.transform.rotation;
        List<Quaternion> curr_jointsRot = new List<Quaternion>();
        foreach (var val in Enum.GetValues(typeof(EnumJoint)))
        {
            curr_jointsRot.Add(JointsGameObjects[(int)val].rotation);
        }

        /* Set rotations with LERP. */
        Quaternion[] rotations = calculateRawRotations(newJointsPositions, hips.transform);              // Calculate Raw Rotations.
        hips.transform.rotation = Quaternion.Lerp(curr_hipsRot, hips.transform.rotation, timeCount);     // Set Hips Rotation with LEPR.
        for (int i = 0; i < rotations.Length; i++)                                                       // Set Rotations in joints with LERP.
        {
            if (rotations[i] == Quaternion.identity)                                                     // Skip, if rotation is identity.
                continue;
            JointsGameObjects[i].rotation = Quaternion.Lerp(curr_jointsRot[i],rotations[i],timeCount);
        }

        /* Keep track of time, because of Lerp. */
        timeCount = timeCount + Time.deltaTime;
        timeCount = Mathf.Clamp01(timeCount);
        if (timeCount == 1) timeCount = 0;
    }



    public static Quaternion XLookRotation(Vector3 right, Vector3 up)
    {
        // X becomes FWD
        // Z becomes LEFT
        // Y remains UP
        Quaternion rightToForward = Quaternion.Euler(0f, -90f, 0f);
        Quaternion forwardToTarget = Quaternion.LookRotation(right, up);

        return forwardToTarget * rightToForward;
    }

    public static Quaternion YLookRotation(Vector3 up, Vector3 upwards)
    {
        // Y becomes FWD
        // Z becomes DOWN
        // X remains RIGHT
        Quaternion UpToForward = Quaternion.Euler(90f, 0f, 0f);
        Quaternion forwardToTarget = Quaternion.LookRotation(up, upwards);

        return forwardToTarget * UpToForward;
    }

    public Quaternion[] calculateRawRotations(Vector3[] newJoints, Transform hips)
    {
        // The order matters. The rotations should be applied first to the parent and then to the child.
        Vector3 root = (newJoints[8] + newJoints[11]) / 2;
        Quaternion[] rotations = new Quaternion[14];
        hips.rotation = XLookRotation(newJoints[8] - newJoints[11], -(root - newJoints[1]));
        /* HEAD           */ rotations[0]  = Quaternion.identity; // end effector.
        /* NECK           */ rotations[1]  = YLookRotation(-(newJoints[1] - newJoints[0]), JointsGameObjects[1].TransformDirection(Vector3.back));
        /* RIGHT_ARM      */ rotations[2]  = XLookRotation(newJoints[3] - newJoints[2], JointsGameObjects[2].TransformDirection(Vector3.up));
        /* RIGHT_FORE_ARM */ rotations[3]  = XLookRotation(newJoints[4] - newJoints[3], JointsGameObjects[3].TransformDirection(Vector3.up));
        /* RIGHT_HAND     */ rotations[4]  = Quaternion.identity; // end effector.
        /* LEFT_ARM       */ rotations[5]  = XLookRotation(-(newJoints[6] - newJoints[5]), JointsGameObjects[5].TransformDirection(Vector3.up));
        /* LEFT_FORE_ARM  */ rotations[6]  = XLookRotation(-(newJoints[7] - newJoints[6]), JointsGameObjects[6].TransformDirection(Vector3.up));
        /* LEFT_HAND      */ rotations[7]  = Quaternion.identity; // end effector.
        /* RIGHT_UP_LEG   */ rotations[8]  = YLookRotation(-(newJoints[9] - newJoints[8]), JointsGameObjects[8].TransformDirection(Vector3.back));
        /* RIGHT_LEG      */ rotations[9]  = YLookRotation(-(newJoints[10] - newJoints[9]), JointsGameObjects[9].TransformDirection(Vector3.back));
        /* RIGHT_FOOT     */ rotations[10] = Quaternion.identity; // end effector.
        /* LEFT_UPLEG     */ rotations[11] = YLookRotation(-(newJoints[12] - newJoints[11]), JointsGameObjects[11].TransformDirection(Vector3.back));
        /* LEFT_LEG       */ rotations[12] = YLookRotation(-(newJoints[13] - newJoints[12]), JointsGameObjects[12].TransformDirection(Vector3.back));
        /* LEFT_FOOT      */ rotations[13] = Quaternion.identity; // end effector.

        return rotations;
    }

    /*
    public Quaternion calculateHipsRotation(Vector3[] joints)
    {
        // Get Root:
        Vector3 root = (joints[8] + joints[11]) / 2;
        // Hips: Works fine!
        Quaternion hips_x = YLookRotation((joints[1] - root), Vector3.up);
        Quaternion hips_yz = XLookRotation(joints[8] - joints[11], Vector3.up);
        return getRotation(hips_x, hips_yz, hips_yz);
    }
    */

    
































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
            s += val.ToString() + ":" + (JointsGameObjects[(int)val].rotation.eulerAngles)+"\n";
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
