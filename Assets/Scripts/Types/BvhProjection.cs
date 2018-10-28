using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization;

[System.Serializable()]
public class BvhProjection {

    public int FileID { get; set; }
    public int frameNum;
    public int angle;
    public Vector3[] joints;


    public BvhProjection(int frameNumber, int projectAngle, Vector3 [] allJoints, int fileID=0)
    {
        frameNum = frameNumber;
        angle = projectAngle;
        joints = allJoints;
        FileID = fileID;
    }


    

    public string[] getJointsToString()
    {
        string[] s = new string[joints.Length];
        for(int i=0; i<s.Length; i++)
        {
            s[i] = joints[i].ToString();
        }
        return s;
    }



    /* Repositioning according to root. << IS THIS CORRECT?? */
    public void convertPositionsToRoot()
    {
        /* Joint - Root */
        Vector3 root = (joints[(int)EnumJoint.RightUpLeg] + joints[(int)EnumJoint.LeftUpLeg]) / 2;
        for (int i= 0; i < joints.Length; i++)
        {
            joints[i] = joints[i] - root;
        }
    }

    /* Scale positions, by multiplying with a scaleFactor. */
    public void scalePositions()
    {
        float scaleFactor = Scaling.getGlobalScaleFactorBVH(this); // <<<
        for (int i = 0; i < joints.Length; i++)
        {
            joints[i] *= scaleFactor;
        }
    }

    public void rotatePositions(int degrees)
    {
        for(int i=0; i<joints.Length; i++)
        {
            /* In the quaternion world, multiplication is the way 
             * to apply the rotation to something - when you multiply quaternion by vector3, 
             * you're actually rotating the vector */
            joints[i] = Quaternion.Euler(0, degrees, 0) * joints[i];
           
        }
    }


    public float Distance2D(OPPose op)
    {
        float sum = 0;
        for (int i = 0; i < op.joints.Length; i++)
        {
            Vector2 j1 = joints[i];
            Vector2 j2 = op.joints[i];
            // The joint of openpose pose might not be available,
            // due to distorted/uncompleted openpose output.
            if (!op.available[i]) continue;
            sum += Vector2.Distance(j1, j2);
        }
        return sum;
    }

    public float Distance2D(BvhProjection p)
    {
        float sum = 0;
        for (int i = 0; i < p.joints.Length; i++)
        {
            Vector2 j1 = joints[i];
            Vector2 j2 = p.joints[i];
            sum += Vector2.Distance(j1, j2);
        }
        return sum;
    }

    public float Distance3D(BvhProjection bvh)
    {
        float sum = 0;
        for (int i = 0; i < bvh.joints.Length; i++)
        {
            Vector3 j1 = joints[i];
            Vector3 j2 = bvh.joints[i];

            sum += Vector3.Distance(j1, j2);
        }
        return sum;
    }



}










public class Vector3SerializationSurrogate : ISerializationSurrogate
{

    // Method called to serialize a Vector3 object
    public void GetObjectData(System.Object obj, SerializationInfo info, StreamingContext context)
    {

        Vector3 v3 = (Vector3)obj;
        info.AddValue("x", v3.x);
        info.AddValue("y", v3.y);
        info.AddValue("z", v3.z);
    }

    // Method called to deserialize a Vector3 object
    public System.Object SetObjectData(System.Object obj, SerializationInfo info,
                                       StreamingContext context, ISurrogateSelector selector)
    {

        Vector3 v3 = (Vector3)obj;
        v3.x = (float)info.GetValue("x", typeof(float));
        v3.y = (float)info.GetValue("y", typeof(float));
        v3.z = (float)info.GetValue("z", typeof(float));
        obj = v3;
        return obj;
    }
}

