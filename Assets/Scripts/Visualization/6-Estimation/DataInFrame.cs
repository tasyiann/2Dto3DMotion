using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenPose.Example;
public class DataInFrame : MonoBehaviour
{
    [HideInInspector]
    public OPPose selectedPoseToDebug;
    [HideInInspector]
    public OPPose previousPose;
    [HideInInspector]
    public List<OPPose> allPoses;
    [HideInInspector]
    public int personIndex;
    [HideInInspector]
    public int currentFrame;
}
