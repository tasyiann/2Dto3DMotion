using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCreate3DFigure : MonoBehaviour
{

    public Mesh cylinderMesh;
    public Material lineMat;
    public Create3DFigure f;
    public DataInFrame script;

    // Start is called before the first frame update
    void Start()
    {
        f = new Create3DFigure(cylinderMesh, lineMat);
    }

    // Update is called once per frame
    void Update()
    {
        if (script == null || script.selectedPoseToDebug == null || script.selectedPoseToDebug.Estimation3D == null || script.selectedPoseToDebug.Estimation3D.projection.joints.Length == 0)
            return;

        f.updateLimbs(script.selectedPoseToDebug.Estimation3D.projection.joints);
    }
}
