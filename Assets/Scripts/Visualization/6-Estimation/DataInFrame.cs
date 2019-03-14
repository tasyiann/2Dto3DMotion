using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenPose.Example;
public class DataInFrame : MonoBehaviour
{
    public OPPose selectedPoseToDebug;
    public OPPose previousPose;
    public List<OPPose> allPoses;
    public int personIndex;
    public int currentFrame;
}
