using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[System.Serializable()]
public class OPPose
{
    private static int KEYPOINTS_NUMBER = 14;       // We don't care about the rest joints.
    public int positionInJson;                      // Position in Json.
    public Vector3[] joints;                        // All joints after being processed.
    public Vector3[] jointsRAW;                     // All joints raw from json. 
    public bool[] available;                        // Is the joint[] available.
    public float scaleFactor;                       // What is the scale factor of this pose.
    public List<Neighbour> neighbours;              // k-neighbours
    public Neighbour selectedN;                     // The leading neighbour.
    public Vector3 translation;
    public int clusterId;

    /* Constructor */
    public OPPose(int jsonPosition)
    {
        positionInJson = jsonPosition;
        joints = new Vector3[KEYPOINTS_NUMBER];
        jointsRAW = new Vector3[KEYPOINTS_NUMBER];
        available = new bool[KEYPOINTS_NUMBER];
        scaleFactor = 0;
        neighbours = new List<Neighbour>();
        selectedN = null;
        clusterId = Base.getNearestClusterId();
    }


    /* Sets the jsonpositions of a person, from Json file. */
    public void fillBodyPositions(Keypoints k)
    {
        /* Fill person's json positions*/
        for (int i = 0; i < KEYPOINTS_NUMBER * 3; i += 3)
        {
            float x = k.pose_keypoints_2d[i];
            float y = k.pose_keypoints_2d[i + 1] * -1.0f; // y axis is reversed
            Vector2 xy = new Vector2(x, y);
            jointsRAW[i / 3] = xy;
            /* Check if joint is available in json */
            if (x == 0 && y == 0)
                available[i / 3] = false;
            else
                available[i / 3] = true;
        }
    }




    /* Convert Position from image positions to positions from root. */
    public void convertPositionsToRoot()
    {
        /* Joint - Root */
        Vector3 rootRAW = (jointsRAW[8] + jointsRAW[11]) / 2;
        for (int i=0; i<joints.Length; i++)
        {
            if (available[i]==false)
                joints[i] = new Vector3(0, 0, 0);
            else
            {
                joints[i] = jointsRAW[i] - rootRAW;
                jointsRAW[i] = joints[i];
            }
                
        }
    }

    /* Scale positions, by multiplying with a scaleFactor. */
    public void scalePositions()
    {
        scaleFactor = Scaling.getGlobalScaleFactorOP(this);
        for(int i=0; i<joints.Length; i++)
        {
            joints[i] *= scaleFactor;
        }
    }


}
