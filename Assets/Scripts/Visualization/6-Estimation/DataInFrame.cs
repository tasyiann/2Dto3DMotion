using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenPose.Example;
public class DataInFrame : MonoBehaviour
{
    // Please leave one as null.
    public OpenPoseUserScript realtime_script = null;
    public VMEstimation offline_script = null;
    public OPPose selectedPoseToDebug;

    private void Start()
    {
        if (realtime_script)
            selectedPoseToDebug = realtime_script.selectedPoseToDebug;
        else
            selectedPoseToDebug = offline_script.selectedPoseToDebug;
    }

    private void Update()
    {
        if (realtime_script)
            selectedPoseToDebug = realtime_script.selectedPoseToDebug;
        else
            selectedPoseToDebug = offline_script.selectedPoseToDebug;
    }
}
